namespace LiteGraph.Sdk
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Expression.
    /// </summary>
    [Serializable]
    public class Expr
    {
        #region Public-Members

        /// <summary>
        /// Left term.
        /// </summary>
        public object Left { get; set; } = null;

        /// <summary>
        /// Operator.
        /// </summary>
        public OperatorEnum Operator { get; set; } = OperatorEnum.Equals;

        /// <summary>
        /// Right term.
        /// </summary>
        public object Right { get; set; } = null;

        #endregion

        #region Private-Members

        private List<OperatorEnum> _RightRequired = new List<OperatorEnum>
        {
            OperatorEnum.And,
            OperatorEnum.Contains,
            OperatorEnum.ContainsNot,
            OperatorEnum.EndsWith,
            OperatorEnum.Equals,
            OperatorEnum.GreaterThan,
            OperatorEnum.GreaterThanOrEqualTo,
            OperatorEnum.In,
            OperatorEnum.LessThan,
            OperatorEnum.LessThanOrEqualTo,
            OperatorEnum.NotEquals,
            OperatorEnum.NotIn,
            OperatorEnum.Or,
            OperatorEnum.StartsWith
        };

        private List<Type> _LiteralTypes = new List<Type>
        {
            typeof(string),
            typeof(bool),
            typeof(bool?),
            typeof(int),
            typeof(int?),
            typeof(DateTime),
            typeof(DateTime?),
            typeof(DateTimeOffset),
            typeof(DateTimeOffset?),
            typeof(decimal),
            typeof(decimal?),
            typeof(double),
            typeof(double?),
            typeof(long),
            typeof(long?)
        };

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public Expr()
        {

        }

        /// <summary>
        /// A structure in the form of term-operator-term that defines a Boolean evaluation.
        /// A term can be a literal value, an embedded Expr object, or a list.
        /// List and Array objects can only be supplied on the right side of an expression.
        /// </summary>
        /// <param name="left">The left term of the expression.</param>
        /// <param name="oper">The operator.</param>
        /// <param name="right">The right term of the expression.</param>
        public Expr(object left, OperatorEnum oper, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (_RightRequired.Contains(oper) && right == null)
                throw new ArgumentException("The specified operator '" + oper.ToString() + "' requires a term on the 'Right' property.");

            Left = left;
            Operator = oper;
            Right = right;
        }

        /// <summary>
        /// An expression that allows you to determine if an object is between two values, i.e. GreaterThanOrEqualTo the first value, and LessThanOrEqualTo the second value.
        /// </summary>
        /// <param name="left">The left term of the expression; can either be a string term or a nested expression.</param> 
        /// <param name="right">List of two values where the first value is the lower value and the second value is the higher value.</param>
        public static Expr Between(object left, List<object> right)
        {
            if (right == null) throw new ArgumentNullException(nameof(right));
            if (right.Count != 2) throw new ArgumentException("Right term must contain exactly two members.");
            Expr startOfBetween = new Expr(left, OperatorEnum.GreaterThanOrEqualTo, right.First());
            Expr endOfBetween = new Expr(left, OperatorEnum.LessThanOrEqualTo, right.Last());
            return PrependAndClause(startOfBetween, endOfBetween);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// String representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string ret = "(";

            Type leftType = Left.GetType();
            Type rightType = null;

            if (Right != null)
            {
                rightType = Right.GetType();
            }

            if (_LiteralTypes.Contains(leftType))
            {
                // Console.WriteLine("Appending left literal: " + Left.ToString());
                ret += Left.ToString() + " ";
            }
            else if (IsArray(Left))
            {
                // Console.WriteLine("Appending left array");
                ret += "(array) ";
            }
            else if (IsList(Left))
            {
                // Console.WriteLine("Appending left list");
                ret += "(list) ";
            }
            else
            {
                // Console.WriteLine("Appending left expression: " + Left.ToString());
                ret += ((Expr)Left).ToString() + " ";
            }

            ret += Operator.ToString();

            if (Right != null)
            {
                if (_LiteralTypes.Contains(rightType))
                {
                    // Console.WriteLine("Appending right literal: " + Right.ToString());
                    ret += " " + Right.ToString();
                }
                else if (IsArray(Right))
                {
                    // Console.WriteLine("Appending right array");
                    ret += " (array)";
                }
                else if (IsList(Right))
                {
                    // Console.WriteLine("Appending right list");
                    ret += " (list)";
                }
                else
                {
                    // Console.WriteLine("Appending right expression: " + Right.ToString());
                    ret += " " + ((Expr)Right).ToString();
                }
            }
            else
            {
                ret += " (null)";
            }

            ret += ")";
            return ret;
        }

        /// <summary>
        /// Prepends a new expression using the supplied left term, operator, and right term using an AND clause.
        /// </summary>
        /// <param name="left">The left term of the expression; can either be a string term or a nested expression.</param>
        /// <param name="oper">The operator.</param>
        /// <param name="right">The right term of the expression; can either be an object for comparison or a nested expression.</param>
        public Expr PrependAnd(object left, OperatorEnum oper, object right)
        {
            if (_RightRequired.Contains(oper) && right == null)
                throw new ArgumentException("The specified operator '" + oper.ToString() + "' requires a term on the 'Right' property.");

            Expr e = new Expr(left, oper, right);
            return PrependAnd(e);
        }

        /// <summary>
        /// Prepends the expression with the supplied expression using an AND clause.
        /// </summary>
        /// <param name="prepend">The expression to prepend.</param> 
        public Expr PrependAnd(Expr prepend)
        {
            if (prepend == null) throw new ArgumentNullException(nameof(prepend));

            Expr orig = new Expr(this.Left, this.Operator, this.Right);
            Expr e = PrependAndClause(prepend, orig);
            Left = e.Left;
            Operator = e.Operator;
            Right = e.Right;

            return this;
        }

        /// <summary>
        /// Prepends a new expression using the supplied left term, operator, and right term using an OR clause.
        /// </summary>
        /// <param name="left">The left term of the expression; can either be a string term or a nested expression.</param>
        /// <param name="oper">The operator.</param>
        /// <param name="right">The right term of the expression; can either be an object for comparison or a nested expression.</param>
        public Expr PrependOr(object left, OperatorEnum oper, object right)
        {
            if (_RightRequired.Contains(oper) && right == null)
                throw new ArgumentException("The specified operator '" + oper.ToString() + "' requires a term on the 'Right' property.");

            Expr e = new Expr(left, oper, right);
            return PrependOr(e);
        }

        /// <summary>
        /// Prepends the expression with the supplied expression using an OR clause.
        /// </summary>
        /// <param name="prepend">The expression to prepend.</param> 
        public Expr PrependOr(Expr prepend)
        {
            if (prepend == null) throw new ArgumentNullException(nameof(prepend));

            Expr orig = new Expr(this.Left, this.Operator, this.Right);
            Expr e = PrependOrClause(prepend, orig);
            Left = e.Left;
            Operator = e.Operator;
            Right = e.Right;

            return this;
        }

        /// <summary>
        /// Create a copy of an expression.
        /// </summary>
        /// <returns>New instance.</returns>
        public Expr Copy()
        {
            return new Expr(Left, Operator, Right);
        }

        /// <summary>
        /// Prepends the expression in prepend to the expression original using an AND clause.
        /// </summary>
        /// <param name="prepend">The expression to prepend.</param>
        /// <param name="original">The original expression.</param>
        /// <returns>A new expression.</returns>
        public static Expr PrependAndClause(Expr prepend, Expr original)
        {
            if (prepend == null) throw new ArgumentNullException(nameof(prepend));
            if (original == null) throw new ArgumentNullException(nameof(original));
            Expr ret = new Expr
            {
                Left = prepend,
                Operator = OperatorEnum.And,
                Right = original
            };
            return ret;
        }

        /// <summary>
        /// Prepends the expression in prepend to the expression original using an OR clause.
        /// </summary>
        /// <param name="prepend">The expression to prepend.</param>
        /// <param name="original">The original expression.</param>
        /// <returns>A new expression.</returns>
        public static Expr PrependOrClause(Expr prepend, Expr original)
        {
            if (prepend == null) throw new ArgumentNullException(nameof(prepend));
            if (original == null) throw new ArgumentNullException(nameof(original));
            Expr ret = new Expr
            {
                Left = prepend,
                Operator = OperatorEnum.Or,
                Right = original
            };
            return ret;
        }

        /// <summary>
        /// Convert a list of expression objects to a nested expression containing AND between each expression in the list. 
        /// </summary>
        /// <param name="exprList">List of expression objects.</param>
        /// <returns>A nested expression.</returns>
        public static Expr ListToNestedAndExpression(List<Expr> exprList)
        {
            if (exprList == null) throw new ArgumentNullException(nameof(exprList));
            if (exprList.Count < 1) return null;

            int evaluated = 0;
            Expr ret = null;
            Expr left = null;
            List<Expr> remainder = new List<Expr>();

            if (exprList.Count == 1)
            {
                foreach (Expr curr in exprList)
                {
                    ret = curr;
                    break;
                }

                return ret;
            }
            else
            {
                foreach (Expr curr in exprList)
                {
                    if (evaluated == 0)
                    {
                        left = new Expr();
                        left.Left = curr.Left;
                        left.Operator = curr.Operator;
                        left.Right = curr.Right;
                        evaluated++;
                    }
                    else
                    {
                        remainder.Add(curr);
                        evaluated++;
                    }
                }

                ret = new Expr();
                ret.Left = left;
                ret.Operator = OperatorEnum.And;
                Expr right = ListToNestedAndExpression(remainder);
                ret.Right = right;

                return ret;
            }
        }

        /// <summary>
        /// Convert a list of expression objects to a nested expression containing OR between each expression in the list. 
        /// </summary>
        /// <param name="exprList">List of expression objects.</param>
        /// <returns>A nested expression.</returns>
        public static Expr ListToNestedOrExpression(List<Expr> exprList)
        {
            if (exprList == null) throw new ArgumentNullException(nameof(exprList));
            if (exprList.Count < 1) return null;

            int evaluated = 0;
            Expr ret = null;
            Expr left = null;
            List<Expr> remainder = new List<Expr>();

            if (exprList.Count == 1)
            {
                foreach (Expr curr in exprList)
                {
                    ret = curr;
                    break;
                }

                return ret;
            }
            else
            {
                foreach (Expr curr in exprList)
                {
                    if (evaluated == 0)
                    {
                        left = new Expr();
                        left.Left = curr.Left;
                        left.Operator = curr.Operator;
                        left.Right = curr.Right;
                        evaluated++;
                    }
                    else
                    {
                        remainder.Add(curr);
                        evaluated++;
                    }
                }

                ret = new Expr();
                ret.Left = left;
                ret.Operator = OperatorEnum.Or;
                Expr right = ListToNestedOrExpression(remainder);
                ret.Right = right;

                return ret;
            }
        }

        #endregion

        #region Internal-Methods

        #endregion

        #region Private-Methods

        /// <summary>
        /// Determines if an object is of an array type (excluding String objects).
        /// </summary>
        /// <param name="o">Object.</param>
        /// <returns>True if the object is of an Array type.</returns>
        private bool IsArray(object o)
        {
            if (o == null) return false;
            if (o is string) return false;
            return o.GetType().IsArray;
        }

        /// <summary>
        /// Determines if an object is of a List type.
        /// </summary>
        /// <param name="o">Object.</param>
        /// <returns>True if the object is of a List type.</returns>
        private bool IsList(object o)
        {
            if (o == null) return false;
            return o is IList &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }
        #endregion
    }
}
