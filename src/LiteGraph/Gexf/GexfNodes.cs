namespace LiteGraph.Gexf
{
    using System;
    using System.Xml.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// Nodes.
    /// </summary>
    [XmlRoot(ElementName = "nodes", Namespace = "http://www.gexf.net/1.3")]
    public class GexfNodes
    {
        /// <summary>
        /// List of nodes.
        /// </summary>
        [XmlElement(ElementName = "node", Namespace = "http://www.gexf.net/1.3")]
        public List<GexfNode> Nodes { get; set; } = new List<GexfNode>();

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public GexfNodes()
        {

        }
    }
}
