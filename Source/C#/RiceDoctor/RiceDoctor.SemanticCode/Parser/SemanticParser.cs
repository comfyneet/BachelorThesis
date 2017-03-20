using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            var colors = new List<string>
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
            ColorTags = new ReadOnlyCollection<string>(colors);

            var textContainers = new List<string>
            {
                "u",
                "sub",
                "sup",
                "quote",
                "code",
                "h1",
                "h2",
                "h3",
                "h4",
                "h5"
            };
            TextContainerTags = new ReadOnlyCollection<string>(textContainers);

            var singulars = new List<string>
            {
                "hr",
                "br"
            };
            SingularTags = new ReadOnlyCollection<string>(singulars);

            var lists = new List<string>
            {
                "ul",
                "ol"
            };
            ListTags = new ReadOnlyCollection<string>(lists);

            var orderedListType = new List<string>
            {
                "1",
                "A",
                "a",
                "I",
                "i"
            };
            OrderedListTypeTags = new ReadOnlyCollection<string>(orderedListType);

            var unorderedListType = new List<string>
            {
                "disc",
                "circle",
                "square",
                "none"
            };
            UnorderedListTypeTags = new ReadOnlyCollection<string>(unorderedListType);
        }

        public SemanticParser([NotNull] SemanticLexer lexer) : base(lexer)
        {
        }

        public static string OntologyLink { get; [param: NotNull] set; } = "/ontology/";

        [NotNull]
        private static ReadOnlyCollection<string> ColorTags { get; }

        [NotNull]
        private static ReadOnlyCollection<string> TextContainerTags { get; }

        [NotNull]
        private static ReadOnlyCollection<string> SingularTags { get; }

        [NotNull]
        private static ReadOnlyCollection<string> ListTags { get; }

        [NotNull]
        private static ReadOnlyCollection<string> OrderedListTypeTags { get; }

        [NotNull]
        private static ReadOnlyCollection<string> UnorderedListTypeTags { get; }

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
            else if (TextContainerTags.Contains(ident))
            {
                node = new TextContainerNode(ident);
            }
            else if (SingularTags.Contains(ident))
            {
                singularNode = true;
                node = new SingularNode(ident);
            }
            else if (ident == "b")
            {
                node = new TextContainerNode("strong");
            }
            else if (ident == "i")
            {
                node = new TextContainerNode("em");
            }
            else if (ident == "url")
            {
                Eat(Equal);

                var url = CurrentToken.Value;

                Eat(UnquotedString);

                node = new LinkNode((string) url);
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