namespace LiteGraph.Gexf
{
    using System;
    using System.Xml.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// Attribute value.
    /// </summary>
    [XmlRoot(ElementName = "attvalue", Namespace = "http://www.gexf.net/1.3")]
    public class GexfAttributeValue
    {
        /// <summary>
        /// ID of the object to which the attribute is assigned.
        /// </summary>
        [XmlAttribute(AttributeName = "for")]
        public string For { get; set; }

        /// <summary>
        /// Value of the attribute.
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="forGuid">ID of the object to which the attribute is assigned.</param>
        /// <param name="value">Value of the attribute.</param>
        public GexfAttributeValue(string forGuid, string value)
        {
            For = forGuid ?? throw new ArgumentNullException(nameof(forGuid));
            if (value == null) value = "";
            Value = value;
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public GexfAttributeValue()
        {

        }
    }
}
