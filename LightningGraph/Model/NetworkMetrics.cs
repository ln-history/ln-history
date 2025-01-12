namespace LightningGraph.Model;

public record NetworkMetrics
{
    public int Diameter { get; init; }
    public double AveragePathLength { get; init; }
    public double AverageDegree { get; init; }
    public double AverageLocalClusteringCoefficient { get; init; }

    public double GlobalClusteringCoefficient { get; set; }
    public double Density { get; init; }
    public ICollection<string> NodeIdsSortedByHighestDegreeTop100 { get; init; }
}