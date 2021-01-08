using System;
using System.Collections.Generic;
using System.Text;

namespace LiteGraph
{
    /// <summary>
    /// Logging settings.
    /// </summary>
    public class LoggerSettings
    {
        #region Public-Members

        /// <summary>
        /// Header to prepend to all log messages.
        /// </summary>
        public string Header { get; set; } = "[LiteGraph]";

        /// <summary>
        /// Method to invoke to send log messages.
        /// </summary>
        public Action<string> LogMethod { get; set; } = null;

        /// <summary>
        /// Enable or disable logging of queries.
        /// </summary>
        public bool LogQueries { get; set; } = false;

        /// <summary>
        /// Enable or disable logging of results.
        /// </summary>
        public bool LogResults { get; set; } = false;

        #endregion

        #region Private-Members
         
        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Logging settings.
        /// </summary>
        public LoggerSettings()
        {

        }

        #endregion

        #region Internal-Methods

        internal void Log(string msg)
        {
            LogMethod?.Invoke(Header + " " + msg);
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
