namespace BehaviorNets.Model.Ast;

/// <summary>
/// Represents an expression that applies a unary operator on a value.
/// </summary>
public class UnaryExpression : Expression
{
    /// <summary>
    /// Creates a new unary operator expression.
    /// </summary>
    /// <param name="operator">The operation to apply/</param>
    /// <param name="expression">The expression to apply the operation on.</param>
    public UnaryExpression(UnaryOperator @operator, Expression expression)
    {
        Operator = @operator;
        Expression = expression;
    }

    /// <summary>
    /// Gets or sets the operation to apply.
    /// </summary>
    public UnaryOperator Operator
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the expression to apply the operation on.
    /// </summary>
    public Expression Expression
    {
        get;
        set;
    }

    /// <inheritdoc />
    public override void AcceptVisitor<TState>(IExpressionVisitor<TState> visitor, TState state) =>
        visitor.VisitUnaryExpression(this, state);

    /// <inheritdoc />
    public override TResult AcceptVisitor<TState, TResult>(IExpressionVisitor<TState, TResult> visitor, TState state) =>
        visitor.VisitUnaryExpression(this, state);
}