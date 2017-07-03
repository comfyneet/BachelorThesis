using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using RiceDoctor.Shared;
using static RiceDoctor.QueryAnalysis.QueryTokenType;
using static RiceDoctor.Shared.TokenType;

namespace RiceDoctor.QueryAnalysis
{
    public class QueryParser : Parser<IReadOnlyCollection<Query>>
    {
        [CanBeNull] private IReadOnlyCollection<Query> _queries;

        public QueryParser([NotNull] QueryLexer lexer) : base(lexer)
        {
        }

        [NotNull]
        public override IReadOnlyCollection<Query> Parse()
        {
            if (_queries != null) return _queries;

            _queries = ParseQueryList();

            if (CurrentToken.Type != Eof)
                throw new InvalidOperationException(CoreStrings.SyntaxError(Eof.Name, CurrentToken.Type.Name));

            return _queries;
        }

        [NotNull]
        private IReadOnlyCollection<Query> ParseQueryList()
        {
            var queryList = new List<Query>();

            while (CurrentToken.Type != Eof)
            {
                while (CurrentToken.Type == NewLine)
                    Eat(NewLine);

                queryList.Add(ParseQuery());
            }

            return queryList.OrderByDescending(q => q.Weight).ToList();
        }

        [NotNull]
        private Query ParseQuery()
        {
            Eat(Plus);

            var weight = 60;
            var queryContainer = new QueryContainerNode();
            while (CurrentToken.Type != NewLine)
                if (CurrentToken.Type == Star)
                {
                    var wildcard = ParseWildcard();
                    queryContainer.Append(wildcard);
                }
                else if (CurrentToken.Type == Comma)
                {
                    var comma = ParseComma();
                    queryContainer.Append(comma);
                }
                else if (CurrentToken.Type == Word)
                {
                    var text = ParseText();
                    queryContainer.Append(text);
                }
                else if (CurrentToken.Type == LParen)
                {
                    var alternative = ParseAlternative();
                    queryContainer.Append(alternative);
                }
                else if (CurrentToken.Type == LSquare)
                {
                    if (Peek().Type == Word)
                    {
                        var option = ParseOption();
                        queryContainer.Append(option);
                    }
                    else
                    {
                        var discard = ParseDiscard();
                        queryContainer.Append(discard);
                    }
                }
                else
                {
                    if (CurrentToken.Type == LBrace) weight = ParseWeight();

                    break;
                }

            Eat(NewLine);

            return new Query(weight, queryContainer);
        }

        private int ParseWeight()
        {
            Eat(LBrace);

            var weight = CurrentToken.Value;

            Eat(Word);

            Eat(RBrace);

            return int.Parse((string) weight);
        }

        [NotNull]
        private WildcardNode ParseWildcard()
        {
            Eat(Star);

            var wildcard = new WildcardNode();

            return wildcard;
        }

        [NotNull]
        private DiscardNode ParseDiscard()
        {
            Eat(LSquare);

            Eat(Star);

            Eat(RSquare);

            var optionWildcard = new DiscardNode();

            return optionWildcard;
        }

        [NotNull]
        private AlternativeNode ParseAlternative()
        {
            Eat(LParen);

            var texts = ParseVertiTexts();

            Eat(RParen);

            var alternative = new AlternativeNode();
            foreach (var text in texts)
                alternative.Append(text);

            return alternative;
        }

        [NotNull]
        private OptionNode ParseOption()
        {
            Eat(LSquare);

            var texts = ParseVertiTexts();

            Eat(RSquare);

            var option = new OptionNode();
            foreach (var text in texts)
                option.Append(text);

            return option;
        }

        [NotNull]
        private IReadOnlyCollection<TextNode> ParseVertiTexts()
        {
            var texts = new List<TextNode>();

            var text = ParseText();
            texts.Add(text);

            while (CurrentToken.Type == VertiBar)
            {
                Eat(VertiBar);

                text = ParseText();
                texts.Add(text);
            }

            return texts;
        }

        [NotNull]
        private string ParseWord()
        {
            var word = CurrentToken.Value;

            Eat(Word);

            return (string) word;
        }

        [NotNull]
        private CommaNode ParseComma()
        {
            Eat(Comma);

            return new CommaNode();
        }

        private TextNode ParseText()
        {
            var builder = new StringBuilder();

            var word = ParseWord();
            builder.Append(word);

            while (CurrentToken.Type == Word)
            {
                word = ParseWord();
                builder.Append(' ' + word);
            }

            var text = new TextNode(builder.ToString());

            return text;
        }
    }
}