using GeneClusteringApp.Clustering;
using GeneClusteringApp.Data;
using GeneClusteringApp.Evaluation;

namespace GeneClusteringApp;

class Program
{
    static void Main(string[] args)
    {
        //    // Тест 1: Создание матрицы
        //    Console.WriteLine("--- Тест 1: Создание матрицы ---");
        //    var matrix = new GeneMatrix(10, 5);
        //    matrix.PrintInfo();
        //    matrix.PrintPreview();

        //    Console.WriteLine("\n--- Тест 2: Заполнение и доступ к данным ---");
        //    // Заполняем тестовыми значениями
        //    for (int g = 0; g < matrix.GeneCount; g++)
        //    {
        //        for (int s = 0; s < matrix.SampleCount; s++)
        //        {
        //            matrix[g, s] = g * 10 + s;
        //        }
        //    }
        //    matrix.PrintPreview();

        //    Console.WriteLine("\n--- Тест 3: Получение вектора гена ---");
        //    var geneVector = matrix.GetGeneVector(3);
        //    Console.WriteLine($"Ген 4: [{string.Join(", ", geneVector)}]");

        //    Console.WriteLine("\n--- Тест 4: Синтетические данные с кластерами ---");
        //    var clusteredData = SyntheticDataGenerator.GenerateWithClusters(
        //        genes: 100,
        //        samples: 20,
        //        trueClusters: 4,
        //        noiseLevel: 0.3);
        //    clusteredData.PrintInfo();

        //    Console.WriteLine("\nПервые 10 генов с паттернами:");
        //    for (int g = 0; g < 10; g++)
        //    {
        //        var vec = clusteredData.GetGeneVector(g);
        //        // Показываем только первые 5 значений для краткости
        //        Console.WriteLine($"Ген {g}: [{string.Join(", ", vec.Take(5).Select(v => v.ToString("F2")))}...]");
        //    }

        //    Console.WriteLine("\n--- Тест 5: Данные с паттернами ---");
        //    var patternData = SyntheticDataGenerator.GenerateWithPatterns(20, 10);
        //    patternData.PrintInfo();

        //    Console.WriteLine("\nПаттерны экспрессии (первые 8 генов):");
        //    for (int g = 0; g < 8; g++)
        //    {
        //        var vec = patternData.GetGeneVector(g);
        //        Console.WriteLine($"Ген {g}: [{string.Join(", ", vec.Select(v => v.ToString("F2")))}]");
        //    }
        // Генерируем данные с известной кластерной структурой

        // ========== ТЕСТ 1: Данные с кластерами ==========
        Console.WriteLine("1. Генерация данных с 4 кластерами...");
        var data = SyntheticDataGenerator.GenerateWithClusters(
            genes: 500,
            samples: 10,
            trueClusters: 4,
            noiseLevel: 0.3);

        data.PrintInfo();

        Console.WriteLine("\n2. Запуск k-means с k=4...");
        var kmeans = new KMeans(k: 4, maxIterations: 50);
        var (labels, centroids) = kmeans.ClusterWithCentroids(data);

        Console.WriteLine("\n3. Оценка качества кластеризации:");

        double inertia = InertiaCalculator.Calculate(data, labels, centroids);
        double silhouette = SilhouetteAnalyzer.AverageSilhouette(data, labels, 4);

        Console.WriteLine($"   Inertia (сумма квадратов): {inertia:F2}");
        Console.WriteLine($"   Silhouette Score: {silhouette:F4}");
        Console.WriteLine($"   Интерпретация: {SilhouetteAnalyzer.Interpret(silhouette)}");

        // Выводим размеры кластеров
        int[] clusterSizes = new int[4];
        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] >= 0 && labels[i] < 4)
                clusterSizes[labels[i]]++;
            else
                Console.WriteLine($"  [Ошибка] Ген {i} имеет метку {labels[i]}");
        }

        Console.WriteLine("\n4. Размеры кластеров:");
        for (int i = 0; i < 4; i++)
        {
            Console.WriteLine($"   Кластер {i}: {clusterSizes[i]} генов");
        }

        // Проверяем, насколько хорошо распределены кластеры
        double expectedSize = 500.0 / 4;
        Console.WriteLine($"\n   Ожидаемый размер (равномерный): {expectedSize:F0} генов на кластер");

        // ========== ТЕСТ 2: Случайные данные ==========
        Console.WriteLine("\n5. Тест на случайных данных (без структуры):");
        var randomData = SyntheticDataGenerator.GenerateRandom(genes: 500, samples: 10);
        var (randomLabels, randomCentroids) = kmeans.ClusterWithCentroids(randomData);

        double randomSilhouette = SilhouetteAnalyzer.AverageSilhouette(randomData, randomLabels, 4);
        Console.WriteLine($"   Silhouette Score: {randomSilhouette:F4}");
        Console.WriteLine($"   Интерпретация: {SilhouetteAnalyzer.Interpret(randomSilhouette)}");
    }
}