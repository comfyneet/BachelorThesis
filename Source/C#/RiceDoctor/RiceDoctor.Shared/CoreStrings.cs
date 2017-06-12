using JetBrains.Annotations;

namespace RiceDoctor.Shared
{
    public static class CoreStrings
    {
        [NotNull]
        public static string ArgumentIsEmpty([NotNull] string argumentName)
        {
            Check.NotEmpty(argumentName, nameof(argumentName));

            return $"The string argument '{argumentName}' cannot be empty.";
        }

        [NotNull]
        public static string ArgumentPropertyNull([NotNull] string propertyName, [NotNull] string argumentName)
        {
            Check.NotEmpty(propertyName, nameof(propertyName));
            Check.NotEmpty(argumentName, nameof(argumentName));

            return $"The property '{propertyName}' of the argument '{argumentName}' cannot be null.";
        }

        [NotNull]
        public static string CollectionArgumentIsEmpty([NotNull] string argumentName)
        {
            Check.NotEmpty(argumentName, nameof(argumentName));

            return $"The collection argument '{argumentName}' must contain at least one element.";
        }

        [NotNull]
        public static string InvalidArgumentType(
            [NotNull] string argumentName,
            [NotNull] string argumentTypeName,
            [NotNull] string expectedTypeName)
        {
            Check.NotEmpty(argumentName, nameof(argumentName));
            Check.NotEmpty(argumentTypeName, nameof(argumentTypeName));
            Check.NotEmpty(expectedTypeName, nameof(expectedTypeName));

            return
                $"The argument '{argumentName}' belongs to the type '{argumentTypeName}' but the expected type was '{expectedTypeName}'.";
        }

        [NotNull]
        public static string CannotSetAgain([NotNull] string valueName)
        {
            Check.NotEmpty(valueName, nameof(valueName));

            return $"The value for '{valueName}' has already been set.";
        }

        [NotNull]
        public static string SyntaxError([NotNull] string expectedValue, [NotNull] string foundValue)
        {
            Check.NotEmpty(expectedValue, nameof(expectedValue));
            Check.NotEmpty(foundValue, nameof(foundValue));

            return $"Syntax error, '{expectedValue}' expected but '{foundValue}' found.";
        }

        [NotNull]
        public static string MalformedArgument([NotNull] string argumentName)
        {
            Check.NotEmpty(argumentName, nameof(argumentName));

            return $"Argument '{argumentName}' cannot have malformed value.";
        }

        [NotNull]
        public static string NonexistentType([NotNull] string type)
        {
            Check.NotEmpty(type, nameof(type));

            return $"Type '{type}' does not exist.";
        }
    }
}