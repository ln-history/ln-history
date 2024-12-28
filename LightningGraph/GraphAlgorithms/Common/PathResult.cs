namespace LightningGraph.GraphAlgorithms.Common;

public class PathResult
{
    public List<int> Path { get; }
    public double Distance { get; }
    public bool Exists { get; }

    private PathResult(List<int> path, double distance, bool exists)
    {
        Path = path;
        Distance = distance;
        Exists = exists;
    }

    public static PathResult NotFound => new(new List<int>(), -1, false);
    
    public static PathResult Found(List<int> path, double distance) => 
        new(path, distance, true);
}