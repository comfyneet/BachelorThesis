using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using RiceDoctor.Shared;
using static RiceDoctor.SemanticCode.SemanticTokenType;
using static RiceDoctor.Shared.TokenType;

namespace RiceDoctor.SemanticCode
{
    public class SemanticParser : Parser<TextContainerNode>
    {
        [CanBeNull] private TextContainerNode _root;

        static SemanticParser()
        {
            ColorTags = new List<string>
            {
                "red",
                "green",
                "blue",
                "navy",
                "fuchsia",
                "orange",
                "yellow",
                "gray",
                "purple"
            };

            TextContainerTags = new Dictionary<string, string>
            {
                {"b", "strong"},
                {"i", "em"},
                {"u", "u"},
                {"sub", "sub"},
                {"sup", "sup"},
                {"quote", "blockquote"},
                {"code", "code"},
                {"h1", "h1"},
                {"h2", "h2"},
                {"h3", "h3"},
                {"h4", "h4"},
                {"h5", "h5"},
                {"center", "center"}
            };

            SingularTags = new List<string>
            {
                "hr",
                "br"
            };

            ListTags = new List<string>
            {
                "ul",
                "ol"
            };

            OrderedListTypeTags = new List<string>
            {
                "1",
                "A",
                "a",
                "I",
                "i"
            };

            UnorderedListTypeTags = new List<string>
            {
                "disc",
                "circle",
                "square",
                "none"
            };
        }

        public SemanticParser([NotNull] SemanticLexer lexer) : base(lexer)
        {
        }

        public static string ClassLink { get; [param: NotNull] set; } = "/Ontology/Class?className=";

        public static string IndividualLink { get; [param: NotNull] set; } = "/Ontology/Individual?individualName=";

        public static string StaticImageLink { get; [param: NotNull] set; } = "/images/";

        [NotNull]
        private static IReadOnlyCollection<string> ColorTags { get; }

        [NotNull]
        private static IReadOnlyDictionary<string, string> TextContainerTags { get; }

        [NotNull]
        private static IReadOnlyCollection<string> SingularTags { get; }

        [NotNull]
        private static IReadOnlyCollection<string> ListTags { get; }

        [NotNull]
        private static IReadOnlyCollection<string> OrderedListTypeTags { get; }

        [NotNull]
        private static IReadOnlyCollection<string> UnorderedListTypeTags { get; }

        [NotNull]
        public static string Parse([NotNull] string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            var lexer = new SemanticLexer(text);
            var parser = new SemanticParser(lexer);

            return parser.Parse().ToString();
        }

        [NotNull]
        public override TextContainerNode Parse()
        {
            if (_root != null) return _root;

            _root = new TextContainerNode("div");
            _root.AddAttribute("style", "white-space: pre-wrap;");

            while (CurrentToken.Type != Eof)
            {
                var node = ParseNode();

                _root.Append(node);
            }

            return _root;
        }

        [NotNull]
        private Node<SemanticNode> ParseOtherSquaredNode()
        {
            Eat(LSquare);

            var ident = ParseIdentifier();

            Node<SemanticNode> node;
            var singularNode = false;

            if (ColorTags.Contains(ident))
            {
                node = new ColorNode(ident);
            }
            else if (TextContainerTags.TryGetValue(ident, out var code))
            {
                node = new TextContainerNode(code);
            }
            else if (SingularTags.Contains(ident))
            {
                singularNode = true;
                node = new SingularNode(ident);
            }
            else if (ident == "img" || ident == "simg" || ident == "url")
            {
                Eat(Equal);

                var url = CurrentToken.Value;

                Eat(UnquotedString);

                if (ident == "img") node = new ImageNode((string) url, ImageType.Online);
                else if (ident == "simg") node = new ImageNode((string) url, ImageType.Static);
                else node = new LinkNode((string) url);
            }
            else // Parse ontology nodes
            {
                if (CurrentToken.Type == Equal)
                {
                    Eat(Equal);

                    var individual = ParseIdentifier();

                    node = new IndividualNode(ident, individual);
                }
                else
                {
                    node = new ClassNode(ident);
                }
            }


            Eat(RSquare);

            if (singularNode) return node;

            while (!(CurrentToken.Type == LSquare
                     && Peek().Type == Slash
                     && Peek(2).Type == Ident
                     && (string) Peek(2).Value == ident))
            {
                var childNode = ParseNode();

                ((SemanticContainerNode) node).Append(childNode);
            }

            Eat(LSquare);

            Eat(Slash);

            Eat(Ident);

            Eat(RSquare);

            return node;
        }

        [NotNull]
        private Node<SemanticNode> ParseNode()
        {
            Node<SemanticNode> node = null;

            if (CurrentToken.Type == Words || CurrentToken.Type == NewLine)
                node = ParseText();
            else if (CurrentToken.Type == LSquare)
                if (Peek().Type == Ident && ListTags.Contains((string) Peek().Value))
                    node = ParseList();
                else node = ParseOtherSquaredNode();

            if (node == null) throw new InvalidOperationException("Invalid node type.");

            return node;
        }

        [NotNull]
        private ListNode ParseList()
        {
            Eat(LSquare);

            var type = ParseIdentifier();

            if (!ListTags.Contains(type)) throw new InvalidOperationException("Invalid list type.");

            string listType = null;
            if (CurrentToken.Type == Equal)
            {
                Eat(Equal);

                listType = CurrentToken.Type == Ident ? ParseIdentifier() : ((int) ParseNumber()).ToString();

                if (!UnorderedListTypeTags.Contains(listType) && !OrderedListTypeTags.Contains(listType))
                    throw new InvalidOperationException("Invalid list item type.");
            }

            Eat(RSquare);

            var list = listType != null ? new ListNode(type, listType) : new ListNode(type);

            while (!(CurrentToken.Type == LSquare
                     && Peek().Type == Slash
                     && Peek(2).Type == Ident
                     && (string) Peek(2).Value == type))
            {
                while (CurrentToken.Type == Words && string.IsNullOrWhiteSpace((string) CurrentToken.Value) ||
                       CurrentToken.Type == NewLine)
                    Eat(CurrentToken.Type);

                while (CurrentToken.Type == LSquare
                       && Peek().Type == Star)
                {
                    var listItem = ParseListItem(type);

                    list.Append(listItem);
                }
            }

            Eat(LSquare);

            Eat(Slash);

            Eat(Ident);

            Eat(RSquare);

            return list;
        }

        [NotNull]
        private ListItemNode ParseListItem(string type)
        {
            Eat(LSquare);

            Eat(Star);

            Eat(RSquare);

            var listItem = new ListItemNode();
            while (!(CurrentToken.Type == LSquare
                     && Peek().Type == Star) &&
                   !(CurrentToken.Type == LSquare
                     && Peek().Type == Slash
                     && Peek(2).Type == Ident
                     && (string) Peek(2).Value == type))
            {
                var item = ParseNode();

                listItem.Append(item);
            }

            return listItem;
        }

        [NotNull]
        private TextNode ParseText()
        {
            var builder = new StringBuilder();

            var text = CurrentToken.Value;

            Eat(CurrentToken.Type == Words ? Words : NewLine);

            builder.Append((string) text);

            while (CurrentToken.Type == Words || CurrentToken.Type == NewLine)
            {
                builder.Append((string) CurrentToken.Value);

                Eat(CurrentToken.Type == Words ? Words : NewLine);
            }

            var textNode = new TextNode(builder.ToString());

            return textNode;
        }
    }
}