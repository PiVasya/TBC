using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TBC.Services.CodeGen
{
    public class FlowSchema
    {
        [JsonProperty("nodes")] public List<Node> Nodes { get; set; } = new();
        [JsonProperty("edges")] public List<Edge> Edges { get; set; } = new();
    }

    public class Node 
    {
        [JsonProperty("id")] public string Id { get; set; } = "";
        [JsonProperty("type")] public string Type { get; set; } = "";
        [JsonProperty("data")] public JObject Data { get; set; } = new();
    }

    public class Edge
    {
        [JsonProperty("source")] public string Source { get; set; } = "";
        [JsonProperty("target")] public string Target { get; set; } = "";
    }
}
