namespace LiteGraph.Gexf
{
    using System;
    using System.Xml.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// Attributes.
    /// </summary>
    [XmlRoot(ElementName = "attributes", Namespace = "http://www.gexf.net/1.3")]
    public class GexfAttributes
    {
        /// <summary>
        /// Attributes.
        /// </summary>
        [XmlElement(ElementName = "attribute", Namespace = "http://www.gexf.net/1.3")]
        public List<GexfAttribute> AttributeList { get; set; } = new List<GexfAttribute>();

        /// <summary>
        /// Attribute class, i.e. 'node' or 'edge'.
        /// </summary>
        [XmlAttribute(AttributeName = "class")]
        public string Class
        {
            get
            {
                return _Class;
            }
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Class));
                List<string> validValues = new List<string> { "node", "edge" };
                if (!validValues.Contains(value)) throw new ArgumentException("Invalid value for Class: " + value);
                _Class = value;
            }
        }

        private string _Class = "node";

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public GexfAttributes()
        {

        }
    }
}
