using BehaviorNets.Model;
using BehaviorNets.Model.Evaluation;
using Xunit;

namespace BehaviorNets.Tests.Model.Evaluation;

public class BehaviorNetMarkingTest
{
    [Fact]
    public void TransitionWithNoInputsIsAlwaysEnabled()
    {
        var net = new BehaviorNet();
        var p1 = net.AddPlace("p1");
        var t1 = net
            .AddTransition("t1")
            .WithOutput(p1);

        var marking = new BehaviorNetMarking(net);

        Assert.True(marking.IsEnabled(t1, out var tokens));
        Assert.Equal(Token.Empty, Assert.Single(tokens));
    }

    [Fact]
    public void TransitionWithSingleInputIsEnabledWhenInputPlaceHasToken()
    {
        var net = new BehaviorNet();
        var p1 = net.AddPlace("p1");
        var t1 = net
            .AddTransition("t1")
            .WithInput(p1);

        var marking = new BehaviorNetMarking(net);
        marking.AddToken(p1, Token.Empty);

        Assert.True(marking.IsEnabled(t1, out var tokens));
        Assert.Equal(Token.Empty, Assert.Single(tokens));
    }

    [Fact]
    public void TransitionIsNotEnabledWhenNotAllInputPlacesHaveAToken()
    {
        var net = new BehaviorNet();
        var (p1, p2, p3) = net.AddPlaces("p1", "p2", "p3");
        var t1 = net
            .AddTransition("t1")
            .WithInputs(p1, p2, p3);

        var marking = new BehaviorNetMarking(net);

        Assert.False(marking.IsEnabled(t1, out _));

        marking.AddToken(p1, Token.Empty);
        Assert.False(marking.IsEnabled(t1, out _));

        marking.AddToken(p2, Token.Empty);
        Assert.False(marking.IsEnabled(t1, out _));

        marking.AddToken(p3, Token.Empty);
        Assert.True(marking.IsEnabled(t1, out _));
    }

    [Fact]
    public void TransitionIsNotEnabledWhenAllInputTokensAreConflicting()
    {
        var net = new BehaviorNet();
        var (p1, p2) = net.AddPlaces("p1", "p2");
        var t1 = net
            .AddTransition("t1")
            .WithInputs(p1, p2);

        var marking = new BehaviorNetMarking(net);

        marking.AddToken(p1, Token.Empty.SetVariable("var1", 123));
        marking.AddToken(p2, Token.Empty.SetVariable("var1", 456));

        Assert.False(marking.IsEnabled(t1, out _));
    }

    [Fact]
    public void TransitionIsEnabledWhenAllInputTokensAreCompatible()
    {
        var net = new BehaviorNet();
        var (p1, p2) = net.AddPlaces("p1", "p2");
        var t1 = net
            .AddTransition("t1")
            .WithInputs(p1, p2);

        var marking = new BehaviorNetMarking(net);

        marking.AddToken(p1, Token.Empty.SetVariable("var1", 123));
        Assert.False(marking.IsEnabled(t1, out _));

        marking.AddToken(p2, Token.Empty.SetVariable("var2", 456));
        Assert.True(marking.IsEnabled(t1, out var tokens));

        var merged = Assert.Single(tokens);
        Assert.Equal(Token.Empty
                .SetVariable("var1", 123)
                .SetVariable("var2", 456),
            merged);
    }

    [Fact]
    public void EnabledTransitionShouldEnumerateAllPossibleMerges()
    {
        var net = new BehaviorNet();
        var (p1, p2) = net.AddPlaces("p1", "p2");
        var t1 = net
            .AddTransition("t1")
            .WithInputs(p1, p2);

        var marking = new BehaviorNetMarking(net);

        marking.AddToken(p1, Token.Empty.SetVariable("var1", 123));
        marking.AddToken(p1, Token.Empty.SetVariable("var1", 456));
        marking.AddToken(p1, Token.Empty.SetVariable("var1", 789));
        marking.AddToken(p2, Token.Empty.SetVariable("var2", 111));
        marking.AddToken(p2, Token.Empty.SetVariable("var2", 222));

        Assert.True(marking.IsEnabled(t1, out var tokens));
        Assert.Equal(6, tokens.Length);

        Assert.Contains(Token.Empty
                .SetVariable("var1", 123)
                .SetVariable("var2", 111),
            tokens);
        Assert.Contains(Token.Empty
                .SetVariable("var1", 456)
                .SetVariable("var2", 111),
            tokens);
        Assert.Contains(Token.Empty
                .SetVariable("var1", 789)
                .SetVariable("var2", 111),
            tokens);

        Assert.Contains(Token.Empty
                .SetVariable("var1", 123)
                .SetVariable("var2", 222),
            tokens);
        Assert.Contains(Token.Empty
                .SetVariable("var1", 456)
                .SetVariable("var2", 222),
            tokens);
        Assert.Contains(Token.Empty
                .SetVariable("var1", 789)
                .SetVariable("var2", 222),
            tokens);
    }

    [Fact]
    public void RemovalOfTokensShouldDisableTransition()
    {
        var net = new BehaviorNet();
        var (p1, p2) = net.AddPlaces("p1", "p2");
        var t1 = net.AddTransition("t1")
            .WithInput(p1)
            .WithOutput(p2)
            .WithFunction(new ApiTransitionFunction("Signal"));

        var marking = new BehaviorNetMarking(net);
        marking.AddToken(p1, Token.Empty);

        Assert.True(marking.IsEnabled(t1, out _));

        marking.RemoveToken(p1, Token.Empty);
        Assert.False(marking.IsEnabled(t1, out _));
    }
}