using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.OntologyManager;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public class Problem : IEquatable<Problem>
    {
        public Problem([NotNull] string type,
            [NotNull] IReadOnlyCollection<Class> goalTypes,
            [NotNull] IReadOnlyCollection<Class> suggestTypes)
        {
            Check.NotEmpty(type, nameof(type));
            Check.NotEmpty(goalTypes, nameof(goalTypes));
            Check.NotEmpty(suggestTypes, nameof(suggestTypes));

            Type = type;
            GoalTypes = goalTypes;
            SuggestTypes = suggestTypes;
        }

        [NotNull]
        public string Type { get; }

        [NotNull]
        public IReadOnlyCollection<Class> GoalTypes { get; }

        [NotNull]
        public IReadOnlyCollection<Class> SuggestTypes { get; }

        public bool Equals(Problem other)
        {
            return string.Equals(Type, other.Type);
        }

        public static bool operator ==(Problem problem1, Problem problem2)
        {
            if (ReferenceEquals(problem1, problem2)) return true;
            if (ReferenceEquals(null, problem1)) return false;
            if (ReferenceEquals(null, problem2)) return false;
            return problem1.Equals(problem2);
        }

        public static bool operator !=(Problem problem1, Problem problem2)
        {
            return !(problem1 == problem2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Problem) obj);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }
    }
}