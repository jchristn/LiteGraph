using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace LiteGraph
{
    /// <summary>
    /// Available conditions for search filters.
    /// </summary> 
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SearchCondition
    {
        /// <summary>
        /// The left and right terms are equal to one another.
        /// </summary>
        [EnumMember(Value = "Equals")]
        Equals,
        /// <summary>
        /// The left and right terms are not equal to one another.
        /// </summary>
        [EnumMember(Value = "NotEquals")]
        NotEquals,
        /// <summary>
        /// The left term is greater than the right term.
        /// </summary>
        [EnumMember(Value = "GreaterThan")]
        GreaterThan,
        /// <summary>
        /// The left term is greater than or equal to the right term.
        /// </summary>
        [EnumMember(Value = "GreaterThanOrEqualTo")]
        GreaterThanOrEqualTo,
        /// <summary>
        /// The left term is less than the right term.
        /// </summary>
        [EnumMember(Value = "LessThan")]
        LessThan,
        /// <summary>
        /// The left term is less than or equal to the right term.
        /// </summary>
        [EnumMember(Value = "LessThanOrEqualTo")]
        LessThanOrEqualTo,
        /// <summary>
        /// The left term is null.
        /// </summary>
        [EnumMember(Value = "IsNull")]
        IsNull,
        /// <summary>
        /// The left term is not null.
        /// </summary>
        [EnumMember(Value = "IsNotNull")]
        IsNotNull,
        /// <summary>
        /// The left term is contained within the right term (list).
        /// </summary>
        [EnumMember(Value = "Contains")]
        Contains,
        /// <summary>
        /// The left term is not contained within the right term (list).
        /// </summary>
        [EnumMember(Value = "ContainsNot")]
        ContainsNot,
        /// <summary>
        /// The left term starts with the right term.
        /// </summary>
        [EnumMember(Value = "StartsWith")]
        StartsWith,
        /// <summary>
        /// The left term ends with the right term.
        /// </summary>
        [EnumMember(Value = "EndsWith")]
        EndsWith
    }
}