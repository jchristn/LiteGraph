namespace LiteGraph.Server.Classes
{
    using System;

    /// <summary>
    /// Debug settings.
    /// </summary>
    public class DebugSettings
    {
        /// <summary>
        /// Debug authentication.
        /// </summary>
        public bool Authentication { get; set; } = false;

        /// <summary>
        /// Debug exceptions.
        /// </summary>
        public bool Exceptions { get; set; } = true;

        /// <summary>
        /// Debug requests.
        /// </summary>
        public bool Requests { get; set; } = false;

        /// <summary>
        /// Instantiate.
        /// </summary>
        public DebugSettings()
        {

        }
    }
}