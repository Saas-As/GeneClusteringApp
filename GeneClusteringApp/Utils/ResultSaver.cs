using GeneClusteringApp.Data;
using GeneClusteringApp.Evaluation;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace GeneClusteringApp.Utils;

/// <summary>
/// Сохранение результатов кластеризации в файл
/// </summary>
public static class ResultSaver
{
    /// <summary>
    /// Сохраняет результаты кластеризации в CSV файл
    /// </summary>
    public static void SaveToCsv(string filePath, GeneMatrix data, int[] labels, double[][] centroids, double silhouette)
    {
        Console.WriteLine($"\nСохранение результатов в файл: {filePath}");

        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

        // Заголовок
        writer.WriteLine("# Результаты кластеризации экспрессии генов (k-means)");
        writer.WriteLine($"# Количество кластеров: {centroids.Length}");
        writer.WriteLine($"# Силуэтный коэффициент: {silhouette:F4}");
        writer.WriteLine($"# Дата: {DateTime.Now}");
        writer.WriteLine("#");
        writer.WriteLine("GeneName,Cluster");

        // Данные: ген и его кластер
        for (int g = 0; g < data.GeneCount; g++)
        {
            string geneName = data.GetGeneName(g);
            writer.WriteLine($"{geneName},{labels[g]}");
        }

        Console.WriteLine($"  Сохранено {data.GeneCount} записей");
    }

    /// <summary>
    /// Сохраняет детальный отчёт в текстовый файл
    /// </summary>
    public static void SaveReport(string filePath, GeneMatrix data, int[] labels, double[][] centroids,
        double inertia, double silhouette, int iterations, double timeMs)
    {
        Console.WriteLine($"\nСохранение отчёта в файл: {filePath}");

        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

        writer.WriteLine("==================================================");
        writer.WriteLine("ОТЧЁТ О КЛАСТЕРИЗАЦИИ ЭКСПРЕССИИ ГЕНОВ");
        writer.WriteLine("==================================================");
        writer.WriteLine();
        writer.WriteLine($"Дата: {DateTime.Now}");
        writer.WriteLine($"Алгоритм: k-means с инициализацией k-means++");
        writer.WriteLine();
        writer.WriteLine("--- ПАРАМЕТРЫ ДАННЫХ ---");
        writer.WriteLine($"Количество генов: {data.GeneCount}");
        writer.WriteLine($"Количество образцов: {data.SampleCount}");
        writer.WriteLine();
        writer.WriteLine("--- РЕЗУЛЬТАТЫ КЛАСТЕРИЗАЦИИ ---");
        writer.WriteLine($"Количество кластеров (k): {centroids.Length}");
        writer.WriteLine($"Inertia (сумма квадратов расстояний): {inertia:F2}");
        writer.WriteLine($"Silhouette Score: {silhouette:F4}");
        writer.WriteLine($"Интерпретация: {SilhouetteAnalyzer.Interpret(silhouette)}");
        writer.WriteLine($"Количество итераций: {iterations}");
        writer.WriteLine($"Время выполнения: {timeMs:F0} мс");
        writer.WriteLine();
        writer.WriteLine("--- РАЗМЕРЫ КЛАСТЕРОВ ---");

        int[] clusterSizes = new int[centroids.Length];
        for (int i = 0; i < labels.Length; i++)
            clusterSizes[labels[i]]++;

        for (int c = 0; c < centroids.Length; c++)
        {
            double percentage = (double)clusterSizes[c] / data.GeneCount * 100;
            writer.WriteLine($"Кластер {c}: {clusterSizes[c]} генов ({percentage:F1}%)");
        }

        writer.WriteLine();
        writer.WriteLine("--- ПРИМЕРЫ ГЕНОВ ПО КЛАСТЕРАМ (первые 10) ---");

        // Собираем гены по кластерам
        var genesByCluster = new System.Collections.Generic.List<string>[centroids.Length];
        for (int i = 0; i < centroids.Length; i++)
            genesByCluster[i] = new System.Collections.Generic.List<string>();

        for (int g = 0; g < data.GeneCount; g++)
            genesByCluster[labels[g]].Add(data.GetGeneName(g));

        for (int c = 0; c < centroids.Length; c++)
        {
            writer.WriteLine($"Кластер {c}:");
            var topGenes = genesByCluster[c].Take(10);
            foreach (var gene in topGenes)
                writer.WriteLine($"  - {gene}");
            if (genesByCluster[c].Count > 10)
                writer.WriteLine($"  ... и ещё {genesByCluster[c].Count - 10} генов");
        }

        writer.WriteLine();
        writer.WriteLine("==================================================");
        writer.WriteLine("КОНЕЦ ОТЧЁТА");
        writer.WriteLine("==================================================");

        Console.WriteLine("  Отчёт сохранён");
    }
}