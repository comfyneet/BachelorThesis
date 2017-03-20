using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace RiceDoctor.Shared
{
    [DebuggerStepThrough]
    public static class Check
    {
        public static T NotNull<T>([NoEnumeration] T value, [InvokerParameterName] [NotNull] string parameterName)
        {
            if (ReferenceEquals(value, null))
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        public static T NotNull<T>(
            [NoEnumeration] T value,
            [InvokerParameterName] [NotNull] string parameterName,
            [NotNull] string propertyName)
        {
            if (ReferenceEquals(value, null))
            {
                NotEmpty(parameterName, nameof(parameterName));
                NotEmpty(propertyName, nameof(propertyName));

                throw new ArgumentException(CoreStrings.ArgumentPropertyNull(propertyName, parameterName));
            }

            return value;
        }

        public static IReadOnlyCollection<T> NotEmpty<T>(IReadOnlyCollection<T> value,
            [InvokerParameterName] [NotNull] string parameterName)
        {
            NotNull(value, parameterName);

            if (value.Count == 0)
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(CoreStrings.CollectionArgumentIsEmpty(parameterName));
            }

            return value;
        }

        public static string NotEmpty(string value, [InvokerParameterName] [NotNull] string parameterName)
        {
            Exception e = null;
            if (ReferenceEquals(value, null))
                e = new ArgumentNullException(parameterName);
            else if (value.Trim().Length == 0)
                e = new ArgumentException(CoreStrings.ArgumentIsEmpty(parameterName));

            if (e != null)
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw e;
            }

            return value;
        }

        public static T IsType<T>(object value, [InvokerParameterName] [NotNull] string parameterName)
        {
            if (value is T newValue)
                return newValue;

            NotEmpty(parameterName, nameof(parameterName));

            throw new ArgumentException(CoreStrings.InvalidArgumentType(parameterName,
                value.GetType().Name,
                typeof(T).Name));
        }
    }
}