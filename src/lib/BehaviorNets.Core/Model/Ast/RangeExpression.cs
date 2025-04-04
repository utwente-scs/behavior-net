namespace BehaviorNets.Model.Ast;

/// <summary>
/// Represents a range expression.
/// </summary>
public class RangeExpression : Expression
{
    /// <summary>
    /// Creates a new range expression.
    /// </summary>
    /// <param name="start">The expression defining the start index of the range.</param>
    /// <param name="end">The expression defining the end index of the range.</param>
    public RangeExpression(Expression start, Expression end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Gets or sets the expression defining the start index of the range.
    /// </summary>
    public Expression Start
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the expression defining the end index of the range.
    /// </summary>
    public Expression End
    {
        get;
        set;
    }

    /// <inheritdoc />
    public override void AcceptVisitor<TState>(IExpressionVisitor<TState> visitor, TState state) =>
        visitor.VisitRangeExpression(this, state);

    /// <inheritdoc />
    public override TResult AcceptVisitor<TState, TResult>(IExpressionVisitor<TState, TResult> visitor, TState state)=>
        visitor.VisitRangeExpression(this, state);
}