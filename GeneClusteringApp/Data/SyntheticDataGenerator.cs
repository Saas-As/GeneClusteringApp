using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GeneClusteringApp.Data
{
    /// <summary>
    /// Генератор синтетических данных для тестирования
    /// </summary>
    public static class SyntheticDataGenerator
    {
        private static Random _random = new Random(42);

        /// <summary>
        /// Генерирует данные с известной кластерной структурой 
        /// </summary>
        /// <param name="genes">Количество генов</param>
        /// <param name="samples">Количество образцов</param>
        /// <param name="trueClusters">Реальное количество кластеров</param>
        /// <param name="noiseLevel">Уровень шума (0 = без шума, 1 = сильный шум)</param>
        /// <returns>Матрица экспрессии</returns>
        public static GeneMatrix GenerateWithClusters(int genes, int samples, int trueClusters, double noiseLevel = 0.3)
        {
            var matrix = new GeneMatrix(genes, samples);

            // 1. Генерируем центры кластеров
            var centers = GenerateClusterCenters(trueClusters, samples);

            for (int g = 0; g < genes; g++)
            {
                // Определяем, к какому кластеру относится ген
                int cluster = g % trueClusters;

                // Для каждого образца генерируем значение
                for (int s = 0; s < samples; s++)
                {
                    // Базовое значение от центра кластера
                    double value = centers[cluster][s];
                    // Добавляем шум
                    double noise = (_random.NextDouble() - 0.5) * noiseLevel;
                    value += noise;

                    // Убеждаемся, что значение не отрицательное (экспрессия не бывает отрицательной)
                    if (value < 0) value = 0;

                    matrix[g, s] = value;
                }
            }

            return matrix;
        }

        private static double[][] GenerateClusterCenters(int clusters, int samples)
        {
            // массив массивов
            var centers = new double[clusters][];   

            for (int c = 0; c < clusters; c++)
            {
                centers[c] = new double[samples];

                // Каждый кластер имеет свой "профиль"
                // Используем синусоиду со сдвигом для каждого кластера
                for (int s = 0; s < samples; s++)
                {
                    double t = (double)s / samples;
                    // Уникальная форма для каждого кластера
                    double phase = (double)c / clusters * 2 * Math.PI;
                    centers[c][s] = 5 + 3 * Math.Sin(t * 2 * Math.PI + phase);

                    centers[c][s] += (_random.NextDouble() - 0.5) * 0.5;
                }
            }

            return centers;
        }

        /// <summary>
        /// Генерирует данные с разными "паттернами" экспрессии
        /// </summary>
        public static GeneMatrix GenerateWithPatterns(int genes, int samples)
        {
            var matrix = new GeneMatrix(genes, samples);

            // Определяем 4 типа паттернов
            // Паттерн 1: возрастающий
            // Паттерн 2: убывающий
            // Паттерн 3: пик в середине
            // Паттерн 4: случайный

            for (int g = 0; g < genes; g++)
            {
                int pattern = g % 4;

                for(int s = 0; s < samples; s++)
                {
                    double t = (double)s / samples;
                    double value = 0;

                    switch (pattern)
                    {
                        case 0:
                            value = t * 10;
                            break;
                        case 1:
                            value = (1 - t) * 10;
                            break;
                        case 2:
                            value = Math.Sin(t * Math.PI) * 10;
                            break;
                        case 3:
                            value = _random.NextDouble() * 10;
                            break;
                    }

                    value += (_random.NextDouble() - 0.5) * 0.5;
                    if(value < 0) value = 0;

                    matrix[g,s] =  value;
                }
            }

            return matrix;
        }

        /// <summary>
        /// Генерирует данные без чёткой кластерной структуры (случайные)
        /// </summary>
        public static GeneMatrix GenerateRandom(int genes, int samples, double minValue = 0, double maxValue = 10)
        {
            var matrix = new GeneMatrix(genes, samples);

            for(int g = 0; g < genes; g++)
            {
                for(int s = 0; s < samples;s++)
                {
                    matrix[g,s] = _random.NextDouble() * (maxValue - minValue);
                }
            }

            return matrix;
        }
    }
}
