namespace LiteGraph.Sdk
{
    using ExpressionTree;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    /// <summary>
    /// Vector search request.
    /// </summary>
    public class VectorSearchRequest
    {
        #region Public-Members

        /// <summary>
        /// Tenant GUID.
        /// </summary>
        public Guid TenantGUID { get; set; } = default(Guid);

        /// <summary>
        /// Graph GUID.
        /// </summary>
        public Guid? GraphGUID { get; set; } = null;

        /// <summary>
        /// Vector search domain.
        /// </summary>
        public VectorSearchDomainEnum Domain { get; set; } = VectorSearchDomainEnum.Node;

        /// <summary>
        /// Vector search type.
        /// </summary>
        public VectorSearchTypeEnum SearchType { get; set; } = VectorSearchTypeEnum.CosineSimilarity;

        /// <summary>
        /// Search labels.
        /// </summary>
        public List<string> Labels
        {
            get
            {
                return _Labels;
            }
            set
            {
                if (value == null) value = new List<string>();
                _Labels = value;
            }
        }

        /// <summary>
        /// Search tags.
        /// </summary>
        public NameValueCollection Tags
        {
            get
            {
                return _Tags;
            }
            set
            {
                if (value == null) value = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
                _Tags = value;
            }
        }

        /// <summary>
        /// Expression.
        /// </summary>
        public Expr Expr { get; set; } = null;

        /// <summary>
        /// Embeddings.
        /// </summary>
        public List<float> Embeddings { get; set; } = null;

        #endregion

        #region Private-Members

        private List<string> _Labels = new List<string>();
        private NameValueCollection _Tags = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
        private List<float> _Embeddings = new List<float>();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public VectorSearchRequest()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
