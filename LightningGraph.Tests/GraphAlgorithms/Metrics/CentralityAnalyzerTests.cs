using FluentAssertions;
using LightningGraph.GraphAlgorithms.Metrics;
using LightningGraph.Model;
using LightningGraph.Tests.Faker;

namespace LightningGraph.Tests.GraphAlgorithms.Metrics;

[TestFixture]
public class CentralityAnalyzerTests
{
    [Test]
    [TestCase(1234, 10, 25)]
    [TestCase(1234, 20, 100)]
    public void CalculateCentralityAnalytically_ShouldReturnExpectedMetrics_ForUnweightedGraph(int seed, int vertexCount, int edgeCount)
    {
        // Arrange
        var graph = LightningFastGraphFaker.Create(seed, vertexCount, edgeCount);

        // Act
        var result = CentralityAnalyzer.CalculateCentralityAnalytically(graph);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<CentralityMetrics>();
        result.BetweennessCentrality.Should().NotBeEmpty("Betweenness centrality should not be empty.");
        result.MostCentralVertex.Should().NotBeNull("Most central vertex should not be null.");
        result.Runs.Should().Be(graph.NodeCount,"Runs should equal the number of nodes in the graph.");
    }

    [Test]
    [TestCase(1234, 10, 25, 10_000)]
    [TestCase(1234, 20, 100, 10_000)]
    public void CalculateCentralityAnalytically_ShouldHandleWeightedGraph(int seed, int vertexCount, int edgeCount, int paymentSizeSats)
    {
        // Arrange
        var graph = LightningFastGraphFaker.Create(seed, vertexCount, edgeCount);

        // Act
        var result = CentralityAnalyzer.CalculateCentralityAnalytically(graph, paymentSizeSats);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<CentralityMetrics>();
        result.BetweennessCentrality.Should().NotBeEmpty("because betweenness centrality should not be empty.");
        result.MostCentralVertex.Should().NotBeNull("because the most central vertex should not be null.");
        result.Runs.Should().Be(graph.NodeCount, "because runs should equal the number of nodes in the graph.");
    }
}