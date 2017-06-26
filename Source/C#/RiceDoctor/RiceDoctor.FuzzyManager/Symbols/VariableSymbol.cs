using System;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.FuzzyManager
{
    public class VariableSymbol : Symbol, IEquatable<VariableSymbol>
    {
        public VariableSymbol([NotNull] string id, [NotNull] VariableStmt stmt)
        {
            Check.NotEmpty(id, nameof(id));
            Check.NotNull(stmt, nameof(stmt));

            Stmt = stmt;
            Id = id;
        }

        [NotNull]
        public VariableStmt Stmt { get; }

        public override string Id { get; }

        public bool Equals(VariableSymbol other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Id, other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((VariableSymbol) obj);
        }

        public static bool operator ==(VariableSymbol variableSymbol1, VariableSymbol variableSymbol2)
        {
            if (ReferenceEquals(variableSymbol1, variableSymbol2)) return true;
            if (ReferenceEquals(null, variableSymbol1)) return false;
            if (ReferenceEquals(null, variableSymbol2)) return false;
            return variableSymbol1.Equals(variableSymbol2);
        }

        public static bool operator !=(VariableSymbol variableSymbol1, VariableSymbol variableSymbol2)
        {
            return !(variableSymbol1 == variableSymbol2);
        }
    }
}