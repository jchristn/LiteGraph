namespace LiteGraph.Server.Classes
{
    using System;
    using System.Collections.Specialized;

    /// <summary>
    /// Response context.
    /// </summary>
    public class ResponseContext
    {
        #region Public-Members

        /// <summary>
        /// Request GUID.
        /// </summary>
        public Guid RequestGuid { get; set; } = default(Guid);

        /// <summary>
        /// API version.
        /// </summary>
        public ApiVersionEnum ApiVersion { get; set; } = ApiVersionEnum.V1_0;

        /// <summary>
        /// Boolean indicating success.
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Content type.
        /// </summary>
        public string ContentType { get; set; } = null;

        /// <summary>
        /// Response headers.
        /// </summary>
        public NameValueCollection Headers
        {
            get
            {
                return _Headers;
            }
            set
            {
                if (value == null) value = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
                _Headers = value;
            }
        }

        /// <summary>
        /// Status code.
        /// </summary>
        public int StatusCode { get; set; } = 200;

        /// <summary>
        /// Error.
        /// </summary>
        public ApiErrorResponse Error { get; set; } = null;

        /// <summary>
        /// Data.
        /// </summary>
        public object Data { get; set; } = null;

        /// <summary>
        /// Bytes.
        /// </summary>
        public byte[] Bytes { get; set; } = null;

        #endregion

        #region Private-Members

        private NameValueCollection _Headers = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public ResponseContext()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="ctx">Request context.</param>
        /// <param name="data">Data.</param>
        public ResponseContext(RequestContext ctx, object data = null)
        {
            RequestGuid = ctx.RequestGuid;
            ApiVersion = ctx.ApiVersion;
            Success = true;
            Data = data;
        }

        /// <summary>
        /// Instantiate from error.
        /// </summary>
        /// <param name="requestGuid">Request GUID.</param>
        /// <param name="error">Error.</param>
        /// <param name="version">API version.</param>
        /// <param name="data">Data.</param>
        /// <param name="description">Description.</param>
        /// 
        /// <returns>Instance.</returns>
        public static ResponseContext FromError(
            Guid requestGuid,
            ApiErrorEnum error,
            ApiVersionEnum version,
            object data = null,
            string description = null)
        {
            ResponseContext ret = new ResponseContext
            {
                RequestGuid = requestGuid,
                ApiVersion = version,
                Success = false,
                ContentType = Constants.JsonContentType,
                Error = new ApiErrorResponse(error, null, description),
                Data = data
            };

            return ret;
        }

        /// <summary>
        /// Instantiate from error.
        /// </summary>
        /// <param name="req">Request context.</param>
        /// <param name="error">Error.</param>
        /// <param name="data">Data.</param>
        /// <param name="description">Description.</param>
        /// <returns>Instance.</returns>
        public static ResponseContext FromError(
            RequestContext req,
            ApiErrorEnum error,
            object data = null,
            string description = null)
        {
            ResponseContext ret = new ResponseContext
            {
                RequestGuid = req.RequestGuid,
                ApiVersion = req.ApiVersion,
                Success = false,
                ContentType = Constants.JsonContentType,
                Error = new ApiErrorResponse(error, null, description),
                Data = data
            };

            return ret;
        }

        /// <summary>
        /// Create with error data.
        /// </summary>
        /// <param name="req">Request context.</param>
        /// <param name="error">Error.</param>
        /// <param name="statusCode">Status code.</param>
        /// <param name="data">Data.</param>
        /// <param name="description">Description.</param>
        /// <returns>Response context.</returns>
        public static ResponseContext FromError(
            RequestContext req,
            ApiErrorEnum error,
            int statusCode,
            object data = null,
            string description = null)
        {
            ResponseContext ret = new ResponseContext
            {
                RequestGuid = req.RequestGuid,
                ApiVersion = req.ApiVersion,
                StatusCode = statusCode,
                Success = false,
                ContentType = Constants.JsonContentType,
                Error = new ApiErrorResponse(error, null, description),
                Data = data
            };

            return ret;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
