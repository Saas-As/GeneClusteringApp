using System;
using GeneClusteringApp.Data;

namespace GeneClusteringApp.Utils;

/// <summary>
/// Методы нормализации данных экспрессии генов
/// </summary>
public static class Normalizer
{
    /// <summary>
    /// Стандартизация Z-score: (x - mean) / std
    /// Приводит данные к распределению со средним 0 и дисперсией 1
    /// </summary>
    public static GeneMatrix ZScoreNormalize(GeneMatrix data)
    {
        Console.WriteLine("\nВыполняется Z-score нормализация...");

        var result = new GeneMatrix(data.GeneCount, data.SampleCount);

        // Копируем названия
        for (int g = 0; g < data.GeneCount; g++)
            result.SetGeneName(g, data.GetGeneName(g));
        for (int s = 0; s < data.SampleCount; s++)
            result.SetSampleName(s, data.GetSampleName(s));

        // Для каждого гена считаем среднее и стандартное отклонение
        for (int g = 0; g < data.GeneCount; g++)
        {
            double[] values = data.GetGeneVector(g);

            double mean = 0;
            for (int s = 0; s < values.Length; s++)
                mean += values[s];
            mean /= values.Length;

            double variance = 0;
            for (int s = 0; s < values.Length; s++)
                variance += Math.Pow(values[s] - mean, 2);
            variance /= values.Length;

            double std = Math.Sqrt(variance);

            for (int s = 0; s < values.Length; s++)
            {
                if (std > 1e-10)
                    result[g, s] = (values[s] - mean) / std;
                else
                    result[g, s] = 0;
            }
        }

        Console.WriteLine("  Нормализация завершена");
        return result;
    }

    /// <summary>
    /// Минимаксная нормализация: (x - min) / (max - min)
    /// Приводит данные к диапазону [0, 1]
    /// </summary>
    public static GeneMatrix MinMaxNormalize(GeneMatrix data)
    {
        Console.WriteLine("\nВыполняется Min-Max нормализация...");

        var result = new GeneMatrix(data.GeneCount, data.SampleCount);

        // Копируем названия
        for (int g = 0; g < data.GeneCount; g++)
            result.SetGeneName(g, data.GetGeneName(g));
        for (int s = 0; s < data.SampleCount; s++)
            result.SetSampleName(s, data.GetSampleName(s));

        // Для каждого гена находим min и max
        for (int g = 0; g < data.GeneCount; g++)
        {
            double[] values = data.GetGeneVector(g);

            double min = values[0];
            double max = values[0];
            for (int s = 1; s < values.Length; s++)
            {
                if (values[s] < min) min = values[s];
                if (values[s] > max) max = values[s];
            }

            double range = max - min;

            for (int s = 0; s < values.Length; s++)
            {
                if (range > 1e-10)
                    result[g, s] = (values[s] - min) / range;
                else
                    result[g, s] = 0;
            }
        }

        Console.WriteLine("  Нормализация завершена");
        return result;
    }

    /// <summary>
    /// Логарифмическое преобразование log2(x + 1)
    /// Используется для RNA-seq данных
    /// </summary>
    public static GeneMatrix Log2Normalize(GeneMatrix data)
    {
        Console.WriteLine("\nВыполняется log2 нормализация...");

        var result = new GeneMatrix(data.GeneCount, data.SampleCount);

        // Копируем названия
        for (int g = 0; g < data.GeneCount; g++)
            result.SetGeneName(g, data.GetGeneName(g));
        for (int s = 0; s < data.SampleCount; s++)
            result.SetSampleName(s, data.GetSampleName(s));

        for (int g = 0; g < data.GeneCount; g++)
        {
            for (int s = 0; s < data.SampleCount; s++)
            {
                double value = data[g, s];
                result[g, s] = Math.Log2(value + 1);
            }
        }

        Console.WriteLine("  Нормализация завершена");
        return result;
    }
}