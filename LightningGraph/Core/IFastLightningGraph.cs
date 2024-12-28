using LightningGraph.Model;

namespace LightningGraph.Implementations;

public interface IFastLightningGraph
{
    // NetworkMetrics AnalyzeNetwork();

    BridgeAnalysis AnalyzeBridges();

    double[,] CalculateAllPairsShortestPaths();

    CentralityMetrics AnalyzeCentrality();

    CentralityMetrics AnalyzeCentralityMonteCarlo(int runs = 1_000, int? seed = null);

    double CompareCentralityMethods(int runs = 1_000, int? seed = null);
}