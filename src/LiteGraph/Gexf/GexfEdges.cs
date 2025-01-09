namespace LiteGraph.Gexf
{
    using System;
    using System.Xml.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// Edges.
    /// </summary>
    [XmlRoot(ElementName = "edges", Namespace = "http://www.gexf.net/1.3")]
    public class GexfEdges
    {
        /// <summary>
        /// List of edges.
        /// </summary>
        [XmlElement(ElementName = "edge", Namespace = "http://www.gexf.net/1.3")]
        public List<GexfEdge> Edges { get; set; } = new List<GexfEdge>();

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public GexfEdges()
        {

        }
    }
}
