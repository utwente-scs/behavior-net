using System;
using Xunit;

namespace BehaviorNets.Parser.Tests;

public class EdgeTest
{
    [Fact]
    public void EdgeFromPlaceToTransition()
    {
        var net = BehaviorNetFactory.FromText(@"
behavior {
    place a
    transition t {
        Signal()
    }

    a -> t
}");

        var place = Assert.Single(net.Places);
        var transition = Assert.Single(net.Transitions);
        Assert.Contains(place, transition.InputPlaces);
    }

    [Fact]
    public void EdgeFromTransitionToPlace()
    {
        var net = BehaviorNetFactory.FromText(@"
behavior {
    place a
    transition t {
        Signal()
    }

    t -> a
}");

        var place = Assert.Single(net.Places);
        var transition = Assert.Single(net.Transitions);
        Assert.Contains(place, transition.OutputPlaces);
    }

    [Fact]
    public void EdgeFromTransitionToPTransitionShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => BehaviorNetFactory.FromText(@"
behavior {
    place a
    transition t {
        Signal()
    }

    t -> t
}"));
    }

    [Fact]
    public void EdgeFromPlaceToPlaceShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => BehaviorNetFactory.FromText(@"
behavior {
    place a
    transition t {
        Signal()
    }

    a -> a
}"));
    }

    [Fact]
    public void EdgeChainPlaceToTransitionToPlace()
    {
        var net = BehaviorNetFactory.FromText(@"
behavior {
    place [a b]
    transition t {
        Signal()
    }

    a -> t -> b
}");

        var transition = Assert.Single(net.Transitions);
        Assert.Equal("a", Assert.Single(transition.InputPlaces).Name);
        Assert.Equal("b", Assert.Single(transition.OutputPlaces).Name);
    }

    [Fact]
    public void EdgeChainTransitionToPlaceToTransition()
    {
        var net = BehaviorNetFactory.FromText(@"
behavior {
    place p
    transition t1 {
        Signal1()
    }
    transition t2 {
        Signal12()
    }

    t1 -> p -> t2
}");

        var place = Assert.Single(net.Places);
        Assert.Equal("t1", Assert.Single(place.InputTransitions).Name);
        Assert.Equal("t2", Assert.Single(place.OutputTransitions).Name);
    }

    [Fact]
    public void InvalidEdgeChainDoubleTransition()
    {
        Assert.Throws<ArgumentException>(() => BehaviorNetFactory.FromText(@"
behavior {
    place a
    transition t {
        Signal()
    }

    a -> t -> t -> a
}"));
    }

    [Fact]
    public void InvalidEdgeChainDoublePlace()
    {
        Assert.Throws<ArgumentException>(() => BehaviorNetFactory.FromText(@"
behavior {
    place a
    transition t {
        Signal()
    }

    t -> a -> a -> t
}"));
    }
}