using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using JetBrains.Annotations;

namespace RiceDoctor.Shared
{
    [DebuggerStepThrough]
    public static class DictionaryExtensions
    {
        [NotNull]
        public static ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            return new ReadOnlyDictionary<TKey, TValue>(dictionary);
        }
    }
}