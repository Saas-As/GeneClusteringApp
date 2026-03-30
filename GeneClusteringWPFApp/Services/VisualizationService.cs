using GeneClusteringApp.Data;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneClusteringWPFApp.Services
{
    /// <summary>
    /// Сервис для визуализации результатов кластеризации
    /// </summary>
    public static class VisualizationService
    {
        /// <summary>
        /// Результат PCA анализа
        /// </summary>
        public class PCAResult
        {
            public List<PCAPoint> Points { get; set; } = new();
            //public double ExplainedVariancePC1 { get; set; }
            //public double ExplainedVariancePC2 { get; set; }
        }

        /// <summary>
        /// Точка в пространстве PCA
        /// </summary>
        public class PCAPoint
        {
            public double PC1 { get; set; }
            public double PC2 { get; set; }
            public int Cluster { get; set; }
            public string GeneName { get; set; } = "";
            public double[] OriginalVector { get; set; } = Array.Empty<double>();
        }

        /// <summary>
        /// Выполняет PCA анализ данных
        /// </summary>
        public static PCAResult PerformPCA(GeneMatrix data, int[] labels, int components = 2)
        {
            int genes = data.GeneCount;
            int samples = data.SampleCount;

            // Создаём матрицу данных (гены × образцы)
            var matrix = new double[genes, samples];
            for (int g = 0; g < genes; g++)
                for (int s = 0; s < samples; s++)
                    matrix[g, s] = data[g, s];

            // 1. Центрируем данные (вычитаем среднее)
            var means = new double[samples];
            for (int s = 0; s < samples; s++)
            {
                double sum = 0;
                for (int g = 0; g < genes; g++)
                    sum += matrix[g, s];
                means[s] = sum / genes;
                for (int g = 0; g < genes; g++)
                    matrix[g, s] -= means[s];
            }

            // 2. Вычисляем ковариационную матрицу (samples × samples)
            var cov = new double[samples, samples];
            for (int i = 0; i < samples; i++)
            {
                for (int j = 0; j < samples; j++)
                {
                    double sum = 0;
                    for (int g = 0; g < genes; g++)
                        sum += matrix[g, i] * matrix[g, j];
                    cov[i, j] = sum / (genes - 1);
                }
            }

            // 3. Находим собственные значения и векторы (упрощённо)
            // Для реального PCA нужно использовать библиотеку, но для визуализации
            // используем первые два компонента как проекцию на главные оси
            var result = new PCAResult();

            // Временное решение: используем первые два измерения как PC1 и PC2
            // (в реальном приложении лучше использовать библиотеку типа MathNet.Numerics)
            for (int g = 0; g < genes; g++)
            {
                result.Points.Add(new PCAPoint
                {
                    PC1 = matrix[g, 0],
                    PC2 = matrix[g, Math.Min(1, samples - 1)],
                    Cluster = labels[g],
                    GeneName = data.GetGeneName(g),
                    OriginalVector = data.GetGeneVector(g)
                });
            }

            return result;
        }

        /// <summary>
        /// Создаёт график PCA проекции
        /// </summary>
        public static PlotModel CreatePCAPlot(PCAResult pcaResult, string title = "PCA проекция")
        {
            var plotModel = new PlotModel { Title = title };

            // Получаем уникальные кластеры
            var clusters = pcaResult.Points.Select(p => p.Cluster).Distinct().OrderBy(c => c).ToList();
            var colors = GetColors(clusters.Count);

            // Добавляем серии для каждого кластера
            foreach (var cluster in clusters)
            {
                var series = new ScatterSeries
                {
                    Title = $"Кластер {cluster}",
                    MarkerType = MarkerType.Circle,
                    MarkerFill = colors[cluster % colors.Length],
                    MarkerSize = 3
                };

                var points = pcaResult.Points.Where(p => p.Cluster == cluster);
                foreach (var point in points)
                {
                    series.Points.Add(new ScatterPoint(point.PC1, point.PC2));
                }

                plotModel.Series.Add(series);
            }

            // Настройка осей
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "PC1",
                TitleFontSize = 12
            });
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "PC2",
                TitleFontSize = 12
            });

            return plotModel;
        }


        /// <summary>
        /// Получить массив цветов для кластеров
        /// </summary>
        private static OxyColor[] GetColors(int count)
        {
            var baseColors = new[]
            {
                OxyColors.Red, OxyColors.Blue, OxyColors.Green, OxyColors.Orange,
                OxyColors.Purple, OxyColors.Brown, OxyColors.Pink, OxyColors.Cyan,
                OxyColors.Magenta, OxyColors.Lime, OxyColors.Teal, OxyColors.Gold,
                OxyColors.Coral, OxyColors.Indigo, OxyColors.Olive
            };

            var result = new OxyColor[count];
            for (int i = 0; i < count; i++)
                result[i] = baseColors[i % baseColors.Length];
            return result;
        }
    }
}