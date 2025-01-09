namespace LiteGraph.Gexf
{
    using System;
    using System.Xml.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// Attribute.
    /// </summary>
    [XmlRoot(ElementName = "attribute", Namespace = "http://www.gexf.net/1.3")]
    public class GexfAttribute
    {
        /// <summary>
        /// Attribute ID.
        /// </summary>
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; } = null;

        /// <summary>
        /// Attribute title.
        /// </summary>
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; } = null;

        /// <summary>
        /// Attribute type.
        /// </summary>
        [XmlAttribute(AttributeName = "type")]
        public string Type
        {
            get
            {
                return _Type;
            }
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Type));
                List<string> validValues = new List<string> { "string", "integer", "float", "double", "boolean", "date" };
                if (!validValues.Contains(value)) throw new ArgumentException("Invalid value for Type: " + value);
                _Type = value;
            }
        }

        private string _Type = "string";

        /// <summary>
        /// Attribute default value.
        /// </summary>
        [XmlElement(ElementName = "default", Namespace = "http://www.gexf.net/1.3")]
        public string Default { get; set; } = null;

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="id">Attribute ID.</param>
        /// <param name="title">Attribute title.</param>
        /// <param name="type">Attribute type.</param>
        public GexfAttribute(string id, string title, string type = "string")
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public GexfAttribute()
        {

        }
    }
}
