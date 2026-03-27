using GeneClusteringApp.Clustering;
using GeneClusteringApp.Data;

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

        Console.WriteLine("Генерация данных с 4 кластерами...");
        var data = SyntheticDataGenerator.GenerateWithClusters(
            genes: 500,
            samples: 10,
            trueClusters: 4,
            noiseLevel: 0.3);

        data.PrintInfo();

        // Запускаем k-means
        Console.WriteLine("\nЗапуск k-means с k=4...");
        var kmeans = new KMeans(k: 4, maxIterations: 50);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        int[] labels = kmeans.Cluster(data);
        stopwatch.Stop();

        Console.WriteLine($"Кластеризация завершена за {stopwatch.ElapsedMilliseconds} мс");

        // Выводим статистику по кластерам
        int[] clusterSizes = new int[4];
        for (int i = 0; i < labels.Length; i++)
        {
            clusterSizes[labels[i]]++;
        }

        Console.WriteLine("\nРазмеры кластеров:");
        for (int i = 0; i < 4; i++)
        {
            Console.WriteLine($"  Кластер {i}: {clusterSizes[i]} генов");
        }
    }


}