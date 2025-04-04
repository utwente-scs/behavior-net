namespace BehaviorNets.Model.Ast;

/// <summary>
/// Represents an expression that applies a binary operator on two values.
/// </summary>
public class BinaryExpression : Expression
{
    /// <summary>
    /// Creates a new binary operator expression.
    /// </summary>
    /// <param name="left">The left hand side of the expression.</param>
    /// <param name="operator">The operation to apply.</param>
    /// <param name="right">The right hand side of the expression.</param>
    public BinaryExpression(Expression left, BinaryOperator @operator, Expression right)
    {
        Left = left;
        Operator = @operator;
        Right = right;
    }

    /// <summary>
    /// Gets or sets the left hand side of the expression.
    /// </summary>
    public Expression Left
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the operation to apply.
    /// </summary>
    public BinaryOperator Operator
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the right hand side of the expression.
    /// </summary>
    public Expression Right
    {
        get;
        set;
    }

    /// <inheritdoc />
    public override void AcceptVisitor<TState>(IExpressionVisitor<TState> visitor, TState state) =>
        visitor.VisitBinaryExpression(this, state);

    /// <inheritdoc />
    public override TResult AcceptVisitor<TState, TResult>(IExpressionVisitor<TState, TResult> visitor, TState state) =>
        visitor.VisitBinaryExpression(this, state);
}