namespace LiteGraph.Sdk
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Available conditions for search filters.
    /// </summary>
    public enum OperatorEnum
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
        /// The left term does not start with the right term.
        /// </summary>
        [EnumMember(Value = "StartsWithNot")]
        StartsWithNot,
        /// <summary>
        /// The left term ends with the right term.
        /// </summary>
        [EnumMember(Value = "EndsWith")]
        EndsWith,
        /// <summary>
        /// The left term does not end with the right term.
        /// </summary>
        [EnumMember(Value = "EndsWithNot")]
        EndsWithNot,
        /// <summary>
        /// The left and right both resolve to true.
        /// </summary>
        [EnumMember(Value = "And")]
        And,
        /// <summary>
        /// Either the left, the right, or both resolve to true.
        /// </summary>
        [EnumMember(Value = "Or")]
        Or,
        /// <summary>
        /// Value is contained within a list
        /// </summary>
        [EnumMember(Value = "In")]
        In,
        /// <summary>
        /// Value is not contained within a list
        /// </summary>
        [EnumMember(Value = "NotIn")]
        NotIn,
    }
}
