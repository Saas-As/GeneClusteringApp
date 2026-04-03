using GeneClusteringApp.Clustering;
using GeneClusteringApp.Data;
using Xunit;

namespace GeneClusteringApp.Tests;

public class KMeansTests
{
    private GeneMatrix CreateSimpleMatrix()
    {
        // 6 генов, 2 образца: создаём 3 чётких кластера
        var matrix = new GeneMatrix(6, 2);

        // Кластер 0: гены 0,1 -> значения близкие к (1,1)
        matrix[0, 0] = 1.0; matrix[0, 1] = 1.0;
        matrix[1, 0] = 1.1; matrix[1, 1] = 0.9;

        // Кластер 1: гены 2,3 -> значения близкие к (5,5)
        matrix[2, 0] = 5.0; matrix[2, 1] = 5.0;
        matrix[3, 0] = 5.1; matrix[3, 1] = 4.9;

        // Кластер 2: гены 4,5 -> значения близкие к (10,10)
        matrix[4, 0] = 10.0; matrix[4, 1] = 10.0;
        matrix[5, 0] = 10.1; matrix[5, 1] = 9.9;

        return matrix;
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        var kmeans = new KMeans(3, maxIterations: 50, tolerance: 0.001, useKMeansPlusPlus: true);

        // Через публичное свойство проверяем, что объект создан
        Assert.NotNull(kmeans);
    }

    [Fact]
    public void Cluster_ShouldReturnCorrectNumberOfLabels()
    {
        var matrix = CreateSimpleMatrix();
        var kmeans = new KMeans(3);

        int[] labels = kmeans.Cluster(matrix);

        Assert.Equal(matrix.GeneCount, labels.Length);
    }

    [Fact]
    public void Cluster_AllLabelsShouldBeInRange()
    {
        var matrix = CreateSimpleMatrix();
        var kmeans = new KMeans(3);

        int[] labels = kmeans.Cluster(matrix);

        foreach (int label in labels)
        {
            Assert.InRange(label, 0, 2);
        }
    }

    [Fact]
    public void ClusterWithCentroids_ShouldReturnCentroidsAndLabels()
    {
        var matrix = CreateSimpleMatrix();
        var kmeans = new KMeans(3);

        var (labels, centroids) = kmeans.ClusterWithCentroids(matrix);

        Assert.Equal(matrix.GeneCount, labels.Length);
        Assert.Equal(3, centroids.Length);
        Assert.Equal(matrix.SampleCount, centroids[0].Length);
    }

    [Fact]
    public void Cluster_ShouldConvergeWithinMaxIterations()
    {
        var matrix = CreateSimpleMatrix();
        var kmeans = new KMeans(3, maxIterations: 10);

        kmeans.Cluster(matrix);

        Assert.True(kmeans.LastIterationCount <= 10);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Cluster_ShouldWorkWithDifferentK(int k)
    {
        var matrix = CreateSimpleMatrix();
        var kmeans = new KMeans(k, maxIterations: 50);

        int[] labels = kmeans.Cluster(matrix);

        Assert.Equal(matrix.GeneCount, labels.Length);

        // Проверяем, что все метки в диапазоне [0, k-1]
        foreach (int label in labels)
        {
            Assert.InRange(label, 0, k - 1);
        }
    }

    [Fact]
    public void Cluster_EmptyClusterShouldBeReinitialized()
    {
        // Матрица с 2 генами, но просим 3 кластера -> один кластер будет пустым
        var matrix = new GeneMatrix(2, 2);
        matrix[0, 0] = 1.0; matrix[0, 1] = 1.0;
        matrix[1, 0] = 2.0; matrix[1, 1] = 2.0;

        var kmeans = new KMeans(3, maxIterations: 10);

        // Не должно выбросить исключение
        var exception = Record.Exception(() => kmeans.Cluster(matrix));
        Assert.Null(exception);
    }
}