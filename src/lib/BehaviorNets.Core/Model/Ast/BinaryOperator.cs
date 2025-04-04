namespace BehaviorNets.Model.Ast;

/// <summary>
/// Defines all operators that can be used in a <see cref="BinaryExpression"/>.
/// </summary>
public enum BinaryOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEquals,
    LessThan,
    LessThanOrEquals,
    BooleanAnd,
    BooleanOr,
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,
    BitwiseOr,
    BitwiseAnd,
    BitwiseXor,
    In
}