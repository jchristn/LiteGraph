namespace LiteGraph.Gexf
{
    using System;
    using System.Xml.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// Graph.
    /// </summary>
    [XmlRoot(ElementName = "graph", Namespace = "http://www.gexf.net/1.3")]
    public class GexfGraph
    {
        /// <summary>
        /// Attributes.
        /// </summary>
        [XmlElement(ElementName = "attributes", Namespace = "http://www.gexf.net/1.3")]
        public GexfAttributes Attributes { get; set; } = new GexfAttributes();

        /// <summary>
        /// Nodes.
        /// </summary>
        [XmlElement(ElementName = "nodes", Namespace = "http://www.gexf.net/1.3")]
        public GexfNodes NodeList { get; set; } = new GexfNodes();

        /// <summary>
        /// Edges.
        /// </summary>
        [XmlElement(ElementName = "edges", Namespace = "http://www.gexf.net/1.3")]
        public GexfEdges EdgeList { get; set; } = new GexfEdges();

        /// <summary>
        /// Default edge type, i.e. 'directed'.
        /// </summary>
        [XmlAttribute(AttributeName = "defaultedgetype")]
        public string DefaultEdgeType
        {
            get
            {
                return _DefaultEdgeType;
            }
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(DefaultEdgeType));
                List<string> validValues = new List<string> { "directed", "undirected", "mutual" };
                if (!validValues.Contains(value)) throw new ArgumentException("Invalid value for DefaultEdgeType: " + value);
                _DefaultEdgeType = value;
            }
        }

        private string _DefaultEdgeType = "directed";

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public GexfGraph()
        {

        }
    }
}
