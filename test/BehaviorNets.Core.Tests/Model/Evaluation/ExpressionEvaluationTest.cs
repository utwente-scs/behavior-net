using BehaviorNets.Model.Ast;
using BehaviorNets.Model.Evaluation;
using Xunit;

namespace BehaviorNets.Tests.Model.Evaluation;

public class ExpressionEvaluationTest
{
    private static void ExpectResult(object result, Token state, Expression expression)
    {
        object actualResult = expression.AcceptVisitor(ExpressionEvaluator.Instance, state);
        Assert.Equal(result, actualResult);
    }

    [Fact]
    public void Literal()
    {
        ExpectResult(123, Token.Empty,
            Expression.Literal(123));
    }

    [Theory]
    [InlineData("x", 123)]
    [InlineData("y", 456)]
    public void ExistingVariable(string name, int value)
    {
        var state = Token.Empty
            .SetVariable("x", 123)
            .SetVariable("y", 456);
        ExpectResult(value, state, Expression.Variable(name));
    }

    [Fact]
    public void NonExistingVariable()
    {
        Assert.Throws<EvaluationException>(() =>
            Expression.Variable("nonexisting").AcceptVisitor(ExpressionEvaluator.Instance, Token.Empty));
    }

    [Theory]
    [InlineData(123, BinaryOperator.Equals, 123, true)]
    [InlineData(123, BinaryOperator.Equals, 456, false)]
    [InlineData(123, BinaryOperator.NotEquals, 123, false)]
    [InlineData(123, BinaryOperator.NotEquals, 456, true)]
    [InlineData(1, BinaryOperator.LessThan, 1, false)]
    [InlineData(1, BinaryOperator.LessThan, 2, true)]
    [InlineData(2, BinaryOperator.LessThan, 1, false)]
    [InlineData(1, BinaryOperator.LessThanOrEquals, 1, true)]
    [InlineData(1, BinaryOperator.LessThanOrEquals, 2, true)]
    [InlineData(2, BinaryOperator.LessThanOrEquals, 1, false)]
    [InlineData(1, BinaryOperator.GreaterThan, 1, false)]
    [InlineData(1, BinaryOperator.GreaterThan, 2, false)]
    [InlineData(2, BinaryOperator.GreaterThan, 1, true)]
    [InlineData(1, BinaryOperator.GreaterThanOrEquals, 1, true)]
    [InlineData(1, BinaryOperator.GreaterThanOrEquals, 2, false)]
    [InlineData(2, BinaryOperator.GreaterThanOrEquals, 1, true)]
    [InlineData(2, BinaryOperator.Add, 1, 3ul)]
    [InlineData(2, BinaryOperator.Subtract, 1, 1ul)]
    [InlineData(2, BinaryOperator.Multiply, 4, 8ul)]
    [InlineData(8, BinaryOperator.Divide, 4, 2ul)]
    [InlineData(0x12345678, BinaryOperator.BitwiseAnd, 0xFF00FF00, 0x12005600ul)]
    [InlineData(0x12340000, BinaryOperator.BitwiseOr, 0x5678, 0x12345678ul)]
    [InlineData(0x12345678, BinaryOperator.BitwiseOr, 0x5678, 0x12345678ul)]
    [InlineData(0x12340000, BinaryOperator.BitwiseXor, 0x5678, 0x12345678ul)]
    [InlineData(0x12345678, BinaryOperator.BitwiseXor, 0x12345678, 0ul)]
    public void BinaryOperators(ulong left, BinaryOperator op, ulong right, object result)
    {
        ExpectResult(result, Token.Empty,
            Expression.Binary(Expression.Literal(left), op, Expression.Literal(right)));
    }

    [Fact]
    public void AndShortCircuitTest()
    {
        ExpectResult(false, Token.Empty,
            Expression.Binary(Expression.Literal(false), BinaryOperator.BooleanAnd, Expression.Variable("nonexisting")));
    }

    [Fact]
    public void OrShortCircuitTest()
    {
        ExpectResult(true, Token.Empty,
            Expression.Binary(Expression.Literal(true), BinaryOperator.BooleanOr, Expression.Variable("nonexisting")));
    }

    [Theory]
    [InlineData(UnaryOperator.BitwiseNot, false, true)]
    [InlineData(UnaryOperator.BitwiseNot, true, false)]
    public void UnaryOperators(UnaryOperator op, object value, object result)
    {
        ExpectResult(result, Token.Empty,
            Expression.Unary(op, Expression.Literal(value)));
    }

    [Theory]
    [InlineData(4, 5, 10, false)]
    [InlineData(5, 5, 10, true)]
    [InlineData(5, 0, 10, true)]
    [InlineData(9, 0, 10, true)]
    [InlineData(10, 0, 10, false)]
    [InlineData(11, 0, 10, false)]
    public void InIntegerRange(ulong needle, ulong start, ulong end, bool expected)
    {
        ExpectResult(
            expected,
            Token.Empty
                .SetVariable("x", needle)
                .SetVariable("s", start)
                .SetVariable("e", end),
            Expression.Binary("x", BinaryOperator.In, Expression.Range("s", "e")));
    }

    [Theory]
    [InlineData("a", "bcd", false)]
    [InlineData("a", "abcd", true)]
    [InlineData("", "abcd", true)]
    [InlineData("abcd", "", false)]
    public void InStringRange(string needle, string haystack, bool expected)
    {
        ExpectResult(
            expected,
            Token.Empty
                .SetVariable("x", haystack),
            Expression.Binary(Expression.Literal(needle), BinaryOperator.In, Expression.Variable("x")));
    }
}