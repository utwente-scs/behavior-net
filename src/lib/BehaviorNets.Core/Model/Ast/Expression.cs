using System.IO;

namespace BehaviorNets.Model.Ast;

/// <summary>
/// Represents a node in the abstract syntax tree of a constraint expression in a behavior net.
/// </summary>
public abstract class Expression
{
    /// <summary>
    /// Produces a new literal expression node.
    /// </summary>
    /// <param name="value">The literal.</param>
    /// <returns>The expression.</returns>
    public static LiteralExpression Literal(object value) => new(value);

    /// <summary>
    /// Produces a new symbolic variable reference expression node.
    /// </summary>
    /// <param name="name">The name of the symbolic variable.</param>
    /// <returns>The variable expression.</returns>
    public static VariableExpression Variable(string name) => new(name);

    /// <summary>
    /// Wraps two expressions into a new binary operator expression.
    /// </summary>
    /// <param name="left">The left hand side of the binary expression.</param>
    /// <param name="op">The operator to use.</param>
    /// <param name="right">The right hand side of the binary expression.</param>
    /// <returns>The binary expression.</returns>
    public static BinaryExpression Binary(Expression left, BinaryOperator op, Expression right) => new(left, op, right);

    /// <summary>
    /// Wraps a variable name and an expression into a new binary operator expression.
    /// </summary>
    /// <param name="leftVariableName">The variable name used on the left hand side of the binary expression.</param>
    /// <param name="op">The operator to use.</param>
    /// <param name="right">The right hand side of the binary expression.</param>
    /// <returns>The binary expression.</returns>
    public static BinaryExpression Binary(string leftVariableName, BinaryOperator op, Expression right) => new(Variable(leftVariableName), op, right);

    /// <summary>
    /// Wraps two variable names into a new binary operator expression.
    /// </summary>
    /// <param name="leftVariableName">The variable name used on the left hand side of the binary expression.</param>
    /// <param name="op">The operator to use.</param>
    /// <param name="rightVariableName">The variable name used on the right hand side of the binary expression.</param>
    /// <returns>The binary expression.</returns>
    public static BinaryExpression Binary(string leftVariableName, BinaryOperator op, string rightVariableName) =>
        new(Variable(leftVariableName), op, Variable(rightVariableName));

    /// <summary>
    /// Wraps a variable name and a literal into a new binary operator expression.
    /// </summary>
    /// <param name="leftVariableName">The variable name used on the left hand side of the binary expression.</param>
    /// <param name="op">The operator to use.</param>
    /// <param name="literalValue">The literal value used on the right hand side of the binary expression.</param>
    /// <returns>The binary expression.</returns>
    public static BinaryExpression Binary(string leftVariableName, BinaryOperator op, object literalValue) =>
        new(Variable(leftVariableName), op, Literal(literalValue));

    /// <summary>
    /// Wraps an expression around a unary operator.
    /// </summary>
    /// <param name="op">The unary operator.</param>
    /// <param name="value">The embedded expression.</param>
    /// <returns>The unary expression.</returns>
    public static UnaryExpression Unary(UnaryOperator op, Expression value) => new(op, value);

    /// <summary>
    /// Wraps a variable name around a unary operator.
    /// </summary>
    /// <param name="op">The unary operator.</param>
    /// <param name="variableName">The variable name.</param>
    /// <returns>The unary expression.</returns>
    public static UnaryExpression Unary(UnaryOperator op, string variableName) => new(op, Variable(variableName));

    /// <summary>
    /// Constructs a range expression.
    /// </summary>
    /// <param name="start">The expression making up the start index of the range.</param>
    /// <param name="end">The expression making up the end index of the range.</param>
    /// <returns>The range expression</returns>
    public static RangeExpression Range(Expression start, Expression end) => new(start, end);

    /// <summary>
    /// Constructs a range expression, based on two symbolic variables.
    /// </summary>
    /// <param name="startVariable">The variable making up the start index of the range.</param>
    /// <param name="endVariable">The variable making up the end index of the range.</param>
    /// <returns>The range expression</returns>
    public static RangeExpression Range(string startVariable, string endVariable) =>
        new(Variable(startVariable), Variable(endVariable));

    /// <summary>
    /// Visits the expression using the provided expression visitor.
    /// </summary>
    /// <param name="visitor">The visitor.</param>
    /// <param name="state">A state object to pass along.</param>
    /// <typeparam name="TState">The type of state to pass along.</typeparam>
    public abstract void AcceptVisitor<TState>(IExpressionVisitor<TState> visitor, TState state);

    /// <summary>
    /// Visits the expression using the provided expression visitor.
    /// </summary>
    /// <param name="visitor">The visitor.</param>
    /// <param name="state">A state object to pass along.</param>
    /// <typeparam name="TState">The type of state to pass along.</typeparam>
    /// <typeparam name="TResult">The type of result to produce.</typeparam>
    public abstract TResult AcceptVisitor<TState, TResult>(IExpressionVisitor<TState, TResult> visitor, TState state);

    /// <inheritdoc />
    public override string ToString()
    {
        var writer = new StringWriter();
        AcceptVisitor(ExpressionWriter.Instance, writer);
        return writer.ToString();
    }
}