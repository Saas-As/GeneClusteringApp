using GeneClusteringApp.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeneClusteringApp.Evaluation
{
    /// <summary>
    /// Вычисляет силуэтный коэффициент для оценки качества кластеризации
    /// </summary>
    public static class SilhouetteAnalyzer
    {
        /// <summary>
        /// Вычисляет средний силуэтный коэффициент для всех генов
        /// Значение от -1 до 1:
        ///   > 0.5 — отличная кластеризация
        ///   > 0.3 — хорошая кластеризация
        ///   > 0.1 — слабая кластеризация
        ///   < 0.1 — кластеризация не удалась
        /// </summary>
        public static double AverageSilhouette(GeneMatrix data, int[] labels, int k)
        {
            double total = 0.0;

            for (int gene = 0; gene < data.GeneCount; gene++)
            {
                total += SilhouetteForGene(data, gene, labels, k);
            }

            return total / data.GeneCount;
        }

        /// <summary>
        /// Вычисляет силуэтный коэффициент для одного гена
        /// </summary>
        public static double SilhouetteForGene(GeneMatrix data, int gene, int[] labels, int k)
        {
            int myCluster = labels[gene];
            double[] geneVector = data.GetGeneVector(gene);

            // Шаг 1: a = среднее расстояние до генов в СВОЁМ кластере
            double a = AverageDistanceToCluster(data, gene, geneVector, myCluster, labels, excludeSelf: true);

            // Шаг 2: b = среднее расстояние до БЛИЖАЙШЕГО соседнего кластера
            double b = FindNearestOtherClusterDistance(data, gene, geneVector, myCluster, labels, k);

            // Шаг 3: Вычисляем силуэт
            if (a < b)
                return 1 - (a / b);
            else if (a > b)
                return (b / a) - 1;
            else
                return 0;
        }

        /// <summary>
        /// Среднее расстояние от гена до всех генов в указанном кластере
        /// </summary>
        private static double AverageDistanceToCluster(
            GeneMatrix data,
            int gene,
            double[] geneVector,
            int cluster,
            int[] labels,
            bool excludeSelf)
        {
            double sum = 0.0;
            int count = 0;

            for (int otherGene = 0; otherGene < data.GeneCount; otherGene++)
            {
                if (excludeSelf && otherGene == gene) continue;
                if (labels[otherGene] == cluster)
                {
                    double[] otherVector = data.GetGeneVector(otherGene);
                    sum += SquaredEuclideanDistance(geneVector, otherVector);
                    count++;
                }
            }
            if (count == 0) return 0;
            return sum / count;
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

        /// <summary>
        /// Находит ближайший соседний кластер и возвращает среднее расстояние до него
        /// </summary>
        private static double FindNearestOtherClusterDistance(
            GeneMatrix data,
            int gene,
            double[] geneVector,
            int myCluster,
            int[] labels,
            int k)
        {
            double minDistance = double.MaxValue;

            for (int otherCluster = 0; otherCluster < k; otherCluster++)
            {
                if (otherCluster == myCluster) continue;

                double avgDist = AverageDistanceToCluster(data, gene, geneVector, otherCluster, labels, excludeSelf: false);

                if (avgDist < minDistance)
                    minDistance = avgDist;
            }

            return minDistance;
        }
        /// <summary>
        /// Возвращает текстовую интерпретацию силуэтного коэффициента
        /// </summary>
        public static string Interpret(double silhouette)
        {
            if (silhouette > 0.5)
                return "Отличная кластеризация";
            if (silhouette > 0.3)
                return "Хорошая кластеризация";
            if (silhouette > 0.1)
                return "Слабая кластеризация";
            return "Кластеризация не удалась";
        }
    }
}
