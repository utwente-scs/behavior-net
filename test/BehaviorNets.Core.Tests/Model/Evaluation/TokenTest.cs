using System;
using BehaviorNets.Model.Evaluation;
using Xunit;

namespace BehaviorNets.Tests.Model.Evaluation;

public class TokenTest
{
    [Fact]
    public void SetVariableShouldProduceTokenWithVariable()
    {
        var token = Token.Empty;
        var newToken = token.SetVariable("var1", 123);

        Assert.True(newToken.Variables.TryGetValue("var1", out object value));
        Assert.Equal(123, value);
    }

    [Fact]
    public void EmptyTokensEqual()
    {
        var token1 = Token.Empty;
        var token2 = Token.Empty;
        Assert.Equal(token1, token2);
    }

    [Fact]
    public void NonEmptyTokenEquality()
    {
        var token1 = Token.Empty.SetVariable("var1", 123);
        var token2 = Token.Empty.SetVariable("var1", 123);

        Assert.Equal(token1, token2);
    }

    [Fact]
    public void NonEmptyTokenInequality()
    {
        var token1 = Token.Empty.SetVariable("var1", 123);
        var token2 = Token.Empty.SetVariable("var2", 123);

        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void TokenIsCompatibleWithEmptyToken()
    {
        var token1 = Token.Empty.SetVariable("var1", 123);
        var token2 = Token.Empty;

        Assert.True(token1.IsCompatibleWith(token2));
        Assert.True(token2.IsCompatibleWith(token1));
    }

    [Fact]
    public void ConflictingVariableValuesShouldBeIncompatible()
    {
        var token1 = Token.Empty.SetVariable("var1", 123);
        var token2 = Token.Empty.SetVariable("var1", 456);

        Assert.False(token1.IsCompatibleWith(token2));
        Assert.False(token2.IsCompatibleWith(token1));
    }

    [Fact]
    public void SameVariableValuesShouldBeCompatible()
    {
        var token1 = Token.Empty.SetVariable("var1", 123);
        var token2 = Token.Empty.SetVariable("var1", 123);

        Assert.True(token1.IsCompatibleWith(token2));
        Assert.True(token2.IsCompatibleWith(token1));
    }

    [Fact]
    public void DifferentVariableNamesShouldBeCompatible()
    {
        var token1 = Token.Empty.SetVariable("var1", 123);
        var token2 = Token.Empty.SetVariable("var2", 456);

        Assert.True(token1.IsCompatibleWith(token2));
        Assert.True(token2.IsCompatibleWith(token1));
    }

    [Fact]
    public void MergeWithEmptyShouldResultInSameToken()
    {
        var token1 = Token.Empty.SetVariable("var1", 123);
        var token2 = Token.Empty;

        Assert.Equal(token1, token1.Merge(token2));
        Assert.Equal(token1, token2.Merge(token1));
    }

    [Fact]
    public void MergeConflictingTokensShouldThrow()
    {
        var token1 = Token.Empty.SetVariable("var1", 123);
        var token2 = Token.Empty.SetVariable("var1", 456);

        Assert.Throws<ArgumentException>(() => token1.Merge(token2));
        Assert.Throws<ArgumentException>(() => token2.Merge(token1));
    }

    [Fact]
    public void MergeCompatibleTokensShouldMergeVariables()
    {
        var token1 = Token.Empty.SetVariable("var1", 123);
        var token2 = Token.Empty.SetVariable("var2", 456);

        var merged = token1.Merge(token2);

        Assert.True(merged.Variables.TryGetValue("var1", out object value1));
        Assert.True(merged.Variables.TryGetValue("var2", out object value2));
        Assert.Equal(123, value1);
        Assert.Equal(456, value2);
    }

}