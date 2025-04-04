using BehaviorNets.Data;
using BehaviorNets.Model;
using BehaviorNets.Model.Evaluation;
using Xunit;

namespace BehaviorNets.Tests.Model.Evaluation;

public class FullEvaluationTest
{
    private static readonly BehaviorNet SimplePath;
    private static readonly BehaviorNet ForkJoin;
    private static readonly BehaviorNet SimplePathWithProcessMatch;
    private static readonly BehaviorNet SimplePathWithThreadMatch;

    static FullEvaluationTest()
    {
        SimplePath = new BehaviorNet();
        var (p1, p2) = SimplePath.AddPlaces("p1", "p2");

        SimplePath.AddTransition("t1")
            .WithOutput(p1)
            .WithFunction(new ApiTransitionFunction("Signal1").CaptureArgument(0, "arg1"));

        SimplePath.AddTransition("t2")
            .WithInput(p1)
            .WithOutput(p2)
            .WithFunction(new ApiTransitionFunction("Signal2").CaptureArgument(0, "arg1"));

        p2.IsAccepting = true;

        ForkJoin = new BehaviorNet();
        var places = ForkJoin.AddPlaces("p0", "p1", "p2", "p3", "p4", "p5");

        ForkJoin.AddTransition("t1")
            .WithOutput(places[0])
            .WithFunction(new ApiTransitionFunction("Signal1").CaptureArgument(0, "arg1"));

        ForkJoin.AddTransition("t2")
            .WithInput(places[0])
            .WithOutputs(places[1], places[3])
            .WithFunction(new ApiTransitionFunction("Signal2").CaptureArgument(0, "arg2"));

        ForkJoin.AddTransition("t3")
            .WithInput(places[1])
            .WithOutput(places[2])
            .WithFunction(new ApiTransitionFunction("Signal3").CaptureArgument(0, "arg3"));

        ForkJoin.AddTransition("t4")
            .WithInputs(places[3])
            .WithOutput(places[4])
            .WithFunction(new ApiTransitionFunction("Signal4").CaptureArgument(0, "arg4"));

        ForkJoin.AddTransition("t5")
            .WithInputs(places[2], places[4])
            .WithOutput(places[5])
            .WithFunction(new ApiTransitionFunction("Signal5").CaptureArgument(0, "arg5"));

        places[5].IsAccepting = true;

        SimplePathWithProcessMatch = new BehaviorNet();
        places = SimplePathWithProcessMatch.AddPlaces(new[] { "p1", "p2" });

        SimplePathWithProcessMatch.AddTransition("t1")
            .WithOutput(places[0])
            .WithFunction(new ApiTransitionFunction("Signal1").CaptureProcess("processId"));

        SimplePathWithProcessMatch.AddTransition("t2")
            .WithInput(places[0])
            .WithOutput(places[1])
            .WithFunction(new ApiTransitionFunction("Signal2").CaptureProcess("processId"));

        places[1].IsAccepting = true;

        SimplePathWithThreadMatch = new BehaviorNet();
        places = SimplePathWithThreadMatch.AddPlaces(new[] { "p1", "p2" });

        SimplePathWithThreadMatch.AddTransition("t1")
            .WithOutput(places[0])
            .WithFunction(new ApiTransitionFunction("Signal1").CaptureThread("threadId"));

        SimplePathWithThreadMatch.AddTransition("t2")
            .WithInput(places[0])
            .WithOutput(places[1])
            .WithFunction(new ApiTransitionFunction("Signal2").CaptureThread("threadId"));

        places[1].IsAccepting = true;
    }

    [Fact]
    public void SimpleMatch()
    {
        var evaluator = new BehaviorNetEvaluator(SimplePath);

        evaluator.Step(new ExecutionEvent(default, "Signal1", 123));
        evaluator.Step(new ExecutionEvent(default, "Signal2", 123));

        Assert.True(evaluator.IsAccepting);
    }

    [Fact]
    public void NonMatchingSignals()
    {
        var evaluator = new BehaviorNetEvaluator(SimplePath);

        evaluator.Step(new ExecutionEvent(default, "Signal1", 123));
        Assert.False(evaluator.IsAccepting);
    }

    [Fact]
    public void NonMatchingSignalArguments()
    {
        var evaluator = new BehaviorNetEvaluator(SimplePath);

        evaluator.Step(new ExecutionEvent(default, "Signal1", 123));
        evaluator.Step(new ExecutionEvent(default, "Signal2", 456));
        Assert.False(evaluator.IsAccepting);
    }

    [Fact]
    public void GreedyMatch()
    {
        var evaluator = new BehaviorNetEvaluator(SimplePath);

        evaluator.Step(new ExecutionEvent(default, "Signal1", 123));
        evaluator.Step(new ExecutionEvent(default, "Signal1", 456));
        evaluator.Step(new ExecutionEvent(default, "Signal2", 123));

        Assert.True(evaluator.IsAccepting);
    }

    [Fact]
    public void LazyMatch()
    {
        var evaluator = new BehaviorNetEvaluator(SimplePath);

        evaluator.Step(new ExecutionEvent(default, "Signal1", 123));
        evaluator.Step(new ExecutionEvent(default, "Signal1", 456));
        evaluator.Step(new ExecutionEvent(default, "Signal2", 456));

        Assert.True(evaluator.IsAccepting);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForkJoinOrderDoesNotMatter(bool swap)
    {
        var evaluator = new BehaviorNetEvaluator(ForkJoin);

        evaluator.Step(new ExecutionEvent(default, "Signal1", 1));
        evaluator.Step(new ExecutionEvent(default, "Signal2", 2));

        if (swap)
        {
            evaluator.Step(new ExecutionEvent(default, "Signal4", 4));
            evaluator.Step(new ExecutionEvent(default, "Signal3", 3));
        }
        else
        {
            evaluator.Step(new ExecutionEvent(default, "Signal3", 3));
            evaluator.Step(new ExecutionEvent(default, "Signal4", 4));
        }

        evaluator.Step(new ExecutionEvent(default, "Signal5", 5));

        Assert.True(evaluator.IsAccepting);
    }

    [Fact]
    public void MatchOnProcessId()
    {
        var evaluator = new BehaviorNetEvaluator(SimplePathWithProcessMatch);

        evaluator.Step(new ExecutionEvent(default, 1, 2, "Signal1"));
        Assert.False(evaluator.IsAccepting);
        evaluator.Step(new ExecutionEvent(default, 2, 2, "Signal2"));
        Assert.False(evaluator.IsAccepting);
        evaluator.Step(new ExecutionEvent(default, 1, 2, "Signal2"));
        Assert.True(evaluator.IsAccepting);
    }

    [Fact]
    public void MatchOnThreadId()
    {
        var evaluator = new BehaviorNetEvaluator(SimplePathWithThreadMatch);

        evaluator.Step(new ExecutionEvent(default, 1, 1, "Signal1"));
        Assert.False(evaluator.IsAccepting);
        evaluator.Step(new ExecutionEvent(default, 1, 2, "Signal2"));
        Assert.False(evaluator.IsAccepting);
        evaluator.Step(new ExecutionEvent(default, 1, 1, "Signal2"));
        Assert.True(evaluator.IsAccepting);
    }
}