using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public static class PetrickMethod<T>
    {
        [NotNull]
        public static IReadOnlyCollection<Implicant<T>> FindEssentialPrimeImplicants(
            [NotNull] IReadOnlyList<Implicant<T>> primeImplicants)
        {
            Check.NotEmpty(primeImplicants, nameof(primeImplicants));

            var sortedImplicantLists = SortImplicantsByIdentifiers(primeImplicants);

            var expandedImplicantLists = ExpandSortedImplicants(sortedImplicantLists);

            var essentialImplicants = SimplifyExpandedImplicants(expandedImplicantLists);

            return essentialImplicants;
        }

        [NotNull]
        private static IReadOnlyDictionary<T, IReadOnlyCollection<Implicant<T>>> SortImplicantsByIdentifiers(
            [NotNull] IReadOnlyList<Implicant<T>> primeImplicants)
        {
            Check.NotEmpty(primeImplicants, nameof(primeImplicants));

            var tmpSortedImplicantLists = new Dictionary<T, List<Implicant<T>>>();

            foreach (var primeImplicant in primeImplicants)
            foreach (var identifier in primeImplicant.Identifiers)
            {
                if (!tmpSortedImplicantLists.ContainsKey(identifier))
                    tmpSortedImplicantLists[identifier] = new List<Implicant<T>>();

                tmpSortedImplicantLists[identifier].Add(primeImplicant);
            }

            var sortedImplicantLists = tmpSortedImplicantLists
                .ToDictionary(list => list.Key, list => (IReadOnlyCollection<Implicant<T>>) list.Value.AsReadOnly())
                .AsReadOnly();

            return sortedImplicantLists;
        }

        [NotNull]
        private static IReadOnlyCollection<IReadOnlyCollection<Implicant<T>>> ExpandSortedImplicants(
            [NotNull] IReadOnlyDictionary<T, IReadOnlyCollection<Implicant<T>>> sortedImplicantLists)
        {
            Check.NotEmpty(sortedImplicantLists, nameof(sortedImplicantLists));

            var expr = sortedImplicantLists.Values
                .Select(list => list.Select(implicant => new List<Implicant<T>> {implicant}).ToList())
                .ToList();

            while (expr.Count >= 2)
            {
                var leftExpr = expr[0];
                expr.Remove(leftExpr);

                var rightExpr = expr[0];
                expr.Remove(rightExpr);

                var sumOfProducts = new List<List<Implicant<T>>>();
                foreach (var leftImplicantList in leftExpr)
                foreach (var rightImplicantList in rightExpr)
                {
                    var products = new List<Implicant<T>>();
                    products.AddRange(leftImplicantList);
                    products.AddRange(rightImplicantList);

                    sumOfProducts.Add(products);
                }

                expr.Insert(0, sumOfProducts);
            }

            var expandedImplicantLists = expr[0]
                .Select(list => list.AsReadOnly())
                .ToList()
                .AsReadOnly();

            return expandedImplicantLists;
        }

        [NotNull]
        private static IReadOnlyCollection<Implicant<T>> SimplifyExpandedImplicants(
            [NotNull] IReadOnlyCollection<IReadOnlyCollection<Implicant<T>>> expandedImplicantLists)
        {
            Check.NotEmpty(expandedImplicantLists, nameof(expandedImplicantLists));

            var simplifiedImplicants = expandedImplicantLists.First();

            foreach (var expandedImplicantList in expandedImplicantLists)
            {
                var tmpSimplifiedImplicants = new List<Implicant<T>>();

                foreach (var expandedImplicant in expandedImplicantList)
                    if (!tmpSimplifiedImplicants.Contains(expandedImplicant))
                        tmpSimplifiedImplicants.Add(expandedImplicant);

                if (simplifiedImplicants.Count > tmpSimplifiedImplicants.Count)
                    simplifiedImplicants = tmpSimplifiedImplicants;
            }

            return simplifiedImplicants;
        }
    }
}