namespace LiteGraph
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Label metadata.
    /// </summary>
    public class LabelMetadata
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
        /// Key.
        /// </summary>
        public string Label { get; set; } = string.Empty;

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

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public LabelMetadata()
        {

        }

        /// <summary>
        /// Create a list of label metadata from a list of labels.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="graphGuid">Graph GUID.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="edgeGuid">Edge GUID.</param>
        /// <param name="labels">Labels.</param>
        /// <returns>List of label metadata.</returns>
        public static List<LabelMetadata> FromListString(Guid tenantGuid, Guid graphGuid, Guid? nodeGuid, Guid? edgeGuid, List<string> labels)
        {
            if (labels == null) return null;
            if (labels.Count < 1) return new List<LabelMetadata>();

            List<LabelMetadata> ret = new List<LabelMetadata>();

            foreach (string label in labels)
            {
                ret.Add(new LabelMetadata
                {
                    TenantGUID = tenantGuid,
                    GraphGUID = graphGuid,
                    NodeGUID = nodeGuid,
                    EdgeGUID = edgeGuid,
                    Label = label
                });
            }

            return ret;
        }

        /// <summary>
        /// Create a list of strings for all labels in the list.
        /// </summary>
        /// <param name="labels">Labels.</param>
        /// <returns>Labels.</returns>
        public static List<string> ToListString(List<LabelMetadata> labels)
        {
            if (labels == null) return null;
            if (labels.Count < 1) return new List<string>();

            List<string> ret = new List<string>();
            foreach (LabelMetadata label in labels)
            {
                ret.Add(label.Label);
            }

            return ret;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}