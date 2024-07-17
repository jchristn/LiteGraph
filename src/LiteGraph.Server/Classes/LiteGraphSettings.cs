namespace LiteGraph.Server.Classes
{
    using System;

    /// <summary>
    /// LiteGraph settings.
    /// </summary>
    public class LiteGraphSettings
    {
        #region Public-Members

        /// <summary>
        /// Sqlite database filename.
        /// </summary>
        public string Filename
        {
            get
            {
                return _Filename;
            }
        }

        /// <summary>
        /// Maximum number of concurrent operations.
        /// For higher concurrency, use a lower number (e.g. 1).
        /// For lower concurrency, use a higher number (e.g. 10).
        /// This value dictates the maximum number of operations that may be operating in parallel at any one given time.
        /// </summary>
        public int MaxConcurrentOperations
        {
            get
            {
                return _MaxConcurrentOperations;
            }
        }

        #endregion

        #region Private-Members

        private string _Filename = "litegraph.db";
        private int _MaxConcurrentOperations = 4;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public LiteGraphSettings()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
