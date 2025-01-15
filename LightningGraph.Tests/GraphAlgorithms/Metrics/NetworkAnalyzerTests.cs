using FluentAssertions;
using LightningGraph.GraphAlgorithms.Metrics;
using LightningGraph.Tests.Faker;

namespace LightningGraph.Tests.GraphAlgorithms.Metrics;

public class NetworkAnalyzerTests
{
    [Test]
    [TestCase(1234, 10, 25)]
    [TestCase(1234, 20, 100)]
    public void CalculateDiameter_ShouldReturnExpectedDiameter_ForGraph(int seed, int vertexCount, int edgeCount)
    {
        // Arrange
        var graph = LightningFastGraphFaker.Create(seed, vertexCount, edgeCount);

        // Act
        var result = NetworkAnalyzer.CalculateDiameter(graph);

        // Assert
        result.Should().BeGreaterThan(0, "because the diameter of a graph should be greater than zero.");
    }

    [Test]
    [TestCase(1234, 10, 25)]
    [TestCase(1234, 20, 100)]
    public void CalculateAveragePathLength_ShouldReturnExpectedLength_ForGraph(int seed, int vertexCount, int edgeCount)
    {
        // Arrange
        var graph = LightningFastGraphFaker.Create(seed, vertexCount, edgeCount);

        // Act
        var result = NetworkAnalyzer.CalculateAveragePathLength(graph);

        // Assert
        result.Should().BeGreaterThan(0, "because the average path length should be a positive number.");
    }

    [Test]
    [TestCase(1234, 10, 25)]
    [TestCase(1234, 20, 100)]
    public void CalculateAverageOutDegree_ShouldReturnExpectedValue_ForGraph(int seed, int vertexCount, int edgeCount)
    {
        // Arrange
        var graph = LightningFastGraphFaker.Create(seed, vertexCount, edgeCount);

        // Act
        var result = NetworkAnalyzer.CalculateAverageOutDegree(graph);

        // Assert
        result.Should().BeGreaterThan(0, "because the average out-degree should be a positive number.");
    }

    [Test]
    [TestCase(1234, 10, 25)]
    [TestCase(1234, 20, 100)]
    public void CalculateAverageLocalClusteringCoefficient_ShouldReturnExpectedCoefficient_ForGraph(int seed, int vertexCount, int edgeCount)
    {
        // Arrange
        var graph = LightningFastGraphFaker.Create(seed, vertexCount, edgeCount);

        // Act
        var result = NetworkAnalyzer.CalculateAverageLocalClusteringCoefficient(graph);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0, "because the average local clustering coefficient should not be negative.");
    }

    [Test]
    [TestCase(1234, 10, 25)]
    [TestCase(1234, 20, 100)]
    public void CalculateGlobalClusteringCoefficient_ShouldReturnExpectedCoefficient_ForGraph(int seed, int vertexCount, int edgeCount)
    {
        // Arrange
        var graph = LightningFastGraphFaker.Create(seed, vertexCount, edgeCount);

        // Act
        var result = NetworkAnalyzer.CalculateGlobalClusteringCoefficient(graph);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0, "because the global clustering coefficient should not be negative.");
    }

    [Test]
    [TestCase(1234, 10, 25)]
    [TestCase(1234, 20, 100)]
    public void CalculateDensity_ShouldReturnExpectedDensity_ForGraph(int seed, int vertexCount, int edgeCount)
    {
        // Arrange
        var graph = LightningFastGraphFaker.Create(seed, vertexCount, edgeCount);

        // Act
        var result = NetworkAnalyzer.CalculateDensity(graph);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0, "because the density of a graph should not be negative.");
    }

    [Test]
    [TestCase(1234, 10, 25, 5)]
    [TestCase(1234, 20, 100, 10)]
    public void GetTopNodesByDegree_ShouldReturnExpectedTopNodes_ForGraph(int seed, int vertexCount, int edgeCount, int count)
    {
        // Arrange
        var graph = LightningFastGraphFaker.Create(seed, vertexCount, edgeCount);

        // Act
        var result = NetworkAnalyzer.GetTopNodesByDegree(graph, count);

        // Assert
        result.Should().NotBeNullOrEmpty("because the top nodes by degree should not be null or empty.");
        result.Should().HaveCount(count, "because the number of returned top nodes should equal the requested count.");
    }
}