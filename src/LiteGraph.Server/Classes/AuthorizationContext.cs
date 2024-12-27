namespace LiteGraph.Server.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using WatsonWebserver.Core;

    /// <summary>
    /// Authorization context.
    /// </summary>
    public class AuthorizationContext
    {
        #region Public-Members

        /// <summary>
        /// Authorization result.
        /// </summary>
        public AuthorizationResultEnum Result { get; set; } = AuthorizationResultEnum.Denied;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public AuthorizationContext()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
