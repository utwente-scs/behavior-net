using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace BehaviorNets.Model.Evaluation
{
    /// <summary>
    /// Represents a marking in a behavior net, containing all the tokens that were put in places.
    /// </summary>
    public class BehaviorNetMarking
    {
        private static readonly ImmutableArray<Token> GeneratorTokens = ImmutableArray.Create(Token.Empty);

        private readonly Dictionary<Place, HashSet<Token>> _tokens = new();
        private readonly Dictionary<Transition, ImmutableArray<Token>> _cachedInputTokens = new();

        /// <summary>
        /// Creates a new empty marking for the provided net.
        /// </summary>
        /// <param name="net">The net.</param>
        public BehaviorNetMarking(BehaviorNet net)
        {
            Net = net;
            foreach (var place in net.Places)
                _tokens[place] = new HashSet<Token>();
        }

        /// <summary>
        /// Gets the net for which this marking was constructed.
        /// </summary>
        public BehaviorNet Net
        {
            get;
        }

        /// <summary>
        /// Gets the tokens present at the provided place.
        /// </summary>
        /// <param name="place">The place to get the tokens from.</param>
        /// <returns>The tokens.</returns>
        public IReadOnlyCollection<Token> GetTokens(Place place) => _tokens[place];

        /// <summary>
        /// Adds a token to a place.
        /// </summary>
        /// <param name="place">The place to add the token to.</param>
        /// <param name="token">The token to add.</param>
        public void AddToken(Place place, Token token)
        {
            _tokens[place].Add(token);
            InvalidateCache(place);
        }

        /// <summary>
        /// Removes a token from a place.
        /// </summary>
        /// <param name="place">The place to remove a token from.</param>
        /// <param name="token">The token to remove.</param>
        public void RemoveToken(Place place, Token token)
        {
            _tokens[place].Remove(token);
            InvalidateCache(place);
        }

        private void InvalidateCache(Place place)
        {
            foreach (var transition in place.OutputTransitions)
                _cachedInputTokens.Remove(transition);
        }

        /// <summary>
        /// Determines whether a transition has sufficient tokens at all of its input places, and if so, produces
        /// all possible combined tokens that can be considered.
        /// </summary>
        /// <param name="transition">The transition to test.</param>
        /// <param name="possibleMerges">When this function returns <c>true</c>, this parameter will contain all possible
        /// combined input tokens that can be considered during the evaluation of the transition function.</param>
        /// <returns><c>true</c> if the transition is enabled, <c>false</c> otherwise.</returns>
        public bool IsEnabled(Transition transition, out ImmutableArray<Token> possibleMerges)
        {
            if (!_cachedInputTokens.TryGetValue(transition, out possibleMerges))
            {
                possibleMerges = EnumeratePossibleMerges(transition);
                _cachedInputTokens.Add(transition, possibleMerges);
            }

            return possibleMerges.Length > 0;
        }

        private ImmutableArray<Token> EnumeratePossibleMerges(Transition transition)
        {
            // Transitions without input places are always enabled, since they are token "generators".
            if (transition.InputPlaces.Count == 0)
                return GeneratorTokens;

            var result = ImmutableArray<Token>.Empty;

            // Compute total number of potential combinations, and simultaneously check whether all input places
            // actually have tokens. If not, this transition can never be enabled and we can short circuit.

            var tokenSets = new List<Token[]>();
            int totalCombinations = 1;

            foreach (var place in transition.InputPlaces)
            {
                var tokens = GetTokens(place);

                if (tokens.Count == 0)
                    return result;

                tokenSets.Add(tokens.ToArray());
                totalCombinations *= tokens.Count;
            }

            // Enumerate all possible merges. We do this by mapping all numbers from 0 to the total number of
            // combinations to one of the combinations. We use a conversion from number to set of tokens that is
            // similar to how one would convert a number into a different base, where "digit" j in the current
            // number indicates which token to pick from token set j.

            for (int i = 0; i < totalCombinations; i++)
            {
                Token? merged = Token.Empty;

                int n = i;
                for (int j = 0; j < tokenSets.Count && merged.HasValue; j++)
                {
                    int b = tokenSets[j].Length;
                    int digit = n % b;
                    n /= b;

                    merged = merged.Value.TryMerge(tokenSets[j][digit], out var newMerged)
                        ? newMerged
                        : null;
                }

                if (merged is { } newToken)
                    result = result.Add(newToken);
            }

            return result;
        }
    }
}
