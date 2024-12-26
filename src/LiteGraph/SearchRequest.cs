﻿namespace LiteGraph
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
        /// Tenant GUID.
        /// </summary>
        public Guid TenantGUID { get; set; } = default(Guid);

        /// <summary>
        /// Graph GUID.
        /// </summary>
        public Guid GraphGUID { get; set; } = default(Guid);

        /// <summary>
        /// Ordering.
        /// </summary>
        public EnumerationOrderEnum Ordering { get; set; } = EnumerationOrderEnum.CreatedDescending;

        /// <summary>
        /// The number of records to skip.
        /// </summary>
        public int Skip
        {
            get
            {
                return _Skip;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Skip));
                _Skip = value;
            }
        }

        /// <summary>
        /// Search tags.
        /// </summary>
        public Dictionary<string, string> Tags
        {
            get
            {
                return _Tags;
            }
            set
            {
                if (value == null) value = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                _Tags = value;
            }
        }

        /// <summary>
        /// Expression.
        /// </summary>
        public Expr Expr { get; set; } = null;

        #endregion

        #region Private-Members

        private int _Skip = 0;
        private Dictionary<string, string> _Tags { get; set; } = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

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
