using System;
using System.Threading;
using BehaviorNets.Model.Ast;

namespace BehaviorNets.Model.Evaluation
{
    /// <summary>
    /// Provides a mechanism for evaluating constraint expressions in a behavior net.
    /// </summary>
    public class ExpressionEvaluator : IExpressionVisitor<Token, object>
    {
        public static ExpressionEvaluator Instance
        {
            get;
        } = new();

        /// <inheritdoc />
        public object VisitLiteralExpression(LiteralExpression expression, Token state) => expression.Value;

        /// <inheritdoc />
        public object VisitVariableExpression(VariableExpression expression, Token state)
        {
            if (!state.Variables.TryGetValue(expression.Name, out object? value) || value is null)
                throw new EvaluationException($"Variable {expression.Name} does not have a value.");

            return value;
        }

        /// <inheritdoc />
        public object VisitBinaryExpression(BinaryExpression expression, Token state)
        {
            // lambdas to allow for lazy evaluation with boolean short circuit operators.
            object Left() => expression.Left.AcceptVisitor(this, state);
            object Right() => expression.Right.AcceptVisitor(this, state);

            return expression.Operator switch
            {
                BinaryOperator.Equals => Equals(Left(), Right()),
                BinaryOperator.NotEquals => !Equals(Left(), Right()),
                BinaryOperator.GreaterThan => Convert.ToUInt64(Left()) > Convert.ToUInt64(Right()),
                BinaryOperator.GreaterThanOrEquals => Convert.ToUInt64(Left()) >= Convert.ToUInt64(Right()),
                BinaryOperator.LessThan => Convert.ToUInt64(Left()) < Convert.ToUInt64(Right()),
                BinaryOperator.LessThanOrEquals => Convert.ToUInt64(Left()) <= Convert.ToUInt64(Right()),
                BinaryOperator.BooleanAnd => Convert.ToBoolean(Left()) && Convert.ToBoolean(Right()),
                BinaryOperator.BooleanOr => Convert.ToBoolean(Left()) || Convert.ToBoolean(Right()),
                BinaryOperator.Add => Convert.ToUInt64(Left()) + Convert.ToUInt64(Right()),
                BinaryOperator.Subtract => Convert.ToUInt64(Left()) - Convert.ToUInt64(Right()),
                BinaryOperator.Multiply => Convert.ToUInt64(Left()) * Convert.ToUInt64(Right()),
                BinaryOperator.Divide => Convert.ToUInt64(Left()) / Convert.ToUInt64(Right()),
                BinaryOperator.Modulo => Convert.ToUInt64(Left()) % Convert.ToUInt64(Right()),
                BinaryOperator.BitwiseOr => Convert.ToUInt64(Left()) | Convert.ToUInt64(Right()),
                BinaryOperator.BitwiseAnd => Convert.ToUInt64(Left()) & Convert.ToUInt64(Right()),
                BinaryOperator.BitwiseXor => Convert.ToUInt64(Left()) ^ Convert.ToUInt64(Right()),
                BinaryOperator.In => ProcessInExpression(Left(), Right()),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static object ProcessInExpression(object x, object y)
        {
            if (x is string stringNeedle)
            {
                if (y is string haystack)
                    return haystack.Contains(stringNeedle);
                return false;
            }

            ulong integerNeedle = Convert.ToUInt64(x);
            (ulong start, ulong end) = (ValueTuple<ulong, ulong>) y;
            return integerNeedle >= start && integerNeedle < end;
        }

        /// <inheritdoc />
        public object VisitUnaryExpression(UnaryExpression expression, Token state)
        {
            object value = expression.Expression.AcceptVisitor(this, state);

            switch (expression.Operator)
            {
                case UnaryOperator.Negate:
                    return unchecked((ulong) -(long) Convert.ToUInt64(value));
                case UnaryOperator.BitwiseNot:
                    if (value is bool b)
                        return !b;
                    else
                        return ~Convert.ToUInt64(value);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public object VisitRangeExpression(RangeExpression expression, Token state)
        {
            ulong start = Convert.ToUInt64(expression.Start.AcceptVisitor(this, state));
            ulong end = Convert.ToUInt64(expression.End.AcceptVisitor(this, state));

            return (start, end);
        }
    }
}
