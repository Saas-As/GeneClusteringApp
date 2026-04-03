using GeneClusteringApp.Data;
using GeneClusteringApp.Evaluation;
using Xunit;

namespace GeneClusteringApp.Tests;

public class InertiaCalculatorTests
{
    [Fact]
    public void Calculate_WithPerfectCentroids_ShouldReturnZero()
    {
        var matrix = new GeneMatrix(3, 2);
        matrix[0, 0] = 1.0; matrix[0, 1] = 1.0;
        matrix[1, 0] = 2.0; matrix[1, 1] = 2.0;
        matrix[2, 0] = 3.0; matrix[2, 1] = 3.0;

        int[] labels = { 0, 1, 2 };

        double[][] centroids = new double[3][];
        centroids[0] = new double[] { 1.0, 1.0 };
        centroids[1] = new double[] { 2.0, 2.0 };
        centroids[2] = new double[] { 3.0, 3.0 };

        double inertia = InertiaCalculator.Calculate(matrix, labels, centroids);

        Assert.Equal(0.0, inertia, 5);
    }

    [Fact]
    public void Calculate_WithWrongCentroids_ShouldReturnPositiveValue()
    {
        var matrix = new GeneMatrix(2, 2);
        matrix[0, 0] = 1.0; matrix[0, 1] = 1.0;
        matrix[1, 0] = 2.0; matrix[1, 1] = 2.0;

        int[] labels = { 0, 0 };

        double[][] centroids = new double[1][];
        centroids[0] = new double[] { 0.0, 0.0 };

        double inertia = InertiaCalculator.Calculate(matrix, labels, centroids);

        Assert.True(inertia > 0);
    }

    [Fact]
    public void Calculate_ShouldReturnLargerInertiaForSpreadOutData()
    {
        var matrix = new GeneMatrix(2, 2);

        // Компактные данные
        matrix[0, 0] = 1.0; matrix[0, 1] = 1.0;
        matrix[1, 0] = 1.1; matrix[1, 1] = 0.9;

        int[] labels = { 0, 0 };
        double[][] centroids = new double[1][];
        centroids[0] = new double[] { 1.05, 0.95 };

        double compactInertia = InertiaCalculator.Calculate(matrix, labels, centroids);

        // Разреженные данные
        matrix[0, 0] = 1.0; matrix[0, 1] = 1.0;
        matrix[1, 0] = 100.0; matrix[1, 1] = 100.0;
        centroids[0] = new double[] { 50.5, 50.5 };

        double spreadInertia = InertiaCalculator.Calculate(matrix, labels, centroids);

        Assert.True(spreadInertia > compactInertia);
    }
}