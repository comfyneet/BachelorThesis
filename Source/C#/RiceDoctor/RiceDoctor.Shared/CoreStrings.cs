using JetBrains.Annotations;

namespace RiceDoctor.Shared
{
    public static class CoreStrings
    {
        [NotNull]
        public static string ArgumentIsEmpty([CanBeNull] string argumentName)
        {
            return $"The string argument '{argumentName}' cannot be empty.";
        }

        [NotNull]
        public static string ArgumentPropertyNull([CanBeNull] string propertyName, [CanBeNull] string argumentName)
        {
            return $"The property '{propertyName}' of the argument '{argumentName}' cannot be null.";
        }

        [NotNull]
        public static string CollectionArgumentIsEmpty([CanBeNull] string argumentName)
        {
            return $"The collection argument '{argumentName}' must contain at least one element.";
        }

        [NotNull]
        public static string InvalidArgumentType([CanBeNull] string argumentName, [CanBeNull] string argumentTypeName,
            [CanBeNull] string expectedTypeName)
        {
            return
                $"The argument '{argumentName}' belongs to the type '{argumentTypeName}' but the expected type was '{expectedTypeName}'.";
        }

        [NotNull]
        public static string CannotSetAgain([CanBeNull] string valueName)
        {
            return $"The value for '{valueName}' has already been set.";
        }

        [NotNull]
        public static string SyntaxError([CanBeNull] string expectedValue, [CanBeNull] string foundValue)
        {
            return $"Syntax error, \"{expectedValue}\" expected but \"{foundValue}\" found";
        }
    }
}