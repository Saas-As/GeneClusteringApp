using GeneClusteringApp.Data;
using GeneClusteringApp.Evaluation;
using Xunit;

namespace GeneClusteringApp.Tests;

public class SilhouetteAnalyzerTests
{
    private GeneMatrix CreatePerfectClustersMatrix()
    {
        var matrix = new GeneMatrix(6, 2);

        // Кластер 0
        matrix[0, 0] = 1.0; matrix[0, 1] = 1.0;
        matrix[1, 0] = 1.1; matrix[1, 1] = 0.9;

        // Кластер 1 (далеко)
        matrix[2, 0] = 10.0; matrix[2, 1] = 10.0;
        matrix[3, 0] = 10.1; matrix[3, 1] = 9.9;

        // Кластер 2 (ещё дальше)
        matrix[4, 0] = 20.0; matrix[4, 1] = 20.0;
        matrix[5, 0] = 20.1; matrix[5, 1] = 19.9;

        return matrix;
    }

    [Fact]
    public void AverageSilhouette_ShouldReturnValueBetweenMinusOneAndOne()
    {
        var matrix = CreatePerfectClustersMatrix();
        int[] labels = { 0, 0, 1, 1, 2, 2 };

        double silhouette = SilhouetteAnalyzer.AverageSilhouette(matrix, labels, 3);

        Assert.InRange(silhouette, -1.0, 1.0);
    }

    [Fact]
    public void AverageSilhouette_WithPerfectClusters_ShouldBeHigh()
    {
        var matrix = CreatePerfectClustersMatrix();
        int[] labels = { 0, 0, 1, 1, 2, 2 };

        double silhouette = SilhouetteAnalyzer.AverageSilhouette(matrix, labels, 3);

        // Для идеально разделённых кластеров силуэт должен быть близок к 1
        Assert.True(silhouette > 0.8);
    }

    [Theory]
    [InlineData(0.6, "Отличная кластеризация")]
    [InlineData(0.4, "Хорошая кластеризация")]
    [InlineData(0.2, "Слабая кластеризация")]
    [InlineData(0.05, "Кластеризация не удалась")]
    public void Interpret_ShouldReturnCorrectMessage(double silhouette, string expectedMessage)
    {
        string message = SilhouetteAnalyzer.Interpret(silhouette);

        Assert.Equal(expectedMessage, message);
    }

    [Fact]
    public void SilhouetteForGene_ShouldBeCalculatedCorrectly()
    {
        var matrix = new GeneMatrix(3, 2);
        matrix[0, 0] = 0; matrix[0, 1] = 0;
        matrix[1, 0] = 1; matrix[1, 1] = 1;
        matrix[2, 0] = 100; matrix[2, 1] = 100;

        int[] labels = { 0, 0, 1 };

        double silhouette = SilhouetteAnalyzer.SilhouetteForGene(matrix, 0, labels, 2);

        Assert.InRange(silhouette, -1.0, 1.0);
    }

    [Fact]
    public void AverageSilhouette_WithTwoClusters_ShouldBeReasonable()
    {
        var matrix = new GeneMatrix(4, 2);

        // Кластер 0: близкие значения
        matrix[0, 0] = 1.0; matrix[0, 1] = 1.0;
        matrix[1, 0] = 1.2; matrix[1, 1] = 1.1;

        // Кластер 1: далеко от кластера 0
        matrix[2, 0] = 10.0; matrix[2, 1] = 10.0;
        matrix[3, 0] = 10.5; matrix[3, 1] = 9.8;

        int[] labels = { 0, 0, 1, 1 };

        double silhouette = SilhouetteAnalyzer.AverageSilhouette(matrix, labels, 2);

        // Для хорошо разделённых кластеров силуэт должен быть > 0.5
        Assert.True(silhouette > 0.5, $"Silhouette should be > 0.5 for well-separated clusters, but got {silhouette}");
    }
}