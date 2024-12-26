namespace LiteGraph.Server.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using WatsonWebserver.Core;

    /// <summary>
    /// Request context.
    /// </summary>
    public class RequestContext
    {
        #region Public-Members

        /// <summary>
        /// Request GUID.
        /// </summary>
        public Guid RequestGuid { get; set; } = Guid.NewGuid();

        /// <summary>
        /// API version.
        /// </summary>
        public ApiVersionEnum ApiVersion { get; set; } = ApiVersionEnum.Unknown;

        /// <summary>
        /// Requestor IP.
        /// </summary>
        public string Ip
        {
            get
            {
                if (_Http != null)
                    return _Http.Request.Source.IpAddress;

                return null;
            }
        }
        
        /// <summary>
        /// HTTP context.
        /// </summary>
        public HttpContextBase Http
        {
            get
            {
                return _Http;
            }
        }

        /// <summary>
        /// Authentication context.
        /// </summary>
        public AuthenticationContext Authentication
        {
            get
            {
                return _Authentication;
            }
        }

        /// <summary>
        /// Authorization context.
        /// </summary>
        public AuthorizationContext Authorization
        {
            get
            {
                return _Authorization;
            }
        }

        #region Objects

        /// <summary>
        /// Tenant GUID.
        /// </summary>
        public Guid? TenantGUID { get; set; } = null;

        /// <summary>
        /// Tenant.
        /// </summary>
        public TenantMetadata Tenant { get; set; } = null;

        /// <summary>
        /// Graph GUID.
        /// </summary>
        public Guid? GraphGUID { get; set; } = null;

        /// <summary>
        /// Graph.
        /// </summary>
        public Graph Graph { get; set; } = null;

        /// <summary>
        /// GUIDs.
        /// </summary>
        public List<Guid> GUIDs { get; set; } = null;

        /// <summary>
        /// Node GUID.
        /// </summary>
        public Guid? NodeGUID { get; set; } = null;

        /// <summary>
        /// Node.
        /// </summary>
        public Node Node { get; set; } = null;

        /// <summary>
        /// Nodes.
        /// </summary>
        public List<Node> Nodes { get; set; } = null;

        /// <summary>
        /// Edge GUID.
        /// </summary>
        public Guid? EdgeGUID { get; set; } = null;

        /// <summary>
        /// Edge.
        /// </summary>
        public Edge Edge { get; set; } = null;

        /// <summary>
        /// Edges.
        /// </summary>
        public List<Edge> Edges { get; set; } = null;

        /// <summary>
        /// Tag GUID.
        /// </summary>
        public Guid? TagGUID { get; set; } = null;

        /// <summary>
        /// Tag.
        /// </summary>
        public TagMetadata Tag { get; set; } = null;

        /// <summary>
        /// Tags.
        /// </summary>
        public List<TagMetadata> Tags { get; set; } = null;

        /// <summary>
        /// User GUID.
        /// </summary>
        public Guid? UserGUID { get; set; } = null;

        /// <summary>
        /// User.
        /// </summary>
        public UserMaster User { get; set; } = null;

        /// <summary>
        /// Credential GUID.
        /// </summary>
        public Guid? CredentialGUID { get; set; } = null;

        /// <summary>
        /// Credential.
        /// </summary>
        public Credential Credential { get; set; } = null;

        /// <summary>
        /// Existence request.
        /// </summary>
        public ExistenceRequest ExistenceRequest { get; set; } = null;

        /// <summary>
        /// Search request.
        /// </summary>
        public SearchRequest SearchRequest { get; set; } = null;

        /// <summary>
        /// Route request.
        /// </summary>
        public RouteRequest RouteRequest { get; set; } = null;

        #endregion

        #region Query

        /// <summary>
        /// Number of records to skip in enumeration.
        /// </summary>
        public int Skip
        {
            get
            {
                return _Skip;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Skip));
                _Skip = value;
            }
        }

        /// <summary>
        /// Force deletion.
        /// </summary>
        public bool Force { get; set; } = false;

        /// <summary>
        /// Include data.
        /// </summary>
        public bool IncludeData { get; set; } = false;

        /// <summary>
        /// From GUID.
        /// </summary>
        public Guid? FromGUID { get; set; } = null;

        /// <summary>
        /// To GUID.
        /// </summary>
        public Guid? ToGUID { get; set; } = null;

        #endregion

        #endregion

        #region Private-Members

        private HttpContextBase _Http = null;
        private AuthenticationContext _Authentication = new AuthenticationContext();
        private AuthorizationContext _Authorization = new AuthorizationContext();

        private int _Skip = 0;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="ctx">HTTP context.</param>
        public RequestContext(HttpContextBase ctx)
        {
            _Http = ctx ?? throw new ArgumentNullException(nameof(ctx));

            SetApiVersion();
            SetAuthValues();
            SetRequestValues();
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        private void SetApiVersion()
        {
            if (_Http.Request.Url.Parameters.AllKeys.Contains("apiVersion"))
            {
                string apiVersion = _Http.Request.Url.Parameters.Get("apiVersion");
                if (!String.IsNullOrEmpty(apiVersion))
                {
                    if (apiVersion.Equals("v1.0")) ApiVersion = ApiVersionEnum.V1_0;
                }
            }
        }

        private void SetAuthValues()
        {
            if (_Http.Request.HeaderExists(Constants.AuthorizationHeader))
                _Authentication.BearerToken = _Http.Request.RetrieveHeaderValue(Constants.AuthorizationHeader);
        }

        private void SetRequestValues()
        {
            if (_Http.Request.Url.Parameters.Count > 0)
            {
                if (_Http.Request.Url.Parameters.AllKeys.Contains("tenantGuid")) TenantGUID = Guid.Parse(_Http.Request.Url.Parameters.Get("tenantGuid"));
                if (_Http.Request.Url.Parameters.AllKeys.Contains("userGuid")) UserGUID = Guid.Parse(_Http.Request.Url.Parameters.Get("userGuid"));
                if (_Http.Request.Url.Parameters.AllKeys.Contains("credentialGuid")) CredentialGUID = Guid.Parse(_Http.Request.Url.Parameters.Get("credentialGuid"));
                if (_Http.Request.Url.Parameters.AllKeys.Contains("graphGuid")) GraphGUID = Guid.Parse(_Http.Request.Url.Parameters.Get("graphGuid"));
                if (_Http.Request.Url.Parameters.AllKeys.Contains("nodeGuid")) NodeGUID = Guid.Parse(_Http.Request.Url.Parameters.Get("nodeGuid"));
                if (_Http.Request.Url.Parameters.AllKeys.Contains("edgeGuid")) EdgeGUID = Guid.Parse(_Http.Request.Url.Parameters.Get("edgeGuid"));
                if (_Http.Request.Url.Parameters.AllKeys.Contains("tagGuid")) TagGUID = Guid.Parse(_Http.Request.Url.Parameters.Get("tagGuid"));
            }

            if (_Http.Request.QuerystringExists(Constants.SkipQuerystring))
                if (Int32.TryParse(_Http.Request.RetrieveQueryValue(Constants.SkipQuerystring), out int skip)) Skip = skip;

            if (_Http.Request.QuerystringExists(Constants.ForceQuerystring)) Force = true;
            if (_Http.Request.QuerystringExists(Constants.IncludeDataQuerystring)) IncludeData = true;
            if (_Http.Request.QuerystringExists(Constants.FromGuidQuerystring)) FromGUID = Guid.Parse(_Http.Request.RetrieveQueryValue(Constants.FromGuidQuerystring));
            if (_Http.Request.QuerystringExists(Constants.ToGuidQuerystring)) ToGUID = Guid.Parse(_Http.Request.RetrieveQueryValue(Constants.ToGuidQuerystring));
        }

        #endregion
    }
}
