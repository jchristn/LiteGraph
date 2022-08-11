using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace LiteGraph
{
	/*
	 * 
	 * Refer to https://gephi.org/gexf/format/schema.html
	 * 
	 * 
	 */

	/// <summary>
	/// Document metadata.
	/// </summary>
	[XmlRoot(ElementName = "meta", Namespace = "http://www.gexf.net/1.3")]
	public class Meta
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
		public DateTime LastModifiedDate { get; set; } = DateTime.Now.ToUniversalTime();

		/// <summary>
		/// Instantiate the object.
		/// </summary>
		public Meta()
        {

        }
	}

	/// <summary>
	/// Attribute.
	/// </summary>
	[XmlRoot(ElementName = "attribute", Namespace = "http://www.gexf.net/1.3")]
	public class Attribute
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
				if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Type));
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
		public Attribute(string id, string title, string type = "string")
        {
			Id = id ?? throw new ArgumentNullException(nameof(id));
			Title = title ?? throw new ArgumentNullException(nameof(title));
			Type = type ?? throw new ArgumentNullException(nameof(type));
		}

		/// <summary>
		/// Instantiate the object.
		/// </summary>
		public Attribute()
		{

		}
	}

	/// <summary>
	/// Attributes.
	/// </summary>
	[XmlRoot(ElementName = "attributes", Namespace = "http://www.gexf.net/1.3")]
	public class Attributes
	{
		/// <summary>
		/// Attributes.
		/// </summary>
		[XmlElement(ElementName = "attribute", Namespace = "http://www.gexf.net/1.3")]
		public List<Attribute> AttributeList { get; set; } = new List<Attribute>();

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
				if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Class));
				List<string> validValues = new List<string> { "node", "edge" };
				if (!validValues.Contains(value)) throw new ArgumentException("Invalid value for Class: " + value);
				_Class = value;
			}
        }

		private string _Class = "node";

		/// <summary>
		/// Instantiate the object.
		/// </summary>
		public Attributes()
		{

		}
	}

	/// <summary>
	/// Attribute value.
	/// </summary>
	[XmlRoot(ElementName = "attvalue", Namespace = "http://www.gexf.net/1.3")]
	public class AttributeValue
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
		public AttributeValue(string forGuid, string value)
        {
			For = forGuid ?? throw new ArgumentNullException(nameof(forGuid));
			Value = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <summary>
		/// Instantiate the object.
		/// </summary>
		public AttributeValue()
		{

		}
	}

	/// <summary>
	/// Attribute values.
	/// </summary>
	[XmlRoot(ElementName = "attvalues", Namespace = "http://www.gexf.net/1.3")]
	public class AttributeValues
	{
		/// <summary>
		/// Attribute values.
		/// </summary>
		[XmlElement(ElementName = "attvalue", Namespace = "http://www.gexf.net/1.3")]
		public List<AttributeValue> Values { get; set; } = new List<AttributeValue>();

		/// <summary>
		/// Instantiate the object.
		/// </summary>
		public AttributeValues()
		{

		}
	}

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
		public AttributeValues ValueList { get; set; } = new AttributeValues();

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
		public GexfNode(string id, string label)
        {
			Id = id ?? throw new ArgumentNullException(nameof(id));
			Label = label ?? throw new ArgumentNullException(nameof(label));
		}

		/// <summary>
		/// Instantiate the object.
		/// </summary>
		public GexfNode()
		{

		}
	}

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
		public AttributeValues ValueList { get; set; } = new AttributeValues();

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
		public GexfEdge(string id, string source, string target)
        {
			Id = id ?? throw new ArgumentNullException(nameof(id));
			Source = source ?? throw new ArgumentNullException(nameof(source));
			Target = target ?? throw new ArgumentNullException(nameof(target));
		}

		/// <summary>
		/// Instantiate the object.
		/// </summary>
		public GexfEdge()
		{

		}
	}

	/// <summary>
	/// Graph.
	/// </summary>
	[XmlRoot(ElementName = "graph", Namespace = "http://www.gexf.net/1.3")]
	public class Graph
	{
		/// <summary>
		/// Attributes.
		/// </summary>
		[XmlElement(ElementName = "attributes", Namespace = "http://www.gexf.net/1.3")]
		public Attributes Attributes { get; set; } = new Attributes();

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
				if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(DefaultEdgeType));
				List<string> validValues = new List<string> { "directed", "undirected", "mutual" };
				if (!validValues.Contains(value)) throw new ArgumentException("Invalid value for DefaultEdgeType: " + value);
				_DefaultEdgeType = value;
			}
        }

		private string _DefaultEdgeType = "directed";

		/// <summary>
		/// Instantiate the object.
		/// </summary>
		public Graph()
		{

		}
	}

	/// <summary>
	/// Graph Exchange XML Format document.
	/// </summary>
	[XmlRoot(ElementName = "gexf", Namespace = "http://www.gexf.net/1.3")]
	public class GexfDocument
	{
		/// <summary>
		/// Document metadata.
		/// </summary>
		[XmlElement(ElementName = "meta", Namespace = "http://www.gexf.net/1.3")]
		public Meta Meta { get; set; } = new Meta();

		/// <summary>
		/// Graph.  
		/// </summary>
		[XmlElement(ElementName = "graph", Namespace = "http://www.gexf.net/1.3")]
		public Graph Graph { get; set; } = new Graph();

		/// <summary>
		/// XML namespace.
		/// </summary>
		[XmlAttribute(AttributeName = "xmlns")]
		public string Xmlns { get; set; } = "http://www.gexf.net/1.3";
		 
		/// <summary>
		/// Schema location.
		/// </summary>
		[XmlAttribute(AttributeName = "schemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
		public string SchemaLocation { get; set; } = "http://www.gexf.net/1.3 http://www.gexf.net/1.3/gexf.xsd";

		/// <summary>
		/// Version.
		/// </summary>
		[XmlAttribute(AttributeName = "version")]
		public string Version { get; set; } = "1.3";

		/// <summary>
		/// Instantiate the object.
		/// </summary>
		public GexfDocument()
		{

		}
	}
} 
