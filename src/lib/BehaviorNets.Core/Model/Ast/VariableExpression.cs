namespace BehaviorNets.Model.Ast;

/// <summary>
/// Represents a reference to a symbolic variable in an abstract syntax tree.
/// </summary>
public class VariableExpression : Expression
{
    /// <summary>
    /// Creates a new variable expression.
    /// </summary>
    /// <param name="name">The name of the referenced variable.</param>
    public VariableExpression(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets or sets the name of the referenced variable.
    /// </summary>
    public string Name
    {
        get;
        set;
    }

    /// <inheritdoc />
    public override void AcceptVisitor<TState>(IExpressionVisitor<TState> visitor, TState state) =>
        visitor.VisitVariableExpression(this, state);

    /// <inheritdoc />
    public override TResult AcceptVisitor<TState, TResult>(IExpressionVisitor<TState, TResult> visitor, TState state) =>
        visitor.VisitVariableExpression(this, state);
}