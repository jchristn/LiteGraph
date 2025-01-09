namespace LiteGraph.Gexf
{
    using System;
    using System.Xml.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// Document metadata.
    /// </summary>
    [XmlRoot(ElementName = "meta", Namespace = "http://www.gexf.net/1.3")]
    public class GexfMetadata
    {
        /// <summary>
        /// Document creator.
        /// </summary>
        [XmlElement(ElementName = "creator", Namespace = "http://www.gexf.net/1.3")]
        public string Creator { get; set; } = "LiteGraph";

        /// <summary>
        /// Document description.
        /// </summary>
        [XmlElement(ElementName = "description", Namespace = "http://www.gexf.net/1.3")]
        public string Description { get; set; } = "Graph from LiteGraph https://github.com/jchristn/litegraph";

        /// <summary>
        /// Last modified date.
        /// </summary>
        [XmlAttribute(AttributeName = "lastmodifieddate")]
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public GexfMetadata()
        {

        }
    }
}
