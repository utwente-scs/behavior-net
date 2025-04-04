namespace BehaviorNets.Model.Ast;

/// <summary>
/// Provides members for traversing an abstract syntax tree.
/// </summary>
/// <typeparam name="TState">The type of an object that can be passed along during the traversal.</typeparam>
public interface IExpressionVisitor<in TState>
{
    void VisitLiteralExpression(LiteralExpression expression, TState state);
    void VisitVariableExpression(VariableExpression expression, TState state);
    void VisitBinaryExpression(BinaryExpression expression, TState state);
    void VisitUnaryExpression(UnaryExpression expression, TState state);
    void VisitRangeExpression(RangeExpression expression, TState state);
}

/// <summary>
/// Provides members for traversing an abstract syntax tree.
/// </summary>
/// <typeparam name="TState">The type of an object that can be passed along during the traversal.</typeparam>
/// <typeparam name="TResult">The type of result to produce after every visit of a node.</typeparam>
public interface IExpressionVisitor<in TState, out TResult>
{
    TResult VisitLiteralExpression(LiteralExpression expression, TState state);
    TResult VisitVariableExpression(VariableExpression expression, TState state);
    TResult VisitBinaryExpression(BinaryExpression expression, TState state);
    TResult VisitUnaryExpression(UnaryExpression expression, TState state);
    TResult VisitRangeExpression(RangeExpression expression, TState state);
}