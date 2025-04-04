using BehaviorNets.Model;
using Xunit;

namespace BehaviorNets.Tests.Model;

public class BehaviorNetTest
{
    [Fact]
    public void AddOutputTransitionShouldUpdateInputPlaces()
    {
        var net = new BehaviorNet();
        var p1 = net.AddPlace("p1");
        var t1 = net.AddTransition("t1");

        Assert.True(p1.OutputTransitions.Add(t1));
        Assert.False(p1.OutputTransitions.Add(t1));

        Assert.Contains(p1, t1.InputPlaces);
    }

    [Fact]
    public void AddInputTransitionShouldUpdateOutputPlaces()
    {
        var net = new BehaviorNet();
        var p1 = net.AddPlace("p1");
        var t1 = net.AddTransition("t1");

        Assert.True(p1.InputTransitions.Add(t1));
        Assert.False(p1.InputTransitions.Add(t1));

        Assert.Contains(p1, t1.OutputPlaces);
    }
    [Fact]
    public void RemoveOutputTransitionShouldUpdateInputPlaces()
    {
        var net = new BehaviorNet();
        var p1 = net.AddPlace("p1");
        var t1 = net.AddTransition("t1");

        p1.OutputTransitions.Add(t1);

        Assert.True(p1.OutputTransitions.Remove(t1));
        Assert.False(p1.OutputTransitions.Remove(t1));
        Assert.DoesNotContain(p1, t1.InputPlaces);
    }

    [Fact]
    public void RemoveInputTransitionShouldUpdateOutputPlaces()
    {
        var net = new BehaviorNet();
        var p1 = net.AddPlace("p1");
        var t1 = net.AddTransition("t1");

        p1.InputTransitions.Add(t1);

        Assert.True(p1.InputTransitions.Remove(t1));
        Assert.False(p1.InputTransitions.Remove(t1));
        Assert.DoesNotContain(p1, t1.OutputPlaces);
    }

    [Fact]
    public void AddInputPlaceShouldUpdateOutputTransitions()
    {
        var net = new BehaviorNet();
        var p1 = net.AddPlace("p1");
        var t1 = net.AddTransition("t1");

        Assert.True(t1.InputPlaces.Add(p1));
        Assert.False(t1.InputPlaces.Add(p1));
        Assert.Contains(t1, p1.OutputTransitions);
    }

    [Fact]
    public void AddOutputPlaceShouldUpdateInputTransitions()
    {
        var net = new BehaviorNet();
        var p1 = net.AddPlace("p1");
        var t1 = net.AddTransition("t1");

        Assert.True(t1.OutputPlaces.Add(p1));
        Assert.False(t1.OutputPlaces.Add(p1));
        Assert.Contains(t1, p1.InputTransitions);
    }

    [Fact]
    public void RemoveInputPlaceShouldUpdateOutputTransitions()
    {
        var net = new BehaviorNet();
        var p1 = net.AddPlace("p1");
        var t1 = net.AddTransition("t1");
        t1.InputPlaces.Add(p1);

        Assert.True(t1.InputPlaces.Remove(p1));
        Assert.False(t1.InputPlaces.Remove(p1));
        Assert.DoesNotContain(t1, p1.OutputTransitions);
    }

    [Fact]
    public void RemoveOutputPlaceShouldUpdateInputTransitions()
    {
        var net = new BehaviorNet();
        var p1 = net.AddPlace("p1");
        var t1 = net.AddTransition("t1");
        t1.OutputPlaces.Add(p1);

        Assert.True(t1.OutputPlaces.Remove(p1));
        Assert.False(t1.OutputPlaces.Remove(p1));
        Assert.DoesNotContain(t1, p1.InputTransitions);
    }
}