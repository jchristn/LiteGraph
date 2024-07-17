namespace LiteGraph
{
    using System;

    /// <summary>
    /// Syslog server settings.
    /// </summary>
    public class SyslogServer
    {
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

        /// <summary>
        /// Boolean to indicate whether or not randomized port numbers should be used.
        /// If false, the value in 'Port' will be used.
        /// </summary>
        public bool RandomizePorts { get; set; } = true;

        /// <summary>
        /// Minimum port.
        /// </summary>
        public int MinimumPort
        {
            get
            {
                return _MinimumPort;
            }
            set
            {
                if (value < 0 || value > 65535) throw new ArgumentOutOfRangeException(nameof(MinimumPort));
                _MinimumPort = value;
            }
        }

        /// <summary>
        /// Maximum port.
        /// </summary>
        public int MaximumPort
        {
            get
            {
                return _MaximumPort;
            }
            set
            {
                if (value < 0 || value > 65535) throw new ArgumentOutOfRangeException(nameof(MaximumPort));
                _MaximumPort = value;
            }
        }

        private string _Hostname = "127.0.0.1";
        private int _Port = 514;
        private int _MinimumPort = 65000;
        private int _MaximumPort = 65535;

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

            RandomizePorts = false;
        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="hostname">Hostname.</param>
        /// <param name="minPort">Minimum port number.</param>
        /// <param name="maxPort">Maximum port number.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SyslogServer(string hostname, int minPort, int maxPort)
        {
            if (string.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));

            MinimumPort = minPort;
            MaximumPort = maxPort;

            RandomizePorts = true;
        }
    }
}