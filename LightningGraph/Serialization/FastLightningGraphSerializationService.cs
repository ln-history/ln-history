using LightningGraph.Core;

namespace LightningGraph.Serialization;

public static class LightningFastGraphDeserializationService
{
    public static LightningFastGraph Deserialize(string serializedGraph)
    {
        // Convert JSON string back to LightningFastGraph
        return LightningFastGraph.DeserializeFromJson(serializedGraph);
    }
}