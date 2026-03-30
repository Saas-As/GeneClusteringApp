using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GeneClusteringApp.Data;
using GeneClusteringApp.Evaluation;

namespace GeneClusteringWPFApp.Services
{
    /// <summary>
    /// Экспорт результатов кластеризации в файлы
    /// </summary>
    public static class ResultExporter
    {
        public class ExportResult
        {
            public string LabelsCsvPath { get; set; } = "";
            public string ReportTxtPath { get; set; } = "";
            public string ClusterGenesTxtPath { get; set; } = "";
            public string CentroidsCsvPath { get; set; } = "";
            public bool Success { get; set; }
            public string ErrorMessage { get; set; } = "";
        }

        /// <summary>
        /// Экспортирует все результаты в папку
        /// </summary>
        public static ExportResult ExportAll(GeneMatrix data, int[] labels, double[][] centroids,
            double inertia, double silhouette, int k, int iterations, long timeMs, string outputFolder)
        {
            var result = new ExportResult();

            try
            {
                Directory.CreateDirectory(outputFolder);

                string baseName = $"clustering_{DateTime.Now:yyyyMMdd_HHmmss}";

                // 1. CSV с метками кластеров
                result.LabelsCsvPath = Path.Combine(outputFolder, $"{baseName}_labels.csv");
                ExportLabelsToCsv(result.LabelsCsvPath, data, labels);

                // 2. CSV с центроидами
                result.CentroidsCsvPath = Path.Combine(outputFolder, $"{baseName}_centroids.csv");
                ExportCentroidsToCsv(result.CentroidsCsvPath, centroids, data.SampleCount);

                // 3. Текстовый отчёт
                result.ReportTxtPath = Path.Combine(outputFolder, $"{baseName}_report.txt");
                ExportReport(result.ReportTxtPath, data, labels, centroids, inertia, silhouette, k, iterations, timeMs);

                // 4. Гены по кластерам (для биологов)
                result.ClusterGenesTxtPath = Path.Combine(outputFolder, $"{baseName}_genes_by_cluster.txt");
                ExportGenesByCluster(result.ClusterGenesTxtPath, data, labels, k);

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Экспорт меток кластеров в CSV
        /// </summary>
        private static void ExportLabelsToCsv(string filePath, GeneMatrix data, int[] labels)
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            writer.WriteLine("GeneName,Cluster");

            for (int g = 0; g < data.GeneCount; g++)
            {
                writer.WriteLine($"{data.GetGeneName(g)},{labels[g]}");
            }
        }

        /// <summary>
        /// Экспорт центроидов в CSV
        /// </summary>
        private static void ExportCentroidsToCsv(string filePath, double[][] centroids, int sampleCount)
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

            // Заголовок
            writer.Write("Cluster");
            for (int s = 0; s < sampleCount; s++)
                writer.Write($",Sample_{s + 1}");
            writer.WriteLine();

            // Данные
            for (int c = 0; c < centroids.Length; c++)
            {
                writer.Write($"{c}");
                for (int s = 0; s < sampleCount; s++)
                    writer.Write($",{centroids[c][s]:F6}");
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Экспорт полного отчёта
        /// </summary>
        private static void ExportReport(string filePath, GeneMatrix data, int[] labels, double[][] centroids,
            double inertia, double silhouette, int k, int iterations, long timeMs)
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

            writer.WriteLine("=".PadRight(60, '='));
            writer.WriteLine("ОТЧЁТ О КЛАСТЕРИЗАЦИИ ЭКСПРЕССИИ ГЕНОВ");
            writer.WriteLine("=".PadRight(60, '='));
            writer.WriteLine();
            writer.WriteLine($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            writer.WriteLine($"Алгоритм: k-means с инициализацией k-means++");
            writer.WriteLine();

            writer.WriteLine("--- ПАРАМЕТРЫ ДАННЫХ ---");
            writer.WriteLine($"Количество генов: {data.GeneCount}");
            writer.WriteLine($"Количество образцов: {data.SampleCount}");
            writer.WriteLine();

            writer.WriteLine("--- РЕЗУЛЬТАТЫ КЛАСТЕРИЗАЦИИ ---");
            writer.WriteLine($"Количество кластеров (k): {k}");
            writer.WriteLine($"Inertia (сумма квадратов расстояний): {inertia:F2}");
            writer.WriteLine($"Silhouette Score: {silhouette:F4}");
            writer.WriteLine($"Интерпретация: {SilhouetteAnalyzer.Interpret(silhouette)}");
            writer.WriteLine($"Количество итераций: {iterations}");
            writer.WriteLine($"Время выполнения: {timeMs} мс ({timeMs / 1000.0:F1} сек)");
            writer.WriteLine();

            writer.WriteLine("--- РАЗМЕРЫ КЛАСТЕРОВ ---");
            int[] clusterSizes = new int[k];
            for (int i = 0; i < labels.Length; i++)
                clusterSizes[labels[i]]++;

            for (int c = 0; c < k; c++)
            {
                double percent = (double)clusterSizes[c] / data.GeneCount * 100;
                writer.WriteLine($"Кластер {c}: {clusterSizes[c]} генов ({percent:F1}%)");
            }
            writer.WriteLine();

            writer.WriteLine("--- ЦЕНТРОИДЫ КЛАСТЕРОВ (первые 5 образцов) ---");
            for (int c = 0; c < Math.Min(k, 10); c++)
            {
                writer.Write($"Кластер {c}: [");
                var samples = centroids[c].Take(5).Select(v => v.ToString("F3"));
                writer.Write(string.Join(", ", samples));
                if (centroids[c].Length > 5)
                    writer.Write(", ...");
                writer.WriteLine("]");
            }
            writer.WriteLine();

            writer.WriteLine("=".PadRight(60, '='));
            writer.WriteLine("КОНЕЦ ОТЧЁТА");
            writer.WriteLine("=".PadRight(60, '='));
        }

        /// <summary>
        /// Экспорт генов по кластерам (для биологов)
        /// </summary>
        private static void ExportGenesByCluster(string filePath, GeneMatrix data, int[] labels, int k)
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

            // Собираем гены по кластерам
            var genesByCluster = new List<string>[k];
            for (int i = 0; i < k; i++)
                genesByCluster[i] = new List<string>();

            for (int g = 0; g < data.GeneCount; g++)
            {
                genesByCluster[labels[g]].Add(data.GetGeneName(g));
            }

            // Сортируем по размеру кластера
            var sortedClusters = Enumerable.Range(0, k)
                .OrderByDescending(c => genesByCluster[c].Count)
                .ToList();

            foreach (var cluster in sortedClusters)
            {
                writer.WriteLine($"КЛАСТЕР {cluster} (всего {genesByCluster[cluster].Count} генов)");
                writer.WriteLine(new string('-', 40));

                // Выводим гены в колонках для удобства
                var genes = genesByCluster[cluster].OrderBy(g => g).ToList();
                int cols = 4;
                int rows = (int)Math.Ceiling(genes.Count / (double)cols);

                for (int i = 0; i < rows; i++)
                {
                    var line = new List<string>();
                    for (int j = 0; j < cols; j++)
                    {
                        int idx = j * rows + i;
                        if (idx < genes.Count)
                            line.Add(genes[idx]);
                    }
                    writer.WriteLine(string.Join("\t", line));
                }
                writer.WriteLine();
            }
        }
    }
}