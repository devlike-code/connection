using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace connection
{
    public enum NodeType
    {
        Dot, Link, Label
    }

    public struct GraphDataLine
    {
        public NodeType Kind;
        public int Id;
        public int SourceId;
        public int TargetId;
        public Dictionary<string, string> Tags;
    }

    public class GraphData
    {
        public readonly List<GraphDataLine> Data = new();

        public static GraphData LoadFromFile(string filename)
        {
            GraphData graph = new();
            var lines = File.ReadAllLines(filename).ToList();
            lines.RemoveAt(0);

            int lineId = 2;
            foreach (var line in lines)
            {
                graph.Data.Add(line.ImportGraphDataLine(lineId));
                lineId++;
            }

            return graph;
        }
    }

    public static class StringGraphDataExtensions
    {
        public static GraphDataLine ImportGraphDataLine(this string line, int lineId)
        {
            NodeType kind;
            int id;
            int src;
            int tgt;
            Dictionary<string, string> tags;

            var parts = line.Split("|", 5, StringSplitOptions.TrimEntries);
            if (parts[0] == "dot") kind = NodeType.Dot;
            else if (parts[0] == "link") kind = NodeType.Link;
            else if (parts[0] == "label") kind = NodeType.Label;
            else throw new Exception($"Error reading node type: {parts[0]} (line {lineId})");

            if (!int.TryParse(parts[1], out id))
            {
                throw new Exception($"Error reading node id: {parts[1]} (line {lineId})");
            }

            if (!int.TryParse(parts[2], out src))
            {
                throw new Exception($"Error reading source node id: {parts[2]} (line {lineId})");
            }

            if (!int.TryParse(parts[3], out tgt))
            {
                throw new Exception($"Error reading target node id: {parts[3]} (line {lineId})");
            }

            tags = new();

            var tagParts = parts[4].Split(";", StringSplitOptions.TrimEntries);
            foreach (var tagPart in tagParts)
            {
                var sub = tagPart.Split(":", 2, StringSplitOptions.TrimEntries);
                var key = sub[0];
                var val = sub[1].Substring(0, sub[1].Length - 1).Substring(1);
                tags.Add(key, val);
            }

            return new GraphDataLine
            {
                Kind = kind,
                Id = id,
                SourceId = src,
                TargetId = tgt,
                Tags = tags
            };
        }
    }
}

