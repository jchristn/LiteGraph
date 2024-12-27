namespace LiteGraph.Server.Classes
{
    using ExpressionTree;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Route request.
    /// </summary>
    public class RouteRequest
    {
        #region Public-Members

        /// <summary>
        /// Tenant GUID.
        /// </summary>
        public Guid TenantGUID { get; set; } = default(Guid);

        /// <summary>
        /// Graph GUID.
        /// </summary>
        public Guid GraphGUID { get; set; } = default(Guid);

        /// <summary>
        /// From node GUID.
        /// </summary>
        public Guid From { get; set; } = default(Guid);

        /// <summary>
        /// To node GUID.
        /// </summary>
        public Guid To { get; set; } = default(Guid);

        /// <summary>
        /// Edge filters.
        /// </summary>
        public Expr EdgeFilter { get; set; } = null;

        /// <summary>
        /// Node filters.
        /// </summary>
        public Expr NodeFilter { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Route request.
        /// </summary>
        public RouteRequest()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
