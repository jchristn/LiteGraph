using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiteGraph
{
    /// <summary>
    /// GEXF file writer.
    /// </summary>
    public static class GexfWriter
    {
        /// <summary>
        /// Write a GEXF file.
        /// </summary>
        /// <param name="client">LiteGraphClient.</param>
        /// <param name="filename">Output filename.</param>
        public static void Write(LiteGraphClient client, string filename)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            GexfDocument doc = new GexfDocument();
            doc.Graph.DefaultEdgeType = "directed";
            doc.Graph.Attributes.AttributeList.Add(new Attribute("0", "props"));

            GraphResult nodesResult = client.GetAllNodes();
            GraphResult edgesResult = client.GetAllEdges();

            if (nodesResult != null && nodesResult.Data != null)
            {
                List<Node> nodes = ((JArray)nodesResult.Data).ToObject<List<Node>>();
                foreach (Node node in nodes)
                {
                    GexfNode gNode = new GexfNode(node.GUID, node.Name);
                    gNode.ValueList.Values.Add(new AttributeValue("0", node.Properties.ToJson(false)));
                    doc.Graph.NodeList.Nodes.Add(gNode);
                }
            }

            if (edgesResult != null && edgesResult.Data != null)
            {
                List<Edge> edges = ((JArray)edgesResult.Data).ToObject<List<Edge>>();
                foreach (Edge edge in edges)
                {
                    GexfEdge gEdge = new GexfEdge(edge.GUID, edge.FromGUID, edge.ToGUID);
                    gEdge.ValueList.Values.Add(new AttributeValue("0", edge.Properties.ToJson(false)));
                    doc.Graph.EdgeList.Edges.Add(gEdge);
                }
            }

            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                string xml = SerializeXml<GexfDocument>(doc, true);
                byte[] bytes = Encoding.UTF8.GetBytes(xml);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        private static string SerializeXml<T>(object obj, bool pretty = true)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            XmlSerializer xmls = new XmlSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                XmlWriterSettings settings = new XmlWriterSettings();

                if (pretty)
                {
                    settings.Encoding = Encoding.UTF8;
                    settings.Indent = true;
                    settings.NewLineChars = "\n";
                    settings.NewLineHandling = NewLineHandling.None;
                    settings.NewLineOnAttributes = false;
                    settings.ConformanceLevel = ConformanceLevel.Document;
                }
                else
                {
                    settings.Encoding = Encoding.UTF8;
                    settings.Indent = false;
                    settings.NewLineHandling = NewLineHandling.None;
                    settings.NewLineOnAttributes = false;
                    settings.ConformanceLevel = ConformanceLevel.Document;
                }

                using (XmlWriter writer = XmlTextWriter.Create(ms, settings))
                {
                    xmls.Serialize(writer, obj, ns);
                }

                string xml = Encoding.UTF8.GetString(ms.ToArray());

                string byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                while (xml.StartsWith(byteOrderMarkUtf8, StringComparison.Ordinal))
                {
                    xml = xml.Remove(0, byteOrderMarkUtf8.Length);
                }

                return xml;
            }
        }
    }
}
