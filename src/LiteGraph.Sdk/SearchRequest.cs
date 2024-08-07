namespace LiteGraph.Sdk
{
    using ExpressionTree;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Search request.
    /// </summary>
    public class SearchRequest
    {
        #region Public-Members

        /// <summary>
        /// Graph GUID.
        /// </summary>
        public Guid GraphGUID { get; set; } = default(Guid);

        /// <summary>
        /// Ordering.
        /// </summary>
        public EnumerationOrderEnum Ordering { get; set; } = EnumerationOrderEnum.CreatedDescending;

        /// <summary>
        /// Expression.
        /// </summary>
        public Expr Expr { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public SearchRequest()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
