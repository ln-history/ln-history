using FluentAssertions;
using LightningGraph.GraphAlgorithms.Connectivity;
using LightningGraph.Model;
using LightningGraph.Tests.Faker;

namespace LightningGraph.Tests.GraphAlgorithms.Connectivity;

public class BridgeAnalysisTests
{
    [Test]
    [TestCase(1234, 10, 10)]
    [TestCase(1234, 20, 20)]
    public void FindBridges_ShouldReturnExpectedBridges_ForGraph(int seed, int vertexCount, int edgeCount)
    {
        // Arrange
        var graph = LightningFastGraphFaker.Create(seed, vertexCount, edgeCount);

        // Act
        var result = BridgeDetector.FindBridges(graph);

        // Assert
        result.Should().NotBeNull("because the bridge list should not be null.");
        result.Should().BeOfType<List<(string From, string To)>>("because the result should be a list of edges representing bridges.");
        result.Should().NotBeEmpty("because there should be at least one bridge in the graph if it is not fully connected.");
    }

    [Test]
    [TestCase(1234, 10, 25)]
    [TestCase(1234, 20, 100)]
    public void Analyze_ShouldReturnExpectedBridgeAnalysis_ForGraph(int seed, int vertexCount, int edgeCount)
    {
        // Arrange
        var graph = LightningFastGraphFaker.Create(seed, vertexCount, edgeCount);

        // Act
        var result = BridgeDetector.Analyze(graph);

        // Assert
        result.Should().NotBeNull("because the analysis result should not be null.");
        result.Should().BeOfType<BridgeAnalysis>("because the result should be of type BridgeAnalysis.");
    
        // Further assertions can be added to check properties of the BridgeAnalysis object if needed
        result.Bridges.Should().NotBeNull("because the bridges property in BridgeAnalysis should not be null.");
    }
}