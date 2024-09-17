namespace LiteGraph
{
    using System;

    /// <summary>
    /// Syslog server settings.
    /// </summary>
    public class SyslogServer
    {
        #region Public-Members

        /// <summary>
        /// Hostname.
        /// </summary>
        public string Hostname
        {
            get
            {
                return _Hostname;
            }
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Hostname));
                _Hostname = value;
            }
        }

        /// <summary>
        /// Port.
        /// </summary>
        public int Port
        {
            get
            {
                return _Port;
            }
            set
            {
                if (value < 0 || value > 65535) throw new ArgumentOutOfRangeException(nameof(Port));
                _Port = value;
            }
        }

        #endregion

        #region Private-Members

        private string _Hostname = "127.0.0.1";
        private int _Port = 514;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public SyslogServer()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="hostname">Hostname.</param>
        /// <param name="port">Port.</param>
        public SyslogServer(string hostname, int port)
        {
            if (string.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));

            _Hostname = hostname;
            _Port = port;
        }

        #endregion

        #region Public-Members

        #endregion

        #region Private-Members

        #endregion
    }
}