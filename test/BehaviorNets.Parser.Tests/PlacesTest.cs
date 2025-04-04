using System.Linq;
using Xunit;

namespace BehaviorNets.Parser.Tests;

public class PlacesTest
{
    [Fact]
    public void AddSinglePlace()
    {
        var net = BehaviorNetFactory.FromText(@"
behavior {
    place a
}");

        var place = Assert.Single(net.Places);
        Assert.Equal("a", place.Name);
        Assert.False(place.IsAccepting);
    }

    [Fact]
    public void AddMultiplePlaces()
    {
        var net = BehaviorNetFactory.FromText(@"
behavior {
    place [a b c d e f g]
}");

        Assert.Equal(
            new[] {"a", "b", "c", "d", "e", "f", "g"}.ToHashSet(),
            net.Places.Select(p => p.Name).ToHashSet());
        Assert.All(net.Places, p => Assert.False(p.IsAccepting));
    }

    [Fact]
    public void AddSingleAcceptingState()
    {
        var net = BehaviorNetFactory.FromText(@"
behavior {
    place [a b c]
    place d accepting
}");

        Assert.DoesNotContain(net.Places, p => p.Name == "accepting");
        Assert.All(net.Places, p => Assert.Equal(p.Name == "d", p.IsAccepting));
    }

    [Fact]
    public void AddMultipleAcceptingState()
    {
        var net = BehaviorNetFactory.FromText(@"
behavior {
    place [a b c]
    place [d e f] accepting
}");

        Assert.DoesNotContain(net.Places, p => p.Name == "accepting");
        Assert.All(net.Places, p => Assert.Equal(p.Name == "d" || p.Name == "e" || p.Name == "f", p.IsAccepting));
    }
}