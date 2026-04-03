using GeneClusteringApp.Data;
using GeneClusteringApp.Utils;
using Xunit;

namespace GeneClusteringApp.Tests;

public class NormalizerTests
{
    private GeneMatrix CreateTestMatrix()
    {
        var matrix = new GeneMatrix(3, 4);

        // Ген 0: [1, 2, 3, 4]
        matrix[0, 0] = 1; matrix[0, 1] = 2; matrix[0, 2] = 3; matrix[0, 3] = 4;

        // Ген 1: [10, 20, 30, 40]
        matrix[1, 0] = 10; matrix[1, 1] = 20; matrix[1, 2] = 30; matrix[1, 3] = 40;

        // Ген 2: [100, 200, 300, 400]
        matrix[2, 0] = 100; matrix[2, 1] = 200; matrix[2, 2] = 300; matrix[2, 3] = 400;

        return matrix;
    }

    [Fact]
    public void ZScoreNormalize_ShouldProduceMeanZero()
    {
        var matrix = CreateTestMatrix();
        var normalized = Normalizer.ZScoreNormalize(matrix);

        for (int g = 0; g < normalized.GeneCount; g++)
        {
            double[] values = normalized.GetGeneVector(g);
            double mean = values.Average();

            Assert.InRange(mean, -1e-10, 1e-10); // должно быть ~0
        }
    }

    [Fact]
    public void ZScoreNormalize_ShouldPreserveDimensions()
    {
        var matrix = CreateTestMatrix();
        var normalized = Normalizer.ZScoreNormalize(matrix);

        Assert.Equal(matrix.GeneCount, normalized.GeneCount);
        Assert.Equal(matrix.SampleCount, normalized.SampleCount);
    }

    [Fact]
    public void ZScoreNormalize_ShouldPreserveGeneAndSampleNames()
    {
        var matrix = CreateTestMatrix();
        matrix.SetGeneName(0, "BRCA1");
        matrix.SetSampleName(0, "Patient1");

        var normalized = Normalizer.ZScoreNormalize(matrix);

        Assert.Equal("BRCA1", normalized.GetGeneName(0));
        Assert.Equal("Patient1", normalized.GetSampleName(0));
    }

    [Fact]
    public void MinMaxNormalize_ShouldProduceValuesInZeroOneRange()
    {
        var matrix = CreateTestMatrix();
        var normalized = Normalizer.MinMaxNormalize(matrix);

        for (int g = 0; g < normalized.GeneCount; g++)
        {
            for (int s = 0; s < normalized.SampleCount; s++)
            {
                double value = normalized[g, s];
                Assert.InRange(value, 0.0, 1.0);
            }
        }
    }

    [Fact]
    public void MinMaxNormalize_MinShouldBeZero_MaxShouldBeOne()
    {
        var matrix = new GeneMatrix(1, 4);
        matrix[0, 0] = 5; matrix[0, 1] = 10; matrix[0, 2] = 15; matrix[0, 3] = 20;

        var normalized = Normalizer.MinMaxNormalize(matrix);

        Assert.Equal(0.0, normalized[0, 0]); // min
        Assert.Equal(1.0, normalized[0, 3]); // max
    }

    [Fact]
    public void Log2Normalize_ShouldHandleZeroValues()
    {
        var matrix = new GeneMatrix(1, 3);
        matrix[0, 0] = 0;
        matrix[0, 1] = 1;
        matrix[0, 2] = 3;

        var normalized = Normalizer.Log2Normalize(matrix);

        // log2(0+1) = 0
        Assert.Equal(0.0, normalized[0, 0]);
        // log2(1+1) = 1
        Assert.Equal(1.0, normalized[0, 1]);
    }

    [Fact]
    public void ZScoreNormalize_ConstantVector_ShouldProduceZeros()
    {
        var matrix = new GeneMatrix(1, 5);
        for (int s = 0; s < 5; s++)
            matrix[0, s] = 42.0;

        var normalized = Normalizer.ZScoreNormalize(matrix);

        for (int s = 0; s < 5; s++)
            Assert.Equal(0.0, normalized[0, s]);
    }
}