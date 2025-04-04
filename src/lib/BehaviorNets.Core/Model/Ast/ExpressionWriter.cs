using System;
using System.IO;

namespace BehaviorNets.Model.Ast;

/// <summary>
/// Provides a mechanism for formatting an expression to a human-readable string.
/// </summary>
public class ExpressionWriter : IExpressionVisitor<TextWriter>
{
    /// <summary>
    /// Gets the singleton instance for the expression writer class.
    /// </summary>
    public static ExpressionWriter Instance
    {
        get;
    } = new();

    /// <inheritdoc />
    public void VisitLiteralExpression(LiteralExpression expression, TextWriter state)
    {
        if (expression.Value is string s)
        {
            state.Write('"');

            foreach (char c in s)
            {
                if (c is '"' or '\n' or '\r' or '\t' or '\\')
                    state.Write('\\');
                state.Write(c);
            }

            state.Write('"');
        }
        else
        {
            state.Write(expression.Value.ToString());
        }
    }

    /// <inheritdoc />
    public void VisitVariableExpression(VariableExpression expression, TextWriter state) =>
        state.Write(expression.Name);

    /// <inheritdoc />
    public void VisitBinaryExpression(BinaryExpression expression, TextWriter state)
    {
        state.Write('(');
        expression.Left.AcceptVisitor(this, state);

        state.Write(expression.Operator switch
        {
            BinaryOperator.Equals => " == ",
            BinaryOperator.NotEquals => " != ",
            BinaryOperator.LessThan => " < ",
            BinaryOperator.LessThanOrEquals => " <= ",
            BinaryOperator.GreaterThan => " > ",
            BinaryOperator.GreaterThanOrEquals => " >= ",
            BinaryOperator.BooleanOr => " or ",
            BinaryOperator.BooleanAnd => " and ",
            BinaryOperator.Add => " + ",
            BinaryOperator.Subtract => " - ",
            BinaryOperator.Multiply => " * ",
            BinaryOperator.Divide => " / ",
            BinaryOperator.BitwiseAnd => " & ",
            BinaryOperator.BitwiseOr => " | ",
            BinaryOperator.BitwiseXor => " ^ ",
            BinaryOperator.In => " in ",
            _ => throw new ArgumentOutOfRangeException(nameof(expression.Operator))
        });

        expression.Right.AcceptVisitor(this, state);
        state.Write(')');
    }

    /// <inheritdoc />
    public void VisitUnaryExpression(UnaryExpression expression, TextWriter state)
    {
        state.Write(expression.Operator switch
        {
            UnaryOperator.Negate => "-",
            UnaryOperator.BitwiseNot => "~",
            _ => throw new ArgumentOutOfRangeException(nameof(expression.Operator))
        });

        expression.Expression.AcceptVisitor(this, state);
    }

    /// <inheritdoc />
    public void VisitRangeExpression(RangeExpression expression, TextWriter state)
    {
        state.Write('[');
        expression.Start.AcceptVisitor(this, state);
        state.Write("..");
        expression.End.AcceptVisitor(this, state);
        state.Write(']');
    }
}