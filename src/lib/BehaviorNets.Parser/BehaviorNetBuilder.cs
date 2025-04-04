using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;
using BehaviorNets.Model;
using BehaviorNets.Model.Ast;
using BehaviorNets.Parser.Internal;

namespace BehaviorNets.Parser;

internal sealed class BehaviorNetBuilder : BehaviorNetBaseListener
{
    private readonly Dictionary<string, INode> _nodes = new();
    private readonly ParseTreeProperty<Expression> _expressions = new();
    private readonly ParseTreeProperty<ExpressionType> _expressionTypes = new();
    private Transition? _currentTransition;
    private ApiTransitionFunction? _currentPredicate;

    public BehaviorNet? Result
    {
        get;
        private set;
    }

    /// <inheritdoc />
    public override void EnterBehavior(BehaviorNetParser.BehaviorContext context)
    {
        string? name = context.qualifiedName() is { } id
            ? ProcessIdentifier(context.qualifiedName())
            : null;

        Result = new BehaviorNet(name);
        base.EnterBehavior(context);
    }

    /// <inheritdoc />
    public override void ExitPlace(BehaviorNetParser.PlaceContext context)
    {
        var places = Result!.AddPlaces(context.idList().ID().Select(ProcessIdentifier).ToArray());
        bool isAccepting = context.ACCEPTING() is not null;

        foreach (var place in places)
        {
            _nodes.Add(place.Name, place);
            place.IsAccepting = isAccepting;
        }

        base.ExitPlace(context);
    }

    /// <inheritdoc />
    public override void EnterTransition(BehaviorNetParser.TransitionContext context)
    {
        _currentTransition = Result!.AddTransition(ProcessIdentifier(context.ID()));
        _nodes.Add(_currentTransition.Name, _currentTransition);
        base.EnterTransition(context);
    }

    /// <inheritdoc />
    public override void ExitTransition(BehaviorNetParser.TransitionContext context)
    {
        if (_currentTransition is not null && _currentTransition.TransitionFunction is null)
            _currentTransition.TransitionFunction = IdentityTransitionFunction.Instance;
        _currentTransition = null;
        base.ExitTransition(context);
    }

    /// <inheritdoc />
    public override void EnterCondition(BehaviorNetParser.ConditionContext context)
    {
        var arguments = context.argument();
        _currentPredicate = new ApiTransitionFunction(ProcessIdentifier(context.ID()), arguments.Length);

        for (int i = 0; i < arguments.Length; i++)
        {
            if (arguments[i] is BehaviorNetParser.IdArgumentContext idArgument)
                _currentPredicate.CaptureArgument(i, ProcessIdentifier(idArgument.ID()));
        }

        _currentTransition!.TransitionFunction = _currentPredicate;
        base.EnterCondition(context);
    }

    /// <inheritdoc />
    public override void ExitCondition(BehaviorNetParser.ConditionContext context)
    {
        _currentPredicate = null;
        base.ExitCondition(context);
    }

    /// <inheritdoc />
    public override void ExitReturnClause(BehaviorNetParser.ReturnClauseContext context)
    {
        var returnValueArgument = context.argument();
        if (returnValueArgument is BehaviorNetParser.IdArgumentContext idArgument)
            _currentPredicate!.CaptureReturn(ProcessIdentifier(idArgument.ID()));

        base.ExitReturnClause(context);
    }

    /// <inheritdoc />
    public override void ExitInClause(BehaviorNetParser.InClauseContext context)
    {
        if (context.processClause() is { } processClause)
            _currentPredicate!.CaptureProcess(ProcessIdentifier(processClause.ID()));
        if (context.threadClause() is { } threadClause)
            _currentPredicate!.CaptureThread(ProcessIdentifier(threadClause.ID()));
        base.ExitInClause(context);
    }

    /// <inheritdoc />
    public override void ExitWhereClause(BehaviorNetParser.WhereClauseContext context)
    {
        var constraints = context.expression();
        foreach (var constraint in constraints)
        {
            ExpectType(ExpressionType.Boolean, constraint);
            _currentPredicate!.Constraints.Add(_expressions.Get(constraint));
        }

        base.ExitWhereClause(context);
    }

    /// <inheritdoc />
    public override void ExitParensExpression(BehaviorNetParser.ParensExpressionContext context)
    {
        _expressions.Put(context, _expressions.Get(context.expression()));
        _expressionTypes.Put(context, _expressionTypes.Get(context.expression()));
        base.ExitParensExpression(context);
    }

    /// <inheritdoc />
    public override void ExitNotExpression(BehaviorNetParser.NotExpressionContext context)
    {
        var (op, type) = context.OP_NOT().GetText() switch
        {
            "-" => (UnaryOperator.Negate, ExpressionType.Integer),
            "~" => (UnaryOperator.BitwiseNot, _expressionTypes.Get(context.expression())),
            _ => throw new ArgumentOutOfRangeException()
        };

        _expressions.Put(context, Expression.Unary(op, _expressions.Get(context.expression())));
        _expressionTypes.Put(context, type);
        base.ExitNotExpression(context);
    }

    /// <inheritdoc />
    public override void ExitMultiplicativeExpression(BehaviorNetParser.MultiplicativeExpressionContext context)
    {
        ExpectType(ExpressionType.Integer, context.expression(0));
        ExpectType(ExpressionType.Integer, context.expression(1));
        _expressionTypes.Put(context, ExpressionType.Integer);

        var op = context.OP_MUL().GetText()[0] switch
        {
            '*' => BinaryOperator.Multiply,
            '/' => BinaryOperator.Divide,
            '%' => BinaryOperator.Modulo,
            _ => throw new ArgumentOutOfRangeException()
        };

        _expressions.Put(context, Expression.Binary(
            _expressions.Get(context.expression(0)),
            op,
            _expressions.Get(context.expression(1))
        ));

        base.ExitMultiplicativeExpression(context);
    }

    /// <inheritdoc />
    public override void ExitAdditiveExpression(BehaviorNetParser.AdditiveExpressionContext context)
    {
        ExpectType(ExpressionType.Integer, context.expression(0));
        ExpectType(ExpressionType.Integer, context.expression(1));
        _expressionTypes.Put(context, ExpressionType.Integer);

        var op = context.OP_ADD().GetText()[0] switch
        {
            '+' => BinaryOperator.Add,
            '-' => BinaryOperator.Subtract,
            _ => throw new ArgumentOutOfRangeException()
        };

        _expressions.Put(context, Expression.Binary(
            _expressions.Get(context.expression(0)),
            op,
            _expressions.Get(context.expression(1))
        ));

        base.ExitAdditiveExpression(context);
    }

    /// <inheritdoc />
    public override void ExitBitwiseExpression(BehaviorNetParser.BitwiseExpressionContext context)
    {
        ExpectType(ExpressionType.Integer, context.expression(0));
        ExpectType(ExpressionType.Integer, context.expression(1));
        _expressionTypes.Put(context, ExpressionType.Integer);

        var op = context.OP_BIT().GetText()[0] switch
        {
            '&' => BinaryOperator.BitwiseAnd,
            '|' => BinaryOperator.BitwiseOr,
            '^' => BinaryOperator.BitwiseXor,
            _ => throw new ArgumentOutOfRangeException()
        };

        _expressions.Put(context, Expression.Binary(
            _expressions.Get(context.expression(0)),
            op,
            _expressions.Get(context.expression(1))
        ));

        base.ExitBitwiseExpression(context);
    }

    /// <inheritdoc />
    public override void ExitRelationalExpression(BehaviorNetParser.RelationalExpressionContext context)
    {
        var type1 = _expressionTypes.Get(context.expression(0));
        var type2 = _expressionTypes.Get(context.expression(1));

        if (type1 != ExpressionType.String && type2 != ExpressionType.String)
        {
            ExpectType(type1, context.expression(1));
        }

        _expressionTypes.Put(context, ExpressionType.Boolean);

        var op = context.OP_REL().GetText() switch
        {
            "<=" => BinaryOperator.LessThanOrEquals,
            "<" => BinaryOperator.LessThan,
            ">=" => BinaryOperator.GreaterThanOrEquals,
            ">" => BinaryOperator.GreaterThan,
            "==" => BinaryOperator.Equals,
            "!=" => BinaryOperator.NotEquals,
            _ => throw new ArgumentOutOfRangeException()
        };

        _expressions.Put(context, Expression.Binary(
            _expressions.Get(context.expression(0)),
            op,
            _expressions.Get(context.expression(1))
        ));

        base.ExitRelationalExpression(context);
    }

    /// <inheritdoc />
    public override void ExitInExpression(BehaviorNetParser.InExpressionContext context)
    {
        var needle = context.expression(0);
        var haystack = context.expression(1);

        _expressions.Put(context, Expression.Binary(
            _expressions.Get(needle),
            BinaryOperator.In,
            _expressions.Get(haystack)));
        _expressionTypes.Put(context, ExpressionType.Boolean);

        base.ExitInExpression(context);
    }

    /// <inheritdoc />
    public override void ExitAndExpression(BehaviorNetParser.AndExpressionContext context)
    {
        ExpectType(ExpressionType.Boolean, context.expression(0));
        ExpectType(ExpressionType.Boolean, context.expression(1));
        _expressionTypes.Put(context, ExpressionType.Boolean);

        _expressions.Put(context, Expression.Binary(
            _expressions.Get(context.expression(0)),
            BinaryOperator.BooleanAnd,
            _expressions.Get(context.expression(1))
        ));

        base.ExitAndExpression(context);
    }

    /// <inheritdoc />
    public override void ExitOrExpression(BehaviorNetParser.OrExpressionContext context)
    {
        ExpectType(ExpressionType.Boolean, context.expression(0));
        ExpectType(ExpressionType.Boolean, context.expression(1));
        _expressionTypes.Put(context, ExpressionType.Boolean);

        _expressions.Put(context, Expression.Binary(
            _expressions.Get(context.expression(0)),
            BinaryOperator.BooleanOr,
            _expressions.Get(context.expression(1))
        ));

        base.ExitOrExpression(context);
    }

    /// <inheritdoc />
    public override void ExitRangeExpression(BehaviorNetParser.RangeExpressionContext context)
    {
        ExpectType(ExpressionType.Integer, context.expression(0));
        ExpectType(ExpressionType.Integer, context.expression(1));

        var result = Expression.Range(
            _expressions.Get(context.expression(0)),
            _expressions.Get(context.expression(1)));

        _expressions.Put(context, result);
        _expressionTypes.Put(context, ExpressionType.Range);
        base.ExitRangeExpression(context);
    }

    /// <inheritdoc />
    public override void ExitNumberExpression(BehaviorNetParser.NumberExpressionContext context)
    {
        string numberLiteral = context.NUM().GetText();

        ulong number;
        if (numberLiteral.StartsWith("0x"))
            number = ulong.Parse(numberLiteral.Substring(2), NumberStyles.HexNumber);
        else if (numberLiteral[^1] == 'h')
            number = ulong.Parse(numberLiteral.Remove(numberLiteral.Length - 1), NumberStyles.HexNumber);
        else
            number = ulong.Parse(numberLiteral);

        _expressions.Put(context, Expression.Literal(number));
        _expressionTypes.Put(context, ExpressionType.Integer);
        base.ExitNumberExpression(context);
    }

    /// <inheritdoc />
    public override void ExitIdentifierExpression(BehaviorNetParser.IdentifierExpressionContext context)
    {
        _expressions.Put(context, Expression.Variable(ProcessIdentifier(context.ID())));
        _expressionTypes.Put(context, ExpressionType.Integer);
        base.ExitIdentifierExpression(context);
    }

    /// <inheritdoc />
    public override void ExitStringExpression(BehaviorNetParser.StringExpressionContext context)
    {
        _expressions.Put(context, Expression.Literal(ParseString(context.STR().GetText())));
        _expressionTypes.Put(context, ExpressionType.String);
        base.ExitStringExpression(context);
    }

    /// <inheritdoc />
    public override void ExitBoolExpression(BehaviorNetParser.BoolExpressionContext context)
    {
        _expressions.Put(context, Expression.Literal(context.BOOL().GetText() == "true"));
        _expressionTypes.Put(context, ExpressionType.Boolean);
        base.ExitBoolExpression(context);
    }

    private static string ParseString(string rawString)
    {
        var builder = new StringBuilder();

        bool escape = false;
        for (int i = 1; i < rawString.Length - 1; i++)
        {
            char c = rawString[i];

            if (c == '\\')
            {
                escape = !escape;
            }
            else if (escape)
            {
                c = c switch
                {
                    'n' => '\n',
                    'r' => '\r',
                    't' => '\t',
                    '"' => '"',
                    _ => throw new FormatException($"Unrecognized escape sequence \\{c}.")
                };
                escape = false;
            }

            if (!escape)
                builder.Append(c);
        }

        return builder.ToString();
    }

    /// <inheritdoc />
    public override void ExitEdgeChain(BehaviorNetParser.EdgeChainContext context)
    {
        var identifiers = context.ID();
        var previousNode = _nodes[ProcessIdentifier(identifiers[0])];
        for (int i = 1; i < identifiers.Length; i++)
        {
            var nextNode = _nodes[ProcessIdentifier(identifiers[i])];

            switch ((previousNode, nextNode))
            {
                case (Place p, Transition t):
                    t.InputPlaces.Add(p);
                    break;
                case (Transition t, Place p):
                    t.OutputPlaces.Add(p);
                    break;
                default:
                    throw new ArgumentException($"Invalid edge from {previousNode.Name} to {nextNode.Name} at {context.Start.Line}:{context.Start.Column}.");
            }

            previousNode = nextNode;
        }

        base.ExitEdgeChain(context);
    }

    private static string ProcessIdentifier(IParseTree node)
    {
        string rawIdentifier = node.GetText();
        return rawIdentifier[0] == '"'
            ? ParseString(rawIdentifier)
            : rawIdentifier;
    }

    private void ExpectType(ExpressionType expected, BehaviorNetParser.ExpressionContext expression)
    {
        var actualType = _expressionTypes.Get(expression);
        if (actualType != expected)
        {
            throw new ArgumentException(
                $"Expected {expected} typed expression at {expression.Start.Line}:{expression.Start.Column} but found {actualType}.");
        }
    }
}