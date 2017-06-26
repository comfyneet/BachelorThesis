using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.FuzzyManager
{
    public class FunctionSymbol : Symbol, IEquatable<FunctionSymbol>
    {
        public FunctionSymbol([NotNull] string id, [NotNull] IReadOnlyList<string> @params, [NotNull] FunctionStmt stmt)
        {
            Check.NotEmpty(id, nameof(id));
            Check.NotNull(@params, nameof(@params));
            Check.NotNull(stmt, nameof(stmt));

            Id = id;
            Params = @params;
            Stmt = stmt;
        }

        [NotNull]
        public IReadOnlyList<string> Params { get; }

        [NotNull]
        public FunctionStmt Stmt { get; }

        public override string Id { get; }

        public bool Equals(FunctionSymbol other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Id, other.Id) && Params.ScrambledEqual(other.Params);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.GetHashCode() * 397) ^ Params.GetOrderIndependentHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FunctionSymbol) obj);
        }

        public static bool operator ==(FunctionSymbol functionSymbol1, FunctionSymbol functionSymbol2)
        {
            if (ReferenceEquals(functionSymbol1, functionSymbol2)) return true;
            if (ReferenceEquals(null, functionSymbol1)) return false;
            if (ReferenceEquals(null, functionSymbol2)) return false;
            return functionSymbol1.Equals(functionSymbol2);
        }

        public static bool operator !=(FunctionSymbol functionSymbol1, FunctionSymbol functionSymbol2)
        {
            return !(functionSymbol1 == functionSymbol2);
        }
    }
}