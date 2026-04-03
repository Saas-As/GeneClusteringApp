using GeneClusteringApp.Data;
using GeneClusteringApp.Evaluation;
using Xunit;

namespace GeneClusteringApp.Tests;

public class KSelectionTests
{
    private GeneMatrix CreateWellSeparatedData()
    {
        var matrix = new GeneMatrix(30, 2);

        // 10 генов в кластере 0
        for (int i = 0; i < 10; i++)
        {
            matrix[i, 0] = 1.0 + (i * 0.01);
            matrix[i, 1] = 1.0 + (i * 0.01);
        }

        // 10 генов в кластере 1
        for (int i = 10; i < 20; i++)
        {
            matrix[i, 0] = 5.0 + ((i - 10) * 0.01);
            matrix[i, 1] = 5.0 + ((i - 10) * 0.01);
        }

        // 10 генов в кластере 2
        for (int i = 20; i < 30; i++)
        {
            matrix[i, 0] = 10.0 + ((i - 20) * 0.01);
            matrix[i, 1] = 10.0 + ((i - 20) * 0.01);
        }

        return matrix;
    }

    [Fact]
    public void SelectK_ShouldReturnResultWithAllProperties()
    {
        var matrix = CreateWellSeparatedData();

        var result = KSelection.SelectK(matrix, minK: 2, maxK: 5);

        Assert.NotNull(result);
        Assert.NotNull(result.Inertias);
        Assert.NotNull(result.Silhouettes);
        Assert.True(result.Inertias.Count >= 4);
        Assert.True(result.Silhouettes.Count >= 4);
    }

    [Fact]
    public void SelectK_ElbowK_ShouldBeWithinRange()
    {
        var matrix = CreateWellSeparatedData();

        var result = KSelection.SelectK(matrix, minK: 2, maxK: 5);

        Assert.InRange(result.ElbowK, 2, 5);
    }

    [Fact]
    public void SelectK_SilhouetteK_ShouldBeWithinRange()
    {
        var matrix = CreateWellSeparatedData();

        var result = KSelection.SelectK(matrix, minK: 2, maxK: 5);

        Assert.InRange(result.SilhouetteK, 2, 5);
    }

    [Fact]
    public void SelectK_RecommendedK_ShouldNotBeZero()
    {
        var matrix = CreateWellSeparatedData();

        var result = KSelection.SelectK(matrix, minK: 2, maxK: 5);

        Assert.True(result.RecommendedK >= 2);
    }

    [Fact]
    public void SelectK_ShouldWorkWithMinimalParameters()
    {
        var matrix = new GeneMatrix(10, 3);
        for (int g = 0; g < 10; g++)
            for (int s = 0; s < 3; s++)
                matrix[g, s] = g * s;

        var result = KSelection.SelectK(matrix, minK: 2, maxK: 4);

        Assert.NotNull(result);
    }
}