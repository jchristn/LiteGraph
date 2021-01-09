using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiteGraph
{
    /// <summary>
    /// LiteGraph events.
    /// </summary>
    public class LiteGraphEvents
    {
        #region Public-Members

        /// <summary>
        /// Event fired when a node is added.
        /// </summary>
        public event EventHandler<NodeEventArgs> NodeAdded;

        /// <summary>
        /// Event fired when a node is updated.
        /// </summary>
        public event EventHandler<NodeEventArgs> NodeUpdated;

        /// <summary>
        /// Event fired when a node is removed.
        /// </summary>
        public event EventHandler<NodeEventArgs> NodeRemoved;

        /// <summary>
        /// Event fired when an edge is added.
        /// </summary>
        public event EventHandler<EdgeEventArgs> EdgeAdded;

        /// <summary>
        /// Event fired when an edge is updated.
        /// </summary>
        public event EventHandler<EdgeEventArgs> EdgeUpdated;

        /// <summary>
        /// Event fired when an edge is removed.
        /// </summary>
        public event EventHandler<EdgeEventArgs> EdgeRemoved;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public LiteGraphEvents()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Internal-Methods

        internal void HandleNodeAdded(object sender, NodeEventArgs args)
        {
            WrappedEventHandler(() => NodeAdded?.Invoke(sender, args), "NodeAdded", sender);
        }

        internal void HandleNodeUpdated(object sender, NodeEventArgs args)
        {
            WrappedEventHandler(() => NodeUpdated?.Invoke(sender, args), "NodeUpdated", sender);
        }

        internal void HandleNodeRemoved(object sender, NodeEventArgs args)
        {
            WrappedEventHandler(() => NodeRemoved?.Invoke(sender, args), "NodeRemoved", sender);
        }

        internal void HandleEdgeAdded(object sender, EdgeEventArgs args)
        {
            WrappedEventHandler(() => EdgeAdded?.Invoke(sender, args), "EdgeAdded", sender);
        }

        internal void HandleEdgeUpdated(object sender, EdgeEventArgs args)
        {
            WrappedEventHandler(() => EdgeUpdated?.Invoke(sender, args), "EdgeUpdated", sender);
        }

        internal void HandleEdgeRemoved(object sender, EdgeEventArgs args)
        {
            WrappedEventHandler(() => EdgeRemoved?.Invoke(sender, args), "EdgeRemoved", sender);
        }

        #endregion

        #region Private-Methods

        private void WrappedEventHandler(Action action, string handler, object sender)
        {
            if (action == null) return;

            Action<string> logger = ((LiteGraphClient)sender).Logger.LogMethod;
            Formatting jsonFormatting = ((LiteGraphClient)sender).JsonFormatting;

            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                logger?.Invoke("Event handler exception in " + handler + ": " + Environment.NewLine + JsonConvert.SerializeObject(e, jsonFormatting));
            }
        }

        #endregion
    }
}
