using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public static class QuineMcCluskey<T>
    {
        [NotNull]
        public static IReadOnlyCollection<Implicant<T>> Minimize([NotNull] IReadOnlyList<Implicant<T>> implicants)
        {
            Check.NotEmpty(implicants, nameof(implicants));

            var primeImplicants = implicants;
            while (true)
            {
                var sortedImplicants = SortImplicantsByTrueElements(primeImplicants);

                var handledImplicants = sortedImplicants
                    .SelectMany(sortedList => sortedList.Value)
                    .ToDictionary(m => m, m => false);

                var newImplicants = new List<Implicant<T>>();
                for (var i = 0; i < sortedImplicants.Count - 1; ++i)
                for (var j = 0; j < sortedImplicants[i].Count; ++j)
                for (var k = 0; k < sortedImplicants[i + 1].Count; ++k)
                {
                    var implicant1 = sortedImplicants[i][j];
                    var implicant2 = sortedImplicants[i + 1][k];

                    if (GetDifferentElementCount(implicant1.Values, implicant2.Values) == 1)
                    {
                        handledImplicants[implicant1] = true;
                        handledImplicants[implicant2] = true;

                        var newDigits = implicant1.Values.ToList();
                        newDigits[GetOneDifferentElementIndex(implicant1.Values, implicant2.Values)] = null;

                        var isNewImplicant =
                            newImplicants.Any(newImplicant => newImplicant.Values.SequenceEqual(newDigits));

                        if (!isNewImplicant)
                        {
                            var newIdentifiers = new List<T>();
                            newIdentifiers.AddRange(implicant1.Identifiers);
                            newIdentifiers.AddRange(implicant2.Identifiers);

                            var newImplicant = new Implicant<T>(newIdentifiers, newDigits);
                            newImplicants.Add(newImplicant);
                        }
                    }
                }

                var areAllImplicantsHandled = true;

                foreach (var handledImplicant in handledImplicants)
                    if (!handledImplicant.Value)
                        newImplicants.Add(handledImplicant.Key);
                    else areAllImplicantsHandled = false;

                primeImplicants = newImplicants;

                if (areAllImplicantsHandled) break;
            }

            return PetrickMethod<T>.FindEssentialPrimeImplicants(primeImplicants);
        }

        private static IReadOnlyDictionary<int, IReadOnlyList<Implicant<T>>> SortImplicantsByTrueElements(
            [NotNull] IReadOnlyList<Implicant<T>> implicants)
        {
            Check.NotEmpty(implicants, nameof(implicants));

            var tmpSortedImplicants = new Dictionary<int, List<Implicant<T>>>();
            for (var i = 0; i <= implicants[0].Values.Count; ++i)
                tmpSortedImplicants[i] = new List<Implicant<T>>();

            foreach (var implicant in implicants)
                tmpSortedImplicants[GetTrueElementCount(implicant.Values)].Add(implicant);

            var sortedImplicants = tmpSortedImplicants
                .ToDictionary(i => i.Key, i => (IReadOnlyList<Implicant<T>>) i.Value);

            return sortedImplicants;
        }

        private static int GetDifferentElementCount([NotNull] IReadOnlyList<bool?> list1,
            [NotNull] IReadOnlyList<bool?> list2)
        {
            Check.NotEmpty(list1, nameof(list1));
            Check.NotEmpty(list2, nameof(list2));

            if (list1.Count != list2.Count)
                throw new ArgumentException();

            return list1.Where((e, i) => e != list2[i]).Count();
        }

        private static int GetOneDifferentElementIndex([NotNull] IReadOnlyList<bool?> list1,
            [NotNull] IReadOnlyList<bool?> list2)
        {
            Check.NotEmpty(list1, nameof(list1));
            Check.NotEmpty(list2, nameof(list2));

            if (list1.Count != list2.Count)
                throw new ArgumentException();

            var index = -1;
            var hasOneDifferentElement = false;
            for (var i = 0; i < list1.Count; i++)
                if (list1[i] != list2[i])
                {
                    if (hasOneDifferentElement)
                        throw new ArgumentException();

                    index = i;
                    hasOneDifferentElement = true;
                }

            if (!hasOneDifferentElement) throw new ArgumentException();

            return index;
        }

        private static int GetTrueElementCount([NotNull] IReadOnlyCollection<bool?> list)
        {
            Check.NotEmpty(list, nameof(list));

            return list.Count(e => e == true);
        }
    }
}