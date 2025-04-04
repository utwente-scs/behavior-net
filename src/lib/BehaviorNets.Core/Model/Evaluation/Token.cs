using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace BehaviorNets.Model.Evaluation
{
    /// <summary>
    /// Represents a single token in a behavior net.
    /// </summary>
    public readonly struct Token : IEquatable<Token>
    {
        /// <summary>
        /// The empty token.
        /// </summary>
        public static readonly Token Empty = new(ImmutableDictionary<string, object?>.Empty);

        /// <summary>
        /// Creates a new token with the provided concretization of symbolic variables.
        /// </summary>
        /// <param name="variables"></param>
        public Token(ImmutableDictionary<string, object?> variables)
        {
            Variables = variables;
        }

        /// <summary>
        /// Gets a mapping from symbolic variables to their concrete values.
        /// </summary>
        public ImmutableDictionary<string, object?> Variables
        {
            get;
        }

        /// <summary>
        /// Assigns a new concrete value to the provided symbolic variable.
        /// </summary>
        /// <param name="name">The name of the symbolic variable.</param>
        /// <param name="value">The new concrete value.</param>
        /// <returns>The resulting token.</returns>
        public Token SetVariable(string name, object? value) => new(Variables.SetItem(name, value));

        /// <summary>
        /// Determines whether the current token is not in conflict with the provided token. That is, every
        /// concrete value assignment to a symbolic variable in one token does not conflict with any assignment
        /// in the other.
        /// </summary>
        /// <param name="other">The other token.</param>
        /// <returns><c>true</c> if the token is compatible, <c>false</c> otherwise.</returns>
        public bool IsCompatibleWith(Token other)
        {
            foreach ((var key, object? myValue) in Variables)
            {
                if (other.Variables.TryGetValue(key, out var otherValue) && !Equals(myValue, otherValue))
                    return false;
            }

            foreach ((var key, object? otherValue) in other.Variables)
            {
                if (Variables.TryGetValue(key, out var myValue) && !Equals(otherValue, myValue))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Merges the current token with the provided token.
        /// </summary>
        /// <param name="other">The other token to merge with.</param>
        /// <returns>The new token.</returns>
        /// <exception cref="ArgumentException">Occurs when the tokens are in conflict.</exception>
        public Token Merge(Token other)
        {
            var newToken = this;
            foreach ((var key, object? otherValue) in other.Variables)
            {
                if (Variables.TryGetValue(key, out object? myValue))
                {
                    if (!Equals(otherValue, myValue))
                        throw new ArgumentException($"Conflicting values for variable {key}.");
                }
                else
                {
                    newToken = newToken.SetVariable(key, otherValue);
                }
            }

            return newToken;
        }

        /// <summary>
        /// Attempts to merge the current token with the provided token.
        /// </summary>
        /// <param name="other">The other token to merge with.</param>
        /// <param name="merged">When this function returns <c>true</c>, this parameter contains the resulting token.</param>
        /// <returns><c>true</c> if merging succeeded, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentException">Occurs when the tokens are in conflict.</exception>
        public bool TryMerge(Token other,  out Token merged)
        {
            merged = this;
            foreach (var (key, otherValue) in other.Variables)
            {
                if (Variables.TryGetValue(key, out var myValue))
                {
                    if (!Equals(otherValue, myValue))
                        return false;
                }
                else
                {
                    merged = merged.SetVariable(key, otherValue);
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether two tokens contain the same assignments of concrete values.
        /// </summary>
        /// <param name="other">The other token to compare with.</param>
        /// <returns><c>true</c> if the tokens are equal, <c>false</c> othewrise.</returns>
        public bool Equals(Token other)
        {
            if (Variables.Count != other.Variables.Count)
                return false;

            foreach ((var key, object? value) in Variables)
            {
                if (!other.Variables.TryGetValue(key, out var otherValue) || !Equals(value, otherValue))
                    return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Token other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hash = 0;
            foreach (var entry in Variables.OrderBy(x => x.Key))
                hash ^= HashCode.Combine(entry.Key.GetHashCode(), entry.Value?.GetHashCode());
            return hash;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append('{');

            var entries = Variables.ToArray();
            for (int i = 0; i < entries.Length; i++)
            {
                var (key, value) = entries[i];
                builder.Append(key);
                builder.Append(": ");
                builder.AppendValue(value);

                if (i < entries.Length - 1)
                    builder.Append(", ");
            }

            builder.Append('}');
            return builder.ToString();
        }
    }
}
