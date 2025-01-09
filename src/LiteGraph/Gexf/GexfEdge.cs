namespace LiteGraph.Gexf
{
    using System;
    using System.Xml.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// Edge.
    /// </summary>
    [XmlRoot(ElementName = "edge", Namespace = "http://www.gexf.net/1.3")]
    public class GexfEdge
    {
        /// <summary>
        /// Attribute values.
        /// </summary>
        [XmlElement(ElementName = "attvalues", Namespace = "http://www.gexf.net/1.3")]
        public GexfAttributeValues ValueList { get; set; } = new GexfAttributeValues();

        /// <summary>
        /// ID.
        /// </summary>
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Source.
        /// </summary>
        [XmlAttribute(AttributeName = "source")]
        public string Source { get; set; }

        /// <summary>
        /// Target.
        /// </summary>
        [XmlAttribute(AttributeName = "target")]
        public string Target { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="id">ID.</param>
        /// <param name="source">Source.</param>
        /// <param name="target">Target.</param>
        public GexfEdge(Guid id, Guid source, Guid target)
        {
            Id = id.ToString();
            Source = source.ToString();
            Target = target.ToString();
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public GexfEdge()
        {

        }
    }
}
