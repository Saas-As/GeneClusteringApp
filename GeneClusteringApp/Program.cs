using GeneClusteringApp.Clustering;
using GeneClusteringApp.Data;
using GeneClusteringApp.Evaluation;
using GeneClusteringApp.Utils;
using System.Diagnostics;

namespace GeneClusteringApp;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Кластеризация экспрессии генов (k-means) ===\n");

        // ========== ВЫБОР РЕЖИМА ==========
        Console.WriteLine("Выберите режим:");
        Console.WriteLine("  1 - Тест на синтетических данных");
        Console.WriteLine("  2 - Загрузка реальных данных из CSV");
        Console.Write("\nВаш выбор: ");

        string choice = Console.ReadLine();

        if (choice == "1")
        {
            RunSyntheticTest();
        }
        else if (choice == "2")
        {
            RunRealDataTest();
        }
        else
        {
            Console.WriteLine("Неверный выбор. Запуск синтетического теста по умолчанию.");
            RunSyntheticTest();
        }
    }

    static void RunSyntheticTest()
    {
        Console.WriteLine("\n=== СИНТЕТИЧЕСКИЙ ТЕСТ ===\n");

        // Генерация данных с 4 кластерами
        var data = SyntheticDataGenerator.GenerateWithClusters(
            genes: 500,
            samples: 10,
            trueClusters: 4,
            noiseLevel: 0.3);

        data.PrintInfo();

        // Сравнение инициализации
        Console.WriteLine("\n2. Сравнение инициализации (k=4):");

        var kmeansRandom = new KMeans(k: 4, useKMeansPlusPlus: false);
        var (labelsRandom, _) = kmeansRandom.ClusterWithCentroids(data);
        double silhouetteRandom = SilhouetteAnalyzer.AverageSilhouette(data, labelsRandom, 4);

        var kmeansPP = new KMeans(k: 4, useKMeansPlusPlus: true);
        var (labelsPP, centroidsPP) = kmeansPP.ClusterWithCentroids(data);
        double silhouettePP = SilhouetteAnalyzer.AverageSilhouette(data, labelsPP, 4);

        Console.WriteLine($"   Случайная инициализация: Silhouette = {silhouetteRandom:F4}");
        Console.WriteLine($"   K-means++: Silhouette = {silhouettePP:F4}");

        // Выбор оптимального k
        var kSelection = KSelection.SelectK(data, minK: 2, maxK: 8);
        KSelection.PrintResult(kSelection);
    }

    static void RunRealDataTest()
    {
        Console.WriteLine("\n=== РЕАЛЬНЫЕ ДАННЫЕ ===\n");

        // Запрос пути к файлу
        Console.Write("Введите путь к CSV файлу: ");
        string filePath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            Console.WriteLine("Файл не найден! Проверьте путь.");
            return;
        }

        try
        {
            // Загрузка данных
            Console.WriteLine("\n1. Загрузка данных...");
            var data = CSVLoader.LoadFromCsv(filePath);
            data.PrintInfo();
            data.PrintPreview();

            // Нормализация
            Console.WriteLine("\n2. Нормализация данных...");
            Console.Write("   Выберите метод нормализации (1=Z-score, 2=MinMax, 3=Log2, 4=Без нормализации): ");
            string normChoice = Console.ReadLine();

            var normalizedData = normChoice switch
            {
                "1" => Normalizer.ZScoreNormalize(data),
                "2" => Normalizer.MinMaxNormalize(data),
                "3" => Normalizer.Log2Normalize(data),
                _ => data
            };

            // Выбор k
            Console.WriteLine("\n3. Настройка кластеризации...");
            Console.Write("   Введите k (или 0 для автоматического выбора): ");
            string kInput = Console.ReadLine();

            int k;
            if (kInput == "0")
            {
                var kSelection = KSelection.SelectK(normalizedData, minK: 2, maxK: 10);
                KSelection.PrintResult(kSelection);
                k = kSelection.RecommendedK;
            }
            else
            {
                k = int.Parse(kInput);
            }

            // Кластеризация с замером времени
            Console.WriteLine($"\n4. Запуск k-means с k={k}...");
            var kmeans = new KMeans(k, useKMeansPlusPlus: true);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var (labels, centroids) = kmeans.ClusterWithCentroids(normalizedData);
            stopwatch.Stop();

            // Получаем количество итераций
            int iterations = kmeans.LastIterationCount;

            // Оценка качества
            double inertia = InertiaCalculator.Calculate(normalizedData, labels, centroids);
            double silhouette = SilhouetteAnalyzer.AverageSilhouette(normalizedData, labels, k);

            Console.WriteLine($"\n5. Результаты кластеризации:");
            Console.WriteLine($"   Inertia: {inertia:F2}");
            Console.WriteLine($"   Silhouette: {silhouette:F4} - {SilhouetteAnalyzer.Interpret(silhouette)}");
            Console.WriteLine($"   Время выполнения: {stopwatch.ElapsedMilliseconds} мс");

            // Размеры кластеров
            int[] clusterSizes = new int[k];
            for (int i = 0; i < labels.Length; i++)
                clusterSizes[labels[i]]++;

            Console.WriteLine("\n   Размеры кластеров:");
            for (int i = 0; i < k; i++)
                Console.WriteLine($"     Кластер {i}: {clusterSizes[i]} генов");

            // Сохранение результатов
            Console.WriteLine("\n6. Сохранение результатов...");
            Console.Write("   Сохранить результаты в файл? (y/n): ");
            string saveChoice = Console.ReadLine();

            if (saveChoice?.ToLower() == "y")
            {
                Console.Write("   Введите имя файла для сохранения (без расширения): ");
                string fileName = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(fileName))
                    fileName = $"clustering_results_{DateTime.Now:yyyyMMdd_HHmmss}";

                string csvPath = $"{fileName}.csv";
                string reportPath = $"{fileName}_report.txt";

                ResultSaver.SaveToCsv(csvPath, normalizedData, labels, centroids, silhouette);
                ResultSaver.SaveReport(reportPath, normalizedData, labels, centroids, inertia, silhouette, iterations, stopwatch.ElapsedMilliseconds);

                Console.WriteLine($"\n   Результаты сохранены:");
                Console.WriteLine($"     - {csvPath} (метки кластеров)");
                Console.WriteLine($"     - {reportPath} (полный отчёт)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
}