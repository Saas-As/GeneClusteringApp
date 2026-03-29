using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace GeneClusteringApp.Data;

/// <summary>
/// Загрузчик данных экспрессии генов из CSV файлов
/// </summary>
public static class CSVLoader
{
    /// <summary>
    /// Загружает матрицу экспрессии из CSV файла
    /// </summary>
    /// <param name="filePath">Путь к CSV файлу</param>
    /// <param name="hasHeader">Есть ли строка заголовка с названиями образцов</param>
    /// <param name="geneNamesColumn">Индекс колонки с названиями генов (обычно 0)</param>
    /// <returns>Матрица экспрессии</returns>
    public static GeneMatrix LoadFromCsv(string filePath, bool hasHeader = true, int geneNamesColumn = 0)
    {
        Console.WriteLine($"\nЗагрузка данных из файла: {filePath}");

        var lines = File.ReadAllLines(filePath)
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
            .ToList();

        if (lines.Count == 0)
            throw new Exception("Файл пуст или не содержит данных");

        // Определяем размеры матрицы
        int samples = 0;
        int startLine = hasHeader ? 1 : 0;

        // Парсим заголовок (названия образцов)
        string[] sampleNames = null;
        if (hasHeader)
        {
            var headerParts = ParseCsvLine(lines[0]);
            sampleNames = headerParts.Skip(geneNamesColumn + 1).ToArray();
            samples = sampleNames.Length;
        }

        // Определяем количество генов
        int genes = lines.Count - startLine;

        // Если заголовка нет, определяем количество образцов по первой строке
        if (!hasHeader && samples == 0)
        {
            var firstParts = ParseCsvLine(lines[0]);
            samples = firstParts.Length - 1;
        }

        Console.WriteLine($"  Найдено: {genes} генов, {samples} образцов");

        // Создаём матрицу
        var matrix = new GeneMatrix(genes, samples);

        // Устанавливаем названия образцов
        if (sampleNames != null)
        {
            for (int s = 0; s < samples; s++)
                matrix.SetSampleName(s, sampleNames[s]);
        }

        // Заполняем данные
        for (int g = 0; g < genes; g++)
        {
            var parts = ParseCsvLine(lines[startLine + g]);

            // Название гена
            string geneName = parts[geneNamesColumn];
            matrix.SetGeneName(g, geneName);

            // Значения экспрессии
            for (int s = 0; s < samples; s++)
            {
                int colIndex = geneNamesColumn + 1 + s;
                if (colIndex < parts.Length)
                {
                    double value = double.Parse(parts[colIndex], CultureInfo.InvariantCulture);
                    matrix[g, s] = value;
                }
            }
        }

        Console.WriteLine($"  Загрузка завершена");
        return matrix;
    }

    /// <summary>
    /// Парсит строку CSV с учётом кавычек
    /// </summary>
    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        string current = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }

        result.Add(current);
        return result.ToArray();
    }

    /// <summary>
    /// Загружает матрицу и выводит информацию о ней
    /// </summary>
    public static GeneMatrix LoadAndPrintInfo(string filePath)
    {
        var matrix = LoadFromCsv(filePath);
        matrix.PrintInfo();
        matrix.PrintPreview(5, 5);
        return matrix;
    }
}