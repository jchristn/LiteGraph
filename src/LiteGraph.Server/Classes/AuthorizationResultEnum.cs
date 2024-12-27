namespace LiteGraph.Server.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Authorization result.
    /// </summary>
    public enum AuthorizationResultEnum
    {
        /// <summary>
        /// Permitted.
        /// </summary>
        [EnumMember(Value = "Permitted")]
        Permitted,
        /// <summary>
        /// Denied.
        /// </summary>
        [EnumMember(Value = "Denied")]
        Denied,
        /// <summary>
        /// Not found.
        /// </summary>
        [EnumMember(Value = "NotFound")]
        NotFound,
        /// <summary>
        /// Conflict.
        /// </summary>
        [EnumMember(Value = "Conflict")]
        Conflict
    }
}
