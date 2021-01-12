using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace LiteGraph
{
    /// <summary>
    /// A search filter.
    /// </summary>
    public class SearchFilter
    {
        #region Public-Members

        /// <summary>
        /// The field upon which to match.
        /// </summary>
        [JsonProperty(Order = -1)]
        public string Field
        {
            get
            {
                return _Field;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Field));
                _Field = value;
            }
        }

        /// <summary>
        /// The condition by which the graph is evaluated against the supplied value.
        /// </summary>
        public SearchCondition Condition = SearchCondition.Equals;

        /// <summary>
        /// The value to be evaluated using the specified condition.
        /// When using GreaterThan, GreaterThanOrEqualTo, LessThan, or LessThanOrEqualTo, the value supplied must be convertible to decimal.
        /// When using In, the value supplied 
        /// </summary>
        [JsonProperty(Order = 990)]
        public object Value
        {
            get
            {
                return _Value;
            }
            set
            {
                if (Condition == SearchCondition.GreaterThan
                    || Condition == SearchCondition.GreaterThanOrEqualTo
                    || Condition == SearchCondition.LessThan
                    || Condition == SearchCondition.LessThanOrEqualTo)
                {
                    if (value == null) throw new ArgumentNullException(nameof(Value));

                    decimal testDecimal = 0m;
                    if (!Decimal.TryParse(value.ToString(), out testDecimal))
                    {
                        throw new ArgumentException("Value must be convertible to decimal when using GreaterThan, GreaterThanOrEqualTo, LessThan, or LessThanOrEqualTo.");
                    }

                    _Value = value;
                }
                else if (Condition == SearchCondition.In)
                {
                    if (value == null) throw new ArgumentNullException(nameof(Value));
                    if (!IsStringList(Value))
                    {
                        throw new ArgumentException("Value must be convertible to List<string> when using In.");
                    }
                }

                _Value = value;
            }
        }

        #endregion

        #region Private-Members

        private string _Field = "";
        private object _Value = "";

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public SearchFilter()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="field">Field.</param>
        /// <param name="condition">SearchCondition.</param>
        /// <param name="value">Value.</param>
        public SearchFilter(string field, SearchCondition condition, object value)
        {
            if (String.IsNullOrEmpty(field)) throw new ArgumentNullException(nameof(field));

            Field = field;
            Condition = condition;
            Value = value;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Internal-Methods

        internal static bool IsStringList(object value)
        {
            return value is IList<string>
                && value.GetType().IsGenericType
                && value.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<string>));
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}