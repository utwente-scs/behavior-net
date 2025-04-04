using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BehaviorNets.Data;
using BehaviorNets.Model.Ast;
using BehaviorNets.Model.Evaluation;

namespace BehaviorNets.Model
{
    /// <summary>
    /// Provides an implementation for the <see cref="ITransitionFunction"/> that matches on an API function call.
    /// </summary>
    public class ApiTransitionFunction : ITransitionFunction
    {
        /// <summary>
        /// Creates a new transition function that matches on API calls with the provided API name.
        /// </summary>
        /// <param name="apiName">The name of the API function to match on.</param>
        public ApiTransitionFunction(string apiName)
            : this(apiName, 0)
        {
        }

        /// <summary>
        /// Creates a new transition function that matches on API calls with the provided API name.
        /// </summary>
        /// <param name="apiName">The name of the API function to match on.</param>
        /// <param name="argumentCount">The number of arguments the API function defines.</param>
        public ApiTransitionFunction(string apiName, int argumentCount)
        {
            ApiName = apiName;
            Arguments = new List<string?>();
            Constraints = new List<Expression>();
            EnsureArgumentSlotsCreated(argumentCount - 1);
        }

        /// <summary>
        /// Gets the name of the API function to match on.
        /// </summary>
        public string? ApiName
        {
            get;
        }

        /// <summary>
        /// Gets a list of symbolic variable names to use for capturing the arguments of the API function call.
        /// </summary>
        /// <remarks>
        /// If a name in this list is <c>null</c>, then the argument is not captured by a symbolic variable.
        /// </remarks>
        public IList<string?> Arguments
        {
            get;
        }

        /// <summary>
        /// Gets or sets the name of the symbolic variable to use for capturing the return value of the API function call.
        /// </summary>
        /// <remarks>
        /// If this property is set to <c>null</c>, then the return value is not captured by a symbolic variable.
        /// </remarks>
        public string? ReturnValue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the symbolic variable to use for capturing the ID of the process responsible for
        /// calling the API function.
        /// </summary>
        /// <remarks>
        /// If this property is set to <c>null</c>, then the process ID is not captured by a symbolic variable.
        /// </remarks>
        public string? Process
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the symbolic variable to use for capturing the ID of the thread responsible for
        /// calling the API function.
        /// </summary>
        /// <remarks>
        /// If this property is set to <c>null</c>, then the thread ID is not captured by a symbolic variable.
        /// </remarks>
        public string? Thread
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of additional constraints put on every symbolic variable.
        /// </summary>
        public IList<Expression> Constraints
        {
            get;
        }

        /// <summary>
        /// Assigns a symbolic variable to the provided argument of the API call.
        /// </summary>
        /// <param name="index">The index of the argument to capture.</param>
        /// <param name="name">The name of the symbolic variable to use.</param>
        /// <returns>The current transition function.</returns>
        public ApiTransitionFunction CaptureArgument(int index, string name)
        {
            EnsureArgumentSlotsCreated(index);
            Arguments[index] = name;
            return this;
        }

        private void EnsureArgumentSlotsCreated(int index)
        {
            while (Arguments.Count <= index)
                Arguments.Add(null);
        }

        /// <summary>
        /// Assigns a symbolic variable to the return value of the API call.
        /// </summary>
        /// <param name="name">The name of the symbolic variable to use.</param>
        /// <returns>The current transition function.</returns>
        public ApiTransitionFunction CaptureReturn(string name)
        {
            ReturnValue = name;
            return this;
        }

        /// <summary>
        /// Assigns a symbolic variable to the ID of the process responsible for calling the API function.
        /// </summary>
        /// <param name="name">The name of the symbolic variable to use.</param>
        /// <returns>The current transition function.</returns>
        public ApiTransitionFunction CaptureProcess(string name)
        {
            Process = name;
            return this;
        }

        /// <summary>
        /// Assigns a symbolic variable to the ID of the thread responsible for calling the API function.
        /// </summary>
        /// <param name="name">The name of the symbolic variable to use.</param>
        /// <returns>The current transition function.</returns>
        public ApiTransitionFunction CaptureThread(string name)
        {
            Thread = name;
            return this;
        }

        /// <summary>
        /// Adds an additional constraint to the API function call.
        /// </summary>
        /// <param name="expression">The constraint expression</param>
        /// <returns>The current transition function.</returns>
        public ApiTransitionFunction WithConstraint(Expression expression)
        {
            Constraints.Add(expression);
            return this;
        }

        /// <inheritdoc />
        public bool Evaluate(ExecutionEvent @event, ref Token token)
        {
            if (string.IsNullOrEmpty(ApiName))
                return true;

            // Does the event name match?
            if (@event.Name != ApiName)
                return false;

            // Do the arguments match?
            for (int i = 0; i < Arguments.Count && i < @event.Arguments.Count; i++)
            {
                string? variableName = Arguments[i];
                if (variableName is not null && !MatchAndUpdateState(@event, ref token, variableName, @event.Arguments[i]))
                    return false;
            }

            // Does the expected return value match?
            if (!string.IsNullOrEmpty(ReturnValue) && !MatchAndUpdateState(@event, ref token, ReturnValue, @event.ReturnValue))
                return false;

            // Does the expected process ID match?
            if (!string.IsNullOrEmpty(Process) && !MatchAndUpdateState(@event, ref token, Process, Convert.ToUInt64(@event.ProcessId)))
                return false;

            // Does the expected thread ID match?
            if (!string.IsNullOrEmpty(Thread) && !MatchAndUpdateState(@event, ref token, Thread, Convert.ToUInt64(@event.ThreadId)))
                return false;

            // Set special variables pid and tid.
            var state = token
                .SetVariable("pid", (ulong) @event.ProcessId)
                .SetVariable("tid", (ulong) @event.ThreadId);

            // Are all the remaining custom constraints evaluating to `true`?   
            for (var i = 0; i < Constraints.Count; i++)
            {
                if (!(bool) Constraints[i].AcceptVisitor(ExpressionEvaluator.Instance, state))
                    return false;
            }

            return true;
        }

        private static bool MatchAndUpdateState(ExecutionEvent @event, ref Token s, string variableName, object? concreteValue)
        {
            if (s.Variables.TryGetValue(variableName, out object? value))
            {
                // We already have a value captured for this variable, check if it matches.
                if (!Equals(value, concreteValue))
                    return false;
            }
            else
            {
                // The condition defines a variable for this argument but it wasn't encountered before.
                // Just register it.
                s = s.SetVariable(variableName, concreteValue);
            }

            return true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(ApiName);
            builder.Append('(');

            for (int i = 0; i < Arguments.Count; i++)
            {
                builder.Append(Arguments[i] ?? "_");
                if (i < Arguments.Count - 1)
                    builder.Append(", ");
            }

            builder.Append(')');

            if (!string.IsNullOrEmpty(ReturnValue))
            {
                builder.Append(" -> ");
                builder.Append(ReturnValue);
            }

            if (!string.IsNullOrEmpty(Process) || !string.IsNullOrEmpty(Thread))
            {
                builder.AppendLine();
                builder.AppendLine("in");

                if (!string.IsNullOrEmpty(Process))
                {
                    builder.Append("   process ");
                    builder.Append(Process);
                }

                if (!string.IsNullOrEmpty(Thread))
                {
                    if (!string.IsNullOrEmpty(Process))
                        builder.AppendLine();
                    builder.Append("   thread ");
                    builder.AppendLine(Thread);
                }
            }

            if (Constraints.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("where");
                foreach (var constraint in Constraints)
                {
                    builder.Append("   ");
                    builder.AppendLine(constraint.ToString());
                }
            }

            return builder.ToString();
        }
    }
}
