namespace LiteGraph.Gexf
{
    using System;
    using System.Xml.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// Node.
    /// </summary>
    [XmlRoot(ElementName = "node", Namespace = "http://www.gexf.net/1.3")]
    public class GexfNode
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
        /// Label.
        /// </summary>
        [XmlAttribute(AttributeName = "label")]
        public string Label { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="id">ID.</param>
        /// <param name="label">Label.</param>
        public GexfNode(Guid id, string label)
        {
            Id = id.ToString();
            Label = label ?? throw new ArgumentNullException(nameof(label));
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public GexfNode()
        {

        }
    }
}
