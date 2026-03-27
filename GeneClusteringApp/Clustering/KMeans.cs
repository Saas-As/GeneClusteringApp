using GeneClusteringApp.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeneClusteringApp.Clustering
{
    public class KMeans
    {
        private readonly int _k;                    // Количество кластеров
        private readonly int _maxIterations;        // Максимальное число итераций
        private readonly double _tolerance;         // Порог сходимости
        private readonly Random _random;            // Генератор случайных чисел

        /// <summary>
        /// Конструктор алгоритма k-means
        /// </summary>
        /// <param name="k">Количество кластеров</param>
        /// <param name="maxIterations">Максимальное число итераций (по умолчанию 100)</param>
        /// <param name="tolerance">Порог сходимости (по умолчанию 0.0001)</param>
        public KMeans(int k, int maxIterations = 100, double tolerance = 1e-4)
        {
            _k = k;
            _maxIterations = maxIterations;
            _tolerance = tolerance;
            _random = new Random(42); // Фиксированный seed для воспроизводимости
        }

        public int[] Cluster(GeneMatrix data)
        {
            int genes = data.GeneCount;
            int samples = data.SampleCount;

            // ШАГ 1: Инициализация центроидов
            double[][] centroids = InitializeCentroids(data);

            // Массив для хранения принадлежности генов к кластерам
            int[] labels = new int[genes];

            // Основной цикл алгоритма
            for(int i = 0; i < _maxIterations; i++)
            {
                // ШАГ 2.1: Назначение кластеров
                int[] newLabels = AssignClusters(data, centroids);

                // ШАГ 2.2: Пересчёт центроидов
                double[][] newCentroids = RecalculateCentroids(data, newLabels, samples);

                // ШАГ 2.3: Проверка сходимости
                bool converged = CheckConvergence(centroids, newCentroids);

                // Обновляем данные для следующей итерации
                centroids = newCentroids;
                labels = newLabels;

                // Если алгоритм сошёлся — завершаем
                if (converged)
                {
                    Console.WriteLine($"Алгоритм сошёлся на итерации {i + 1}");
                    break;
                }
            }

            return labels;
        }

        /// <summary>
        /// Инициализация центроидов случайным выбором k различных генов
        /// </summary>
        private double[][] InitializeCentroids(GeneMatrix data)
        {
            int genes = data.GeneCount;
            int samples = data.SampleCount;

            double[][] centroids = new double[_k][];
            for (int i = 0; i < _k; i++)
                centroids[i] = new double[samples];

            // Множество для хранения уже выбранных индексов
            HashSet<int> selectedIndices = new HashSet<int>();

            // Выбираем k случайных различных генов
            for(int i = 0; i < _k; i++)
            {
                int randomIndex;
                do
                {
                    randomIndex = _random.Next(genes);
                } while (selectedIndices.Contains(randomIndex));

                selectedIndices.Add(randomIndex);

                // Копируем вектор выбранного гена в центроид
                double[] geneVector = data.GetGeneVector(randomIndex);
                for(int s = 0; s < samples; s++)
                {
                    centroids[i][s] = geneVector[s];
                }
            }

            return centroids;
        }

        /// <summary>
        /// Назначает каждый ген ближайшему центроиду
        /// </summary>
        private int[] AssignClusters(GeneMatrix data, double[][] centroids)
        {
            int genes = data.GeneCount;
            int samples = data.SampleCount;
            int[] labels = new int[genes];

            for(int gene = 0; gene < genes; gene++)
            {
                double minDistance = double.MaxValue;
                int bestCluster = -1;

                double[] geneVector = data.GetGeneVector(gene);

                for(int cluster = 0; cluster < _k;  cluster++)
                {
                    double distance = SquaredEuclideanDistance(geneVector, centroids[cluster]);

                    if(distance < minDistance)
                    {
                        minDistance = distance;
                        bestCluster = cluster;
                    }
                }
                labels[gene] = bestCluster;
            }
            return labels;
        }

        private double SquaredEuclideanDistance(double[] a, double[] b)
        {
            double sum = 0.0;
            for(int i = 0; i < a.Length; i++)
            {
                double diff = a[i] - b[i];
                sum += diff * diff;
            }

            return sum;
        }

        /// <summary>
        /// Пересчитывает центроиды как среднее арифметическое генов в кластере
        /// </summary>
        private double[][] RecalculateCentroids(GeneMatrix data, int[] labels, int samples)
        {
            // Массивы для сумм значений по каждому кластеру
            double[][] sums = new double[_k][];
            for(int i = 0; i < _k; i++)
            {
                sums[i] = new double[samples];
            }

            // Счётчики количества генов в каждом кластере
            int[] counts = new int[_k];

            // Проходим по всем генам и суммируем их по кластерам
            for (int gene = 0; gene < data.GeneCount; gene++)
            {
                int cluster = labels[gene];
                counts[cluster]++;

                // Получаем вектор гена
                double[] geneVector = data.GetGeneVector(gene);

                for (int s = 0; s < samples; s++)
                {
                    sums[cluster][s] += geneVector[s];
                }
            }

            // Вычисляем среднее для каждого кластера
            double[][] newCentroids = new double[_k][];
            for(int cluster = 0; cluster < _k; cluster++)
            {
                newCentroids[cluster] = new double[samples];

                // Если кластер не пуст
                if (counts[cluster] > 0)
                {
                    for(int s = 0; s < samples; s++)
                    {
                        newCentroids[cluster][s] = sums[cluster][s] / counts[cluster];
                    }
                }
                // Если кластер пуст — оставляем нулевой вектор
            }

            // После вычисления средних, проверяем пустые кластеры
            for (int cluster = 0; cluster < _k; cluster++)
            {
                if (counts[cluster] == 0)
                {
                    // Переинициализируем пустой кластер случайным геном
                    int randomGene = _random.Next(data.GeneCount);
                    double[] randomVector = data.GetGeneVector(randomGene);
                    for (int s = 0; s < samples; s++)
                    {
                        newCentroids[cluster][s] = randomVector[s];
                    }
                    Console.WriteLine($"  [Внимание] Кластер {cluster} был пуст, переинициализирован");
                }
            }

            return newCentroids;
        }

        /// <summary>
        /// Проверяет, изменились ли центроиды меньше порога tolerance
        /// </summary>
        private bool CheckConvergence(double[][] oldCentroids, double[][] newCentroids)
        {
            for(int cluster = 0; cluster < _k; cluster++)
            {
                double diff = SquaredEuclideanDistance(oldCentroids[cluster], newCentroids[cluster]);
                if (diff > _tolerance) 
                    return false; // Есть изменения больше порога — не сошлись
            }
            return true; // Все изменения меньше порога — сошлись
        }

        /// <summary>
        /// Выполняет кластеризацию и возвращает метки и центроиды
        /// </summary>
        public (int[] labels, double[][] centroids) ClusterWithCentroids(GeneMatrix data)
        {
            int genes = data.GeneCount;
            int samples = data.SampleCount;

            // Инициализация
            double[][] centroids = InitializeCentroids(data);
            int[] labels = new int[genes];

            // Основной цикл
            for (int iteration = 0; iteration < _maxIterations; iteration++)
            {
                int[] newLabels = AssignClusters(data, centroids);
                double[][] newCentroids = RecalculateCentroids(data, newLabels, samples);

                bool converged = CheckConvergence(centroids, newCentroids);

                centroids = newCentroids;
                labels = newLabels;

                if (converged)
                    break;
            }

            return (labels, centroids);
        }
    }
}
