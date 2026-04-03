using GeneClusteringApp.Data;
using GeneClusteringApp.Utils;
using Xunit;

namespace GeneClusteringApp.Tests;

public class ResultSaverTests
{
    private string GetTempFilePath() => Path.GetTempFileName();

    [Fact]
    public void SaveToCsv_ShouldCreateFile()
    {
        string tempFile = GetTempFilePath();

        try
        {
            var matrix = new GeneMatrix(5, 3);
            for (int g = 0; g < 5; g++)
                matrix.SetGeneName(g, $"Gene{g}");

            int[] labels = { 0, 0, 1, 1, 2 };
            double[][] centroids = new double[3][];
            for (int i = 0; i < 3; i++)
                centroids[i] = new double[3];

            ResultSaver.SaveToCsv(tempFile, matrix, labels, centroids, 0.75);

            Assert.True(File.Exists(tempFile));
            Assert.True(new FileInfo(tempFile).Length > 0);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void SaveToCsv_ShouldContainCorrectHeaders()
    {
        string tempFile = GetTempFilePath();

        try
        {
            var matrix = new GeneMatrix(2, 2);
            matrix.SetGeneName(0, "BRCA1");
            matrix.SetGeneName(1, "TP53");
            int[] labels = { 0, 1 };
            double[][] centroids = new double[2][];
            centroids[0] = new double[2];
            centroids[1] = new double[2];

            ResultSaver.SaveToCsv(tempFile, matrix, labels, centroids, 0.5);

            string content = File.ReadAllText(tempFile);
            Assert.Contains("GeneName,Cluster", content);
            Assert.Contains("BRCA1,0", content);
            Assert.Contains("TP53,1", content);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void SaveReport_ShouldIncludeClusterSizes()
    {
        string tempFile = GetTempFilePath();

        try
        {
            var matrix = new GeneMatrix(10, 2);
            int[] labels = { 0, 0, 0, 1, 1, 1, 2, 2, 2, 2 };
            double[][] centroids = new double[3][];
            for (int i = 0; i < 3; i++)
                centroids[i] = new double[2];

            ResultSaver.SaveReport(tempFile, matrix, labels, centroids, 100, 0.5, 5, 10);

            string content = File.ReadAllText(tempFile);
            Assert.Contains("РАЗМЕРЫ КЛАСТЕРОВ", content);
            Assert.Contains("Кластер 2: 4 генов", content);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}