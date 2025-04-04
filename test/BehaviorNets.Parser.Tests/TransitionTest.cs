using System;
using BehaviorNets.Model;
using BehaviorNets.Model.Ast;
using Xunit;

namespace BehaviorNets.Parser.Tests;

public class TransitionTest
{
    [Fact]
    public void SimpleTransition()
    {
        var net = BehaviorNetFactory.FromText(@"
behavior {
    transition t {
        Signal()
    }
}");

        var transition = Assert.Single(net.Transitions);
        Assert.Equal("t", transition.Name);
        var predicate = Assert.IsAssignableFrom<ApiTransitionFunction>(transition.TransitionFunction);
        Assert.Equal("Signal", predicate.ApiName);
        Assert.Empty(predicate.Arguments);
        Assert.Empty(predicate.Constraints);
    }

    [Fact]
    public void SimpleTransitionMultipleArguments()
    {
        var net = BehaviorNetFactory.FromText(@"
behavior {
    transition t {
        Signal(a, b, _, d)
    }
}");

        var transition = Assert.Single(net.Transitions);
        Assert.Equal("t", transition.Name);
        var predicate = Assert.IsAssignableFrom<ApiTransitionFunction>(transition.TransitionFunction);
        Assert.Equal("Signal", predicate.ApiName);
        Assert.Equal(new[] {"a","b", null,"d"}, predicate.Arguments);
        Assert.Empty(predicate.Constraints);
    }

    [Fact]
    public void ConditionWithReturnValue()
    {
        var net = BehaviorNetFactory.FromText(@"
behavior {
    transition t {
        Signal(a) -> b
    }
}");

        var transition = Assert.Single(net.Transitions);
        Assert.Equal("t", transition.Name);
        var predicate = Assert.IsAssignableFrom<ApiTransitionFunction>(transition.TransitionFunction);
        Assert.Equal("b", predicate.ReturnValue);
    }

    [Theory]
    [InlineData(">", BinaryOperator.GreaterThan)]
    [InlineData(">=", BinaryOperator.GreaterThanOrEquals)]
    [InlineData("<", BinaryOperator.LessThan)]
    [InlineData("<=", BinaryOperator.LessThanOrEquals)]
    [InlineData("==", BinaryOperator.Equals)]
    [InlineData("!=", BinaryOperator.NotEquals)]
    public void SimpleBooleanExpression(string op, BinaryOperator expected)
    {
        var net = BehaviorNetFactory.FromText(@"
behavior {
    transition t {
        Signal(a, b)
        where
            a " + op + @" b
    }
}");

        var transition = Assert.Single(net.Transitions);
        Assert.Equal("t", transition.Name);
        var predicate = Assert.IsAssignableFrom<ApiTransitionFunction>(transition.TransitionFunction);
        var constraint = Assert.IsAssignableFrom<BinaryExpression>(Assert.Single(predicate.Constraints));
        Assert.Equal(expected, constraint.Operator);
    }

    [Theory]
    [InlineData("+")]
    [InlineData("-")]
    [InlineData("/")]
    [InlineData("*")]
    [InlineData("%")]
    public void NonBooleanExpressionAsConstraintShouldThrow(string op)
    {
        Assert.Throws<ArgumentException>(() => BehaviorNetFactory.FromText(@"
behavior {
    transition t {
        Signal(a, b)
        where
            a " + op + @" b
    }
}"));
    }

    [Fact]
    public void InExpressionOnRange()
    {
        var net = BehaviorNetFactory.FromText(@"
behavior {
    transition t {
        Signal(a)
        where
            a in [1..100]
    }
}");

        var transition = Assert.Single(net.Transitions);
        Assert.Equal("t", transition.Name);
        var predicate = Assert.IsAssignableFrom<ApiTransitionFunction>(transition.TransitionFunction);
        var constraint = Assert.IsAssignableFrom<BinaryExpression>(Assert.Single(predicate.Constraints));
        Assert.Equal(BinaryOperator.In, constraint.Operator);
        Assert.IsAssignableFrom<RangeExpression>(constraint.Right);
    }

    [Fact]
    public void InExpressionOnString()
    {
        var net = BehaviorNetFactory.FromText(@"
behavior {
    transition t {
        Signal(a)
        where
            ""Test"" in a
    }
}");

        var transition = Assert.Single(net.Transitions);
        Assert.Equal("t", transition.Name);
        var predicate = Assert.IsAssignableFrom<ApiTransitionFunction>(transition.TransitionFunction);
        var constraint = Assert.IsAssignableFrom<BinaryExpression>(Assert.Single(predicate.Constraints));
        Assert.Equal(BinaryOperator.In, constraint.Operator);
        Assert.IsAssignableFrom<VariableExpression>(constraint.Right);
    }

    [Fact]
    public void ConditionWithReturnValueAndWhereClause()
    {
        var net = BehaviorNetFactory.FromText(@"
behavior {
    transition t {
        Signal(a) -> b
        where
            a == b
    }
}");

        var transition = Assert.Single(net.Transitions);
        Assert.Equal("t", transition.Name);
        var predicate = Assert.IsAssignableFrom<ApiTransitionFunction>(transition.TransitionFunction);
        Assert.Equal("b", predicate.ReturnValue);
        var constraint = Assert.IsAssignableFrom<BinaryExpression>(Assert.Single(predicate.Constraints));
        Assert.Equal(BinaryOperator.Equals, constraint.Operator);
        var a = Assert.IsAssignableFrom<VariableExpression>(constraint.Left);
        var b = Assert.IsAssignableFrom<VariableExpression>(constraint.Right);
        Assert.Equal("a", a.Name);
        Assert.Equal("b", b.Name);
    }

    [Fact]
    public void ConditionMatchingOnProcessId()
    {
        var net = BehaviorNetFactory.FromText(@"
behavior {
    transition t {
        Signal()
        in 
            process pid1
    }
}");

        var transition = Assert.Single(net.Transitions);
        Assert.Equal("t", transition.Name);
        var predicate = Assert.IsAssignableFrom<ApiTransitionFunction>(transition.TransitionFunction);
        Assert.Equal("pid1", predicate.Process);
        Assert.Null(predicate.Thread);
    }

    [Fact]
    public void ConditionMatchingOnThreadId()
    {
        var net = BehaviorNetFactory.FromText(@"
behavior {
    transition t {
        Signal()
        in 
            thread tid1
    }
}");

        var transition = Assert.Single(net.Transitions);
        Assert.Equal("t", transition.Name);
        var predicate = Assert.IsAssignableFrom<ApiTransitionFunction>(transition.TransitionFunction);
        Assert.Null(predicate.Process);
        Assert.Equal("tid1", predicate.Thread);
    }

    [Fact]
    public void ConditionMatchingOnProcessAndThreadId()
    {
        var net = BehaviorNetFactory.FromText(@"
behavior {
    transition t {
        Signal()
        in 
            process pid1
            thread tid1
    }
}");

        var transition = Assert.Single(net.Transitions);
        Assert.Equal("t", transition.Name);
        var predicate = Assert.IsAssignableFrom<ApiTransitionFunction>(transition.TransitionFunction);
        Assert.Equal("pid1", predicate.Process);
        Assert.Equal("tid1", predicate.Thread);
    }
}