using GeneClusteringApp.Data;
using GeneClusteringApp.Clustering;

namespace GeneClusteringApp.Evaluation;

/// <summary>
/// Методы для выбора оптимального числа кластеров k
/// </summary>
public static class KSelection
{
    /// <summary>
    /// Результат выбора k
    /// </summary>
    public class KSelectionResult
    {
        public int ElbowK { get; set; }           // k по методу "локтя"
        public int SilhouetteK { get; set; }      // k по силуэту
        public Dictionary<int, double> Inertias { get; set; } = new();
        public Dictionary<int, double> Silhouettes { get; set; } = new();
        public int RecommendedK { get; set; }     // Рекомендуемое k
        public string Recommendation { get; set; } = "";
    }

    /// <summary>
    /// Выполняет анализ и выбирает оптимальное k обоими методами
    /// </summary>
    public static KSelectionResult SelectK(GeneMatrix data, int minK = 2, int maxK = 10, int maxIterations = 50)
    {
        var result = new KSelectionResult();

        Console.WriteLine($"\n=== Выбор оптимального k (от {minK} до {maxK}) ===\n");

        // Для каждого k запускаем кластеризацию
        for (int k = minK; k <= maxK; k++)
        {
            Console.Write($"k = {k}... ");

            var kmeans = new KMeans(k, maxIterations, useKMeansPlusPlus: true);
            var (labels, centroids) = kmeans.ClusterWithCentroids(data);

            // Считаем inertia
            double inertia = InertiaCalculator.Calculate(data, labels, centroids);
            result.Inertias[k] = inertia;

            // Считаем силуэт
            double silhouette = SilhouetteAnalyzer.AverageSilhouette(data, labels, k);
            result.Silhouettes[k] = silhouette;

            Console.WriteLine($"inertia = {inertia:F2}, силуэт = {silhouette:F4}");
        }

        // Находим оптимальное k по каждому методу
        result.ElbowK = FindElbowPoint(result.Inertias);
        result.SilhouetteK = FindMaxSilhouettePoint(result.Silhouettes);

        // Формируем рекомендацию
        DetermineRecommendation(result);

        return result;
    }

    /// <summary>
    /// Находит точку "локтя" на графике inertia
    /// </summary>
    private static int FindElbowPoint(Dictionary<int, double> inertias)
    {
        var kValues = inertias.Keys.OrderBy(k => k).ToList();
        if (kValues.Count < 3) return kValues.First();

        double maxAngle = 0;
        int elbowK = kValues[1];

        for (int i = 1; i < kValues.Count - 1; i++)
        {
            int kPrev = kValues[i - 1];
            int kCurr = kValues[i];
            int kNext = kValues[i + 1];

            double inertiaPrev = inertias[kPrev];
            double inertiaCurr = inertias[kCurr];
            double inertiaNext = inertias[kNext];

            // Векторы для вычисления угла
            double v1x = kCurr - kPrev;
            double v1y = inertiaCurr - inertiaPrev;
            double v2x = kNext - kCurr;
            double v2y = inertiaNext - inertiaCurr;

            double len1 = Math.Sqrt(v1x * v1x + v1y * v1y);
            double len2 = Math.Sqrt(v2x * v2x + v2y * v2y);

            if (len1 > 0 && len2 > 0)
            {
                double dot = v1x * v2x + v1y * v2y;
                double cosAngle = dot / (len1 * len2);
                double angle = Math.Acos(Math.Max(-1, Math.Min(1, cosAngle)));

                // Чем меньше угол, тем острее "локоть"
                if (angle < maxAngle || maxAngle == 0)
                {
                    maxAngle = angle;
                    elbowK = kCurr;
                }
            }
        }

        return elbowK;
    }

    /// <summary>
    /// Находит k с максимальным силуэтом
    /// </summary>
    private static int FindMaxSilhouettePoint(Dictionary<int, double> silhouettes)
    {
        int bestK = silhouettes.First().Key;
        double bestValue = silhouettes.First().Value;

        foreach (var kvp in silhouettes)
        {
            if (kvp.Value > bestValue)
            {
                bestValue = kvp.Value;
                bestK = kvp.Key;
            }
        }

        return bestK;
    }

    /// <summary>
    /// Формирует рекомендацию на основе обоих методов
    /// </summary>
    private static void DetermineRecommendation(KSelectionResult result)
    {
        // Если оба метода согласны
        if (result.ElbowK == result.SilhouetteK)
        {
            result.RecommendedK = result.ElbowK;
            result.Recommendation = $"Оба метода сошлись на k = {result.RecommendedK}";
        }
        else
        {
            // Если методы расходятся — отдаём приоритет силуэту (он надёжнее)
            result.RecommendedK = result.SilhouetteK;
            result.Recommendation = $"Методы разошлись: " +
                                    $"метод 'локтя' показал k = {result.ElbowK}, " +
                                    $"силуэт показал k = {result.SilhouetteK}. " +
                                    $"Рекомендуется k = {result.SilhouetteK} (силуэт надёжнее)";
        }

        // Дополнительная проверка: если силуэт очень низкий (< 0.3)
        double bestSilhouette = result.Silhouettes[result.SilhouetteK];
        if (bestSilhouette < 0.3)
        {
            result.Recommendation += " Внимание: силуэт очень низкий, данные могут не иметь чёткой кластерной структуры.";
        }
    }

    /// <summary>
    /// Выводит результаты выбора k
    /// </summary>
    public static void PrintResult(KSelectionResult result)
    {
        Console.WriteLine("\n=== РЕЗУЛЬТАТЫ ВЫБОРА k ===\n");

        Console.WriteLine("Метод \"локтя\":");
        Console.WriteLine($"  Оптимальное k = {result.ElbowK}");

        Console.WriteLine("\nМетод силуэта:");
        Console.WriteLine($"  Оптимальное k = {result.SilhouetteK} (силуэт = {result.Silhouettes[result.SilhouetteK]:F4})");

        Console.WriteLine("\nРекомендация:");
        Console.WriteLine($"  k = {result.RecommendedK}");
        Console.WriteLine($"  {result.Recommendation}");

        // Таблица всех значений
        Console.WriteLine("\nПодробная таблица:");
        Console.WriteLine("  k\tInertia\t\tСилуэт");
        Console.WriteLine("  --\t-------\t\t------");

        foreach (var k in result.Inertias.Keys.OrderBy(x => x))
        {
            string elbowMarker = (k == result.ElbowK) ? "(локоть)" : "";
            string silhouetteMarker = (k == result.SilhouetteK) ? "(макс)" : "";
            Console.WriteLine($"  {k}\t{result.Inertias[k]:F2}\t\t{result.Silhouettes[k]:F4}{elbowMarker}{silhouetteMarker}");
        }
    }
}