using GeneClusteringApp.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeneClusteringApp.Evaluation
{
    /// <summary>
    /// Вычисляет внутрикластерную сумму квадратов расстояний (inertia)
    /// </summary>
    public static class InertiaCalculator
    {
        /// <summary>
        /// Вычисляет сумму квадратов расстояний от генов до их центроидов
        /// Чем меньше значение — тем компактнее кластеры
        /// </summary>
        public static double Calculate(GeneMatrix data, int[] labels, double[][] centroids)
        {
            double total = 0.0;

            for (int gene = 0; gene < data.GeneCount; gene++)
            {
                int cluster = labels[gene];

                double[] geneVector = data.GetGeneVector(gene);
                double[] centroid = centroids[cluster];
                double distance = SquaredEuclideanDistance(geneVector, centroid);
                total += distance;
            }
            return total;
        }
        /// <summary>
        /// Вычисляет квадрат евклидова расстояния между двумя векторами
        /// </summary>
        private static double SquaredEuclideanDistance(double[] a, double[] b)
        {
            double sum = 0.0;
            for (int i = 0; i < a.Length; i++)
            {
                double diff = a[i] - b[i];
                sum += diff * diff;
            }
            return sum;
        }
    }
}
