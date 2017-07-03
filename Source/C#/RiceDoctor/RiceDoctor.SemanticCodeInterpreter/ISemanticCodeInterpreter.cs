using JetBrains.Annotations;

namespace RiceDoctor.SemanticCodeInterpreter
{
    public interface ISemanticCodeInterpreter
    {
        [NotNull]
        string Parse([NotNull] string text);
    }
}