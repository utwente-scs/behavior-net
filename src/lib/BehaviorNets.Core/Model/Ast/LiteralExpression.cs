namespace BehaviorNets.Model.Ast;

/// <summary>
/// Represents a literal expression node in an abstract syntax tree.
/// </summary>
public class LiteralExpression : Expression
{
    /// <summary>
    /// Creates a new literal expression.
    /// </summary>
    /// <param name="value">The literal.</param>
    public LiteralExpression(object value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the literal value.
    /// </summary>
    public object Value
    {
        get;
        set;
    }

    /// <inheritdoc />
    public override void AcceptVisitor<TState>(IExpressionVisitor<TState> visitor, TState state) =>
        visitor.VisitLiteralExpression(this, state);

    /// <inheritdoc />
    public override TResult AcceptVisitor<TState, TResult>(IExpressionVisitor<TState, TResult> visitor, TState state) =>
        visitor.VisitLiteralExpression(this, state);
}