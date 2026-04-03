using GeneClusteringApp.Data;
using Xunit;

namespace GeneClusteringApp.Tests;

public class GeneMatrixTests
{
    [Fact]
    public void Constructor_ShouldCreateMatrixWithCorrectDimensions()
    {
        int genes = 10;
        int samples = 5;

        var matrix = new GeneMatrix(genes, samples);

        Assert.Equal(genes, matrix.GeneCount);
        Assert.Equal(samples, matrix.SampleCount);
    }

    [Fact]
    public void Indexer_ShouldSetAndGetValuesCorrectly()
    {
        var matrix = new GeneMatrix(5, 5);

        matrix[2, 3] = 42.5;

        Assert.Equal(42.5, matrix[2, 3]);
    }

    [Fact]
    public void GetGeneVector_ShouldReturnCorrectVector()
    {
        var matrix = new GeneMatrix(3, 4);

        matrix[0, 0] = 1.0; matrix[0, 1] = 2.0; matrix[0, 2] = 3.0; matrix[0, 3] = 4.0;
        matrix[1, 0] = 5.0; matrix[1, 1] = 6.0; matrix[1, 2] = 7.0; matrix[1, 3] = 8.0;

        double[] vector = matrix.GetGeneVector(1);

        Assert.Equal(4, vector.Length);
        Assert.Equal(5.0, vector[0]);
        Assert.Equal(6.0, vector[1]);
        Assert.Equal(7.0, vector[2]);
        Assert.Equal(8.0, vector[3]);
    }

    [Fact]
    public void GetGeneVector_InvalidIndex_ShouldThrowException()
    {
        var matrix = new GeneMatrix(5, 5);

        Assert.Throws<ArgumentOutOfRangeException>(() => matrix.GetGeneVector(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => matrix.GetGeneVector(10));
    }

    [Fact]
    public void GeneNames_ShouldBeSetAndRetrieved()
    {
        var matrix = new GeneMatrix(3, 3);

        matrix.SetGeneName(0, "BRCA1");
        matrix.SetGeneName(1, "TP53");
        matrix.SetGeneName(2, "MYC");

        Assert.Equal("BRCA1", matrix.GetGeneName(0));
        Assert.Equal("TP53", matrix.GetGeneName(1));
        Assert.Equal("MYC", matrix.GetGeneName(2));
    }

    [Fact]
    public void SampleNames_ShouldBeSetAndRetrieved()
    {
        var matrix = new GeneMatrix(3, 3);

        matrix.SetSampleName(0, "Patient_A");
        matrix.SetSampleName(1, "Patient_B");
        matrix.SetSampleName(2, "Patient_C");

        Assert.Equal("Patient_A", matrix.GetSampleName(0));
        Assert.Equal("Patient_B", matrix.GetSampleName(1));
        Assert.Equal("Patient_C", matrix.GetSampleName(2));
    }

    [Fact]
    public void DefaultNames_ShouldBeGenerated()
    {
        var matrix = new GeneMatrix(3, 3);

        Assert.Equal("Gene_1", matrix.GetGeneName(0));
        Assert.Equal("Gene_2", matrix.GetGeneName(1));
        Assert.Equal("Sample_1", matrix.GetSampleName(0));
    }

    [Fact]
    public void PrintInfo_ShouldNotThrowException()
    {
        var matrix = new GeneMatrix(100, 50);

        var exception = Record.Exception(() => matrix.PrintInfo());
        Assert.Null(exception);
    }

    [Fact]
    public void PrintPreview_ShouldNotThrowException()
    {
        var matrix = new GeneMatrix(20, 15);

        var exception = Record.Exception(() => matrix.PrintPreview(5, 5));
        Assert.Null(exception);
    }
}