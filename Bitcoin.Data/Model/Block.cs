using CouchDB.Driver.Types;
using Newtonsoft.Json;

namespace Bitcoin.Data.Model;

public class Block : CouchDocument
{
    [JsonProperty("_id")]
    public string Id { get; set; }
    [JsonProperty("hash")]
    public string Hash { get; set; }
    [JsonProperty("confirmations")]
    public int Confirmations { get; set; }
    [JsonProperty("height")]
    public int Height { get; set; }
    [JsonProperty("time")]
    public long Time { get; set; }
    [JsonProperty("medianTime")]
    public long MedianTime { get; set; }
    [JsonProperty("nonce")]
    public long Nonce { get; set; }
    [JsonProperty("difficulty")]
    public double Difficulty { get; set; }
    [JsonProperty("nTx")]
    public int NTx { get; set; }
    [JsonProperty("strippedSize")]
    public int StrippedSize { get; set; }
    [JsonProperty("size")]
    public int Size { get; set; }
    [JsonProperty("weight")]
    public int Weight { get; set; }
    [JsonProperty("totalFees")]
    public double TotalFees { get; set; }
    [JsonProperty("subsidy")]
    public double Subsidy { get; set; }
}