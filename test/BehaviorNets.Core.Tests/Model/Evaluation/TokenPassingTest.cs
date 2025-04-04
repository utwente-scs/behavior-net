using BehaviorNets.Data;
using BehaviorNets.Model;
using BehaviorNets.Model.Ast;
using BehaviorNets.Model.Evaluation;
using Xunit;

namespace BehaviorNets.Tests.Model.Evaluation;

public class TokenPassingTest
{
    [Fact]
    public void GeneratorTransitionNoPredicate()
    {
        var net = new BehaviorNet();
        var p1 = net.AddPlace("p1");
        var t1 = net
            .AddTransition("t1")
            .WithOutput(p1);

        var evaluator = new BehaviorNetEvaluator(net);
        Assert.Empty(evaluator.Marking.GetTokens(p1));

        evaluator.Step(new ExecutionEvent(default, "Signal"));
        Assert.NotEmpty(evaluator.Marking.GetTokens(p1));
    }

    [Fact]
    public void GeneratorTransitionWithPredicate()
    {
        var net = new BehaviorNet();
        var p1 = net.AddPlace("p1");
        var t1 = net
            .AddTransition("t1")
            .WithOutput(p1)
            .WithFunction(new ApiTransitionFunction("Signal1")
                .CaptureArgument(0, "arg1"));

        var evaluator = new BehaviorNetEvaluator(net);
        Assert.Empty(evaluator.Marking.GetTokens(p1));

        evaluator.Step(new ExecutionEvent(default, "Signal2", 123));
        Assert.Empty(evaluator.Marking.GetTokens(p1));

        evaluator.Step(new ExecutionEvent(default, "Signal1", 123));
        var token = Assert.Single(evaluator.Marking.GetTokens(p1));

        Assert.Equal(Token.Empty.SetVariable("arg1", 123), token);
    }

    [Fact]
    public void GeneratorTransitionWithConstrainedPredicate()
    {
        var net = new BehaviorNet();

        var p1 = net.AddPlace("p1");
        var t1 = net
            .AddTransition("t1")
            .WithOutput(p1)
            .WithFunction(new ApiTransitionFunction("Signal1")
                .CaptureArgument(0, "arg1")
                .WithConstraint(Expression.Binary("arg1", BinaryOperator.GreaterThan, 100))
                .WithConstraint(Expression.Binary("arg1", BinaryOperator.LessThan, 10000)));

        var evaluator = new BehaviorNetEvaluator(net);
        Assert.Empty(evaluator.Marking.GetTokens(p1));

        evaluator.Step(new ExecutionEvent(default, "Signal1", 10001));
        Assert.Empty(evaluator.Marking.GetTokens(p1));

        evaluator.Step(new ExecutionEvent(default, "Signal1", 123));
        var token = Assert.Single(evaluator.Marking.GetTokens(p1));

        Assert.Equal(Token.Empty.SetVariable("arg1", 123), token);
    }

    [Fact]
    public void TransitionShouldOnlyFireIfInputTokenIsCompatible()
    {
        var net = new BehaviorNet();
        var (p1, p2) = net.AddPlaces("p1", "p2");

        var t1 = net
            .AddTransition("t1")
            .WithOutput(p1)
            .WithFunction(new ApiTransitionFunction("Signal1")
                .CaptureArgument(0, "arg1"));

        var t2 = net
            .AddTransition("t1")
            .WithInput(p1)
            .WithOutput(p2)
            .WithFunction(new ApiTransitionFunction("Signal2")
                .CaptureArgument(0, "arg1"));

        var evaluator = new BehaviorNetEvaluator(net);

        evaluator.Step(new ExecutionEvent(default, "Signal1", 123));
        Assert.Empty(evaluator.Marking.GetTokens(p2));

        evaluator.Step(new ExecutionEvent(default, "Signal2", 456));
        Assert.Empty(evaluator.Marking.GetTokens(p2));

        evaluator.Step(new ExecutionEvent(default, "Signal2", 123));
        Assert.Equal(
            Token.Empty.SetVariable("arg1", 123),
            Assert.Single(evaluator.Marking.GetTokens(p2)));
    }

    [Theory]
    [InlineData(new[] {0, 1})]
    [InlineData(new[] {1, 0})]
    [InlineData(new[] {1, 2, 3, 0})]
    [InlineData(new[] {0, 2, 3, 1})]
    public void TransitionShouldNotFireIfInsufficientTokens(int[] orderings)
    {
        string[] signals = {"Signal1", "Signal2", "Signal3", "Signal4"};

        var net = new BehaviorNet();
        var (p1, p2, p3) = net.AddPlaces("p1", "p2", "p3");

        var t1 = net
            .AddTransition("t1")
            .WithOutput(p1)
            .WithFunction(new ApiTransitionFunction("Signal1"));

        var t2 = net
            .AddTransition("t1")
            .WithOutput(p2)
            .WithFunction(new ApiTransitionFunction("Signal2"));

        var t3 = net
            .AddTransition("t1")
            .WithInputs(p1, p2)
            .WithOutput(p3)
            .WithFunction(new ApiTransitionFunction("Final"));

        var evaluator = new BehaviorNetEvaluator(net);

        for (int i = 0; i < orderings.Length - 1; i++)
        {
            // Perform single step.
            evaluator.Step(new ExecutionEvent(default, signals[orderings[i]]));

            // Try final step.
            evaluator.Step(new ExecutionEvent(default, "Final"));

            // Verify we haven't had a transition yet to the final state.
            Assert.Empty(evaluator.Marking.GetTokens(p3));
        }

        // Perform last step. This time, we do transition to the final step.
        evaluator.Step(new ExecutionEvent(default, signals[orderings[^1]]));
        evaluator.Step(new ExecutionEvent(default, "Final"));
        Assert.NotEmpty(evaluator.Marking.GetTokens(p3));
    }

    [Fact]
    public void ForkingTransitionShouldProduceTwoTokens()
    {
        var net = new BehaviorNet();
        var (p1, p2, p3) = net.AddPlaces("p1", "p2", "p3");
        net.AddTransition("t1")
            .WithOutput(p1)
            .WithFunction(new ApiTransitionFunction("Signal1").CaptureArgument(0, "x"));
        net.AddTransition("t2")
            .WithInput(p1)
            .WithOutputs(p2, p3)
            .WithFunction(new ApiTransitionFunction("Signal2").CaptureArgument(0, "y"));

        var evaluator = new BehaviorNetEvaluator(net);
        evaluator.Step(new ExecutionEvent(default, "Signal1", 123));
        evaluator.Step(new ExecutionEvent(default, "Signal2", 456));

        var token1 = Assert.Single(evaluator.Marking.GetTokens(p2));
        var token2 = Assert.Single(evaluator.Marking.GetTokens(p3));
        Assert.Equal(token1, token2);
    }

    [Fact]
    public void ConstrainedPredicateUsingPreviouslyCapturedVariables()
    {
        var net = new BehaviorNet();
        var (p1, p2) = net.AddPlaces("p1", "p2");

        net.AddTransition("t1")
            .WithOutput(p1)
            .WithFunction(new ApiTransitionFunction("Signal1")
                .CaptureArgument(0, "begin")
                .CaptureArgument(1, "end"));

        net.AddTransition("t2")
            .WithInput(p1)
            .WithOutput(p2)
            .WithFunction(new ApiTransitionFunction("Signal2")
                .CaptureArgument(0, "x")
                .WithConstraint(Expression.Binary("x", BinaryOperator.GreaterThanOrEquals, "begin"))
                .WithConstraint(Expression.Binary("x", BinaryOperator.LessThanOrEquals, "end")));

        var evaluator = new BehaviorNetEvaluator(net);
        evaluator.Step(new ExecutionEvent(default, "Signal1", 1000, 2000));

        evaluator.Step(new ExecutionEvent(default, "Signal2", 3000));
        Assert.Empty(evaluator.Marking.GetTokens(p2));

        evaluator.Step(new ExecutionEvent(default, "Signal2", 1500));
        var token = Assert.Single(evaluator.Marking.GetTokens(p2));

        Assert.True(token.Variables.TryGetValue("x", out object value));
        Assert.Equal(1500, value);
    }
}