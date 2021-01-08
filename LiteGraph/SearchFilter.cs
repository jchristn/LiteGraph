using System;
using System.Collections.Generic;
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
        /// The condition by which the parsed document's content is evaluated against the supplied value.
        /// </summary>
        public SearchCondition Condition = SearchCondition.Equals;

        /// <summary>
        /// The value to be evaluated using the specified condition against the parsed document's content.
        /// When using GreaterThan, GreaterThanOrEqualTo, LessThan, or LessThanOrEqualTo, the value supplied must be convertible to decimal.
        /// </summary>
        [JsonProperty(Order = 990)]
        public string Value
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
                    if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));

                    decimal testDecimal = 0m;
                    if (!Decimal.TryParse(value, out testDecimal))
                    {
                        throw new ArgumentException("Value must be convertible to decimal when using GreaterThan, GreaterThanOrEqualTo, LessThan, or LessThanOrEqualTo.");
                    }
                }

                _Value = value;
            }
        }

        #endregion

        #region Private-Members

        private string _Field = "";
        private string _Value = "";

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
        public SearchFilter(string field, SearchCondition condition, string value)
        {
            if (String.IsNullOrEmpty(field)) throw new ArgumentNullException(nameof(field));

            Field = field;
            Condition = condition;
            Value = value;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Evaluates a value against the search filter.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <returns>True if matched.</returns>
        public bool EvaluateValue(object value)
        {
            if (String.IsNullOrEmpty(Field)) throw new InvalidOperationException("Search filter field cannot be null.");

            if (Condition == SearchCondition.Contains)
            {
                if (value == null && Value == null) return true;
                if (value == null && Value != null) return false;
                if (value != null && Value == null) return false;

                string str1 = value.ToString();
                string str2 = Value.ToString();
                return str1.Contains(str2);
            }
            else if (Condition == SearchCondition.ContainsNot)
            {
                if (value == null && Value == null) return false;
                if (value == null && Value != null) return true;
                if (value != null && Value == null) return true;

                string str1 = value.ToString();
                string str2 = Value.ToString();
                return !str1.Contains(str2);
            }
            else if (Condition == SearchCondition.EndsWith)
            {
                if (value == null && Value == null) return true;
                if (value == null && Value != null) return false;
                if (value != null && Value == null) return false;

                string str1 = value.ToString();
                string str2 = Value.ToString();
                return str1.EndsWith(str2);
            }
            else if (Condition == SearchCondition.Equals)
            {
                if (value == null && Value == null) return true;
                if (value == null && Value != null) return false;
                if (value != null && Value == null) return false;

                string str1 = value.ToString();
                string str2 = Value.ToString();
                return str1.Equals(str2);
            }
            else if (Condition == SearchCondition.GreaterThan)
            {
                if (value == null || Value == null) return false;

                if (value is DateTime)
                {
                    DateTime dt1 = Convert.ToDateTime(value);
                    DateTime dt2 = Convert.ToDateTime(Value);
                    return dt1 > dt2;
                }
                else if (value is decimal)
                {
                    decimal d1 = Convert.ToDecimal(value);
                    decimal d2 = Convert.ToDecimal(Value);
                    return d1 > d2;
                }
                else if (value is Int64)
                {
                    Int64 l1 = Convert.ToInt64(value);
                    Int64 l2 = Convert.ToInt64(Value);
                    return l1 > l2;
                }
                else if (value is Int32)
                {
                    Int32 i1 = Convert.ToInt32(value);
                    Int32 i2 = Convert.ToInt32(Value);
                    return i1 > i2;
                }

                return false;
            }
            else if (Condition == SearchCondition.GreaterThanOrEqualTo)
            {
                if (value == null && Value == null) return true;
                if (value == null || Value == null) return false;

                if (value is DateTime)
                {
                    DateTime dt1 = Convert.ToDateTime(value);
                    DateTime dt2 = Convert.ToDateTime(Value);
                    return dt1 >= dt2;
                }
                else if (value is decimal)
                {
                    decimal d1 = Convert.ToDecimal(value);
                    decimal d2 = Convert.ToDecimal(Value);
                    return d1 >= d2;
                }
                else if (value is Int64)
                {
                    Int64 l1 = Convert.ToInt64(value);
                    Int64 l2 = Convert.ToInt64(Value);
                    return l1 >= l2;
                }
                else if (value is Int32)
                {
                    Int32 i1 = Convert.ToInt32(value);
                    Int32 i2 = Convert.ToInt32(Value);
                    return i1 >= i2;
                }

                return false;
            }
            else if (Condition == SearchCondition.IsNotNull)
            {
                if (value == null) return false;
                return true;
            }
            else if (Condition == SearchCondition.IsNull)
            {
                if (value != null) return false;
                return true;
            }
            else if (Condition == SearchCondition.LessThan)
            {
                if (value == null || Value == null) return false;

                if (value is DateTime)
                {
                    DateTime dt1 = Convert.ToDateTime(value);
                    DateTime dt2 = Convert.ToDateTime(Value);
                    return dt1 < dt2;
                }
                else if (value is decimal)
                {
                    decimal d1 = Convert.ToDecimal(value);
                    decimal d2 = Convert.ToDecimal(Value);
                    return d1 < d2;
                }
                else if (value is Int64)
                {
                    Int64 l1 = Convert.ToInt64(value);
                    Int64 l2 = Convert.ToInt64(Value);
                    return l1 < l2;
                }
                else if (value is Int32)
                {
                    Int32 i1 = Convert.ToInt32(value);
                    Int32 i2 = Convert.ToInt32(Value);
                    return i1 < i2;
                }

                return false;
            }
            else if (Condition == SearchCondition.LessThanOrEqualTo)
            {
                if (value == null && Value == null) return true;
                if (value == null || Value == null) return false;

                if (value is DateTime)
                {
                    DateTime dt1 = Convert.ToDateTime(value);
                    DateTime dt2 = Convert.ToDateTime(Value);
                    return dt1 <= dt2;
                }
                else if (value is decimal)
                {
                    decimal d1 = Convert.ToDecimal(value);
                    decimal d2 = Convert.ToDecimal(Value);
                    return d1 <= d2;
                }
                else if (value is Int64)
                {
                    Int64 l1 = Convert.ToInt64(value);
                    Int64 l2 = Convert.ToInt64(Value);
                    return l1 <= l2;
                }
                else if (value is Int32)
                {
                    Int32 i1 = Convert.ToInt32(value);
                    Int32 i2 = Convert.ToInt32(Value);
                    return i1 <= i2;
                }

                return false;
            }
            else if (Condition == SearchCondition.NotEquals)
            {
                if (value == null && Value == null) return false;
                if (value == null && Value != null) return true;
                if (value != null && Value == null) return true;

                string str1 = value.ToString();
                string str2 = Value.ToString();
                return !str1.Equals(str2);
            }
            else if (Condition == SearchCondition.StartsWith)
            {
                if (value == null && Value == null) return true;
                if (value == null && Value != null) return false;
                if (value != null && Value == null) return false;

                string str1 = value.ToString();
                string str2 = Value.ToString();
                return str1.StartsWith(str2);
            }
            else
            {
                throw new ArgumentException("Unknown search filter condition: " + Condition.ToString() + ".");
            }
        }
         
        #endregion

        #region Private-Methods

        #endregion
    }
}