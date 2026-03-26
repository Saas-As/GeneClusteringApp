using GeneClusteringApp.Data;

namespace GeneClusteringApp;

class Program
{
    static void Main(string[] args)
    {
        // Тест 1: Создание матрицы
        Console.WriteLine("--- Тест 1: Создание матрицы ---");
        var matrix = new GeneMatrix(10, 5);
        matrix.PrintInfo();
        matrix.PrintPreview();

        Console.WriteLine("\n--- Тест 2: Заполнение и доступ к данным ---");
        // Заполняем тестовыми значениями
        for (int g = 0; g < matrix.GeneCount; g++)
        {
            for (int s = 0; s < matrix.SampleCount; s++)
            {
                matrix[g, s] = g * 10 + s;
            }
        }
        matrix.PrintPreview();

        Console.WriteLine("\n--- Тест 3: Получение вектора гена ---");
        var geneVector = matrix.GetGeneVector(3);
        Console.WriteLine($"Ген 4: [{string.Join(", ", geneVector)}]");

        Console.WriteLine("\n--- Тест 4: Синтетические данные с кластерами ---");
        var clusteredData = SyntheticDataGenerator.GenerateWithClusters(
            genes: 100,
            samples: 20,
            trueClusters: 4,
            noiseLevel: 0.3);
        clusteredData.PrintInfo();

        Console.WriteLine("\nПервые 10 генов с паттернами:");
        for (int g = 0; g < 10; g++)
        {
            var vec = clusteredData.GetGeneVector(g);
            // Показываем только первые 5 значений для краткости
            Console.WriteLine($"Ген {g}: [{string.Join(", ", vec.Take(5).Select(v => v.ToString("F2")))}...]");
        }

        Console.WriteLine("\n--- Тест 5: Данные с паттернами ---");
        var patternData = SyntheticDataGenerator.GenerateWithPatterns(20, 10);
        patternData.PrintInfo();

        Console.WriteLine("\nПаттерны экспрессии (первые 8 генов):");
        for (int g = 0; g < 8; g++)
        {
            var vec = patternData.GetGeneVector(g);
            Console.WriteLine($"Ген {g}: [{string.Join(", ", vec.Select(v => v.ToString("F2")))}]");
        }
    }
}