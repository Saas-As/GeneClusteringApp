using GeneClusteringApp.Data;
using GeneClusteringApp.Clustering;
using GeneClusteringApp.Evaluation;

namespace GeneClusteringApp;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Кластеризация экспрессии генов (k-means) ===\n");

        // ========== ТЕСТ 1: Данные с кластерами ==========
        Console.WriteLine("1. Генерация данных с 4 кластерами...");
        var data = SyntheticDataGenerator.GenerateWithClusters(
            genes: 500,
            samples: 10,
            trueClusters: 4,
            noiseLevel: 0.3);

        data.PrintInfo();

        // ========== ТЕСТ 2: Сравнение инициализации ==========
        Console.WriteLine("\n2. Сравнение инициализации (k=4):");

        var kmeansRandom = new KMeans(k: 4, useKMeansPlusPlus: false);
        var (labelsRandom, _) = kmeansRandom.ClusterWithCentroids(data);
        double silhouetteRandom = SilhouetteAnalyzer.AverageSilhouette(data, labelsRandom, 4);

        var kmeansPP = new KMeans(k: 4, useKMeansPlusPlus: true);
        var (labelsPP, centroidsPP) = kmeansPP.ClusterWithCentroids(data);
        double silhouettePP = SilhouetteAnalyzer.AverageSilhouette(data, labelsPP, 4);

        Console.WriteLine($"   Случайная инициализация: Silhouette = {silhouetteRandom:F4}");
        Console.WriteLine($"   K-means++: Silhouette = {silhouettePP:F4}");

        // Размеры кластеров для k-means++
        int[] clusterSizes = new int[4];
        for (int i = 0; i < labelsPP.Length; i++)
            clusterSizes[labelsPP[i]]++;

        Console.WriteLine("\n   Размеры кластеров (k-means++):");
        for (int i = 0; i < 4; i++)
            Console.WriteLine($"     Кластер {i}: {clusterSizes[i]} генов");

        // ========== ТЕСТ 3: Выбор оптимального k ==========
        Console.WriteLine("\n3. Выбор оптимального числа кластеров:");
        var kSelection = KSelection.SelectK(data, minK: 2, maxK: 8);
        KSelection.PrintResult(kSelection);

        // ========== ТЕСТ 4: Случайные данные ==========
        Console.WriteLine("\n\n4. Тест на случайных данных (без структуры):");
        var randomData = SyntheticDataGenerator.GenerateRandom(genes: 500, samples: 10);
        var randomSelection = KSelection.SelectK(randomData, minK: 2, maxK: 8);
        KSelection.PrintResult(randomSelection);
    }
}