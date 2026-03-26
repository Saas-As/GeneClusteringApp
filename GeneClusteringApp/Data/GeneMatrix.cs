using System;
using System.Collections.Generic;
using System.Text;

namespace GeneClusteringApp.Data
{
    /// <summary>
    /// Матрица экспрессии генов
    /// Строки — гены, столбцы — образцы
    /// </summary>
    public class GeneMatrix
    {
        private double[,] _data;
        private string[] _geneNames; // гены
        private string[] _sampleNames; // образцы

        /// <summary>
        /// Количество генов (строк)
        /// </summary>
        public int GeneCount { get; private set; }

        /// <summary>
        /// Количество образцов (столбцов)
        /// </summary>
        public int SampleCount { get; private set; }

        /// <summary>
        /// Создаёт новую матрицу указанного размера
        /// </summary>
        /// <param name="genes">Гены</param>
        /// <param name="samples">Образцы</param>
        public GeneMatrix(int genes, int samples)
        {
            GeneCount = genes;
            SampleCount = samples;
            _data = new double[genes, samples];
            _geneNames = new string[genes];
            _sampleNames = new string[samples];

            // Инициализация названий по умолчанию
            for (int i = 0; i < genes; i++)
                _geneNames[i] = $"Gene_{i + 1}";
            for (int j = 0; j < samples; j++)
                _sampleNames[j] = $"Sample_{j + 1}";
        }

        /// <summary>
        /// Доступ к значению экспрессии по индексам
        /// </summary>
        /// <param name="gene"></param>
        /// <param name="sample"></param>
        /// <returns></returns>
        public double this[int gene, int sample]
        {
            get => _data[gene, sample];
            set => _data[gene, sample] = value;
        }

        /// <summary>
        /// Получить вектор экспрессии гена
        /// </summary>
        /// <param name="gene"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public double[] GetGeneVector(int gene)
        {
            if (gene < 0 || gene >= GeneCount)
                throw new ArgumentOutOfRangeException(nameof(gene));

            var vector = new double[SampleCount];
            for (int i = 0; i < SampleCount; i++)
                vector[i] = _data[gene, i];
            return vector;
        }


        /// <summary>
        /// Получить название гена
        /// </summary>
        public string GetGeneName(int gene)
        {
            return _geneNames[gene];
        }

        /// <summary>
        /// Установить название гена
        /// </summary>
        public void SetGeneName(int gene, string name)
        {
            _geneNames[gene] = name;
        }

        /// <summary>
        /// Получить название образца
        /// </summary>
        public string GetSampleName(int sample)
        {
            return _sampleNames[sample];
        }

        /// <summary>
        /// Установить название образца
        /// </summary>
        public void SetSampleName(int sample, string name)
        {
            _sampleNames[sample] = name;
        }

        /// <summary>
        /// Выводит информацию о матрице в консоль
        /// </summary>
        public void PrintInfo()
        {
            Console.WriteLine($"Матрица экспрессии: {GeneCount} генов * {SampleCount} образцов");
            Console.WriteLine($"Объём данных: {GeneCount * SampleCount} значений");
            Console.WriteLine($"Память: ~{GeneCount * SampleCount * 8 / 1024.0:F1} КБ");
        }

        /// <summary>
        /// Выводит первые несколько строк для проверки
        /// </summary>
        public void PrintPreview(int maxGenes = 5, int maxSamples = 5)
        {
            Console.WriteLine("\nПервые строки матрицы (гены * образцы):");

            // Заголовки образцов
            Console.Write("Ген\\Обр\t");
            for (int s = 0; s < Math.Min(maxSamples, SampleCount); s++)
                Console.Write($"{GetSampleName(s)} ");
            Console.WriteLine();

            // Данные
            for (int g = 0; g < Math.Min(maxGenes, GeneCount); g++)
            {
                Console.Write($"{GetGeneName(g)}\t");
                for (int s = 0; s < Math.Min(maxSamples, SampleCount); s++)
                    Console.Write($"{_data[g, s]:F2}\t");
                Console.WriteLine();
            }

            if (GeneCount > maxGenes)
                Console.WriteLine($"... и ещё {GeneCount - maxGenes} генов");
            if (SampleCount > maxSamples)
                Console.WriteLine($"... и ещё {SampleCount - maxSamples} образцов");
        }
    }
}
