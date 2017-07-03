using RiceDoctor.Shared;

namespace RiceDoctor.SemanticCodeInterpreter
{
    public class Interpreter : ISemanticCodeInterpreter
    {
        public string Parse(string text)
        {
            Check.NotNull(text, nameof(text));

            if (string.IsNullOrWhiteSpace(text)) return "";

            var lexer = new SemanticLexer(text);
            var parser = new SemanticParser(lexer);

            return parser.Parse().ToString();
        }
    }
}