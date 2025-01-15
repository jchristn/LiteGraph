namespace LiteGraph
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Vector metadata.
    /// </summary>
    public class VectorMetadata
    {
        #region Public-Members

        /// <summary>
        /// GUID.
        /// </summary>
        public Guid GUID { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Tenant GUID.
        /// </summary>
        public Guid TenantGUID { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Graph GUID.
        /// </summary>
        public Guid? GraphGUID { get; set; } = null;

        /// <summary>
        /// Node GUID.
        /// </summary>
        public Guid? NodeGUID { get; set; } = null;

        /// <summary>
        /// Edge GUID.
        /// </summary>
        public Guid? EdgeGUID { get; set; } = null;

        /// <summary>
        /// Model.
        /// </summary>
        public string Model { get; set; } = null;

        /// <summary>
        /// Dimensionality.
        /// </summary>
        public int Dimensionality
        {
            get
            {
                return _Dimensionality;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Dimensionality));
                _Dimensionality = value;
            }
        }

        /// <summary>
        /// Content.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Vectors.
        /// </summary>
        public List<float> Vectors { get; set; } = null;

        /// <summary>
        /// Creation timestamp, in UTC.
        /// </summary>
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp from last update, in UTC.
        /// </summary>
        public DateTime LastUpdateUtc { get; set; } = DateTime.UtcNow;

        #endregion

        #region Private-Members

        private int _Dimensionality = 0;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public VectorMetadata()
        {

        }

        /// <summary>
        /// Create a list of vector metadata from a list of floats.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        /// <param name="embeddings">Embeddings.</param>
        /// <returns>List of vector metadata.</returns>
        public static List<VectorMetadata> FromFloatsList(Guid tenantGuid, Guid graphGuid, Guid? nodeGuid, Guid? edgeGuid, List<List<float>> embeddings)
        {
            if (embeddings == null) return null;
            if (embeddings.Count < 1) return new List<VectorMetadata>();

            List<VectorMetadata> ret = new List<VectorMetadata>();

            foreach (List<float> floats in embeddings)
            {
                ret.Add(new VectorMetadata
                {
                    TenantGUID = tenantGuid,
                    GraphGUID = graphGuid,
                    NodeGUID = nodeGuid,
                    EdgeGUID = edgeGuid,
                    Vectors = floats
                });
            }

            return ret;
        }

        /// <summary>
        /// Create a list float lists for all vector metadata entries.
        /// </summary>
        /// <param name="vectors">Vectors.</param>
        /// <returns>Labels.</returns>
        public static List<List<float>> ToListString(List<VectorMetadata> vectors)
        {
            if (vectors == null) return null;
            if (vectors.Count < 1) return new List<List<float>>();

            List<List<float>> ret = new List<List<float>>();
            foreach (VectorMetadata vector in vectors)
            {
                ret.Add(vector.Vectors);
            }

            return ret;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }}