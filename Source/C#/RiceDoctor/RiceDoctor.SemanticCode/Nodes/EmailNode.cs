using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.SemanticCode
{
    public class EmailNode : TextContainerNode
    {
        public EmailNode([NotNull] string email) : base("a")
        {
            Check.NotEmpty(email, nameof(email));

            AddAttribute("href", $"mailto:{email}");
        }

        [NotNull]
        public string Href => Attributes["href"];
    }
}