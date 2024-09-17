namespace LiteGraph.Server
{
    using System;
    using System.Text.Json.Serialization;
    using WatsonWebserver.Core;

    /// <summary>
    /// Active request.
    /// </summary>
    public class ActiveRequest
    {
        #region Public-Members

        /// <summary>
        /// Request GUID.
        /// </summary>
        [JsonIgnore]
        public Guid GUID { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Source IP address.
        /// </summary>
        public string SourceIp { get; set; } = null;

        /// <summary>
        /// Source port.
        /// </summary>
        public int SourcePort
        {
            get
            {
                return _SourcePort;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(SourcePort));
                _SourcePort = value;
            }
        }

        /// <summary>
        /// HTTP method.
        /// </summary>
        public HttpMethod Method { get; set; } = HttpMethod.GET;

        /// <summary>
        /// URL.
        /// </summary>
        public string Url { get; set; } = null;

        /// <summary>
        /// Number of request bytes.
        /// </summary>
        public long RequestBytes
        {
            get
            {
                return _RequestBytes;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(RequestBytes));
                _RequestBytes = value;
            }
        }

        /// <summary>
        /// Start time, in UTC time.
        /// </summary>
        public DateTime StartUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Total number of milliseconds the operation has been running.
        /// </summary>
        public double TotalMs
        {
            get
            {
                TimeSpan ts = DateTime.UtcNow - StartUtc;
                return Math.Round(ts.TotalMilliseconds, 2);
            }
        }

        #endregion

        #region Private-Members

        private int _SourcePort = 0;
        private long _RequestBytes = 0;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public ActiveRequest()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
