namespace LiteGraph.Gexf
{
    using System;
    using System.Xml.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// Attribute values.
    /// </summary>
    [XmlRoot(ElementName = "attvalues", Namespace = "http://www.gexf.net/1.3")]
    public class GexfAttributeValues
    {
        /// <summary>
        /// Attribute values.
        /// </summary>
        [XmlElement(ElementName = "attvalue", Namespace = "http://www.gexf.net/1.3")]
        public List<GexfAttributeValue> Values { get; set; } = new List<GexfAttributeValue>();

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public GexfAttributeValues()
        {

        }
    }
}
