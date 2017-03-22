using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using RiceDoctor.Shared;
using static RiceDoctor.ConversationalAgent.ChatTokenType;
using static RiceDoctor.Shared.TokenType;

namespace RiceDoctor.ConversationalAgent
{
    public class ChatParser : Parser<IReadOnlyCollection<Question>>
    {
        [CanBeNull] private ReadOnlyCollection<Question> _questions;

        public ChatParser([NotNull] ChatLexer lexer) : base(lexer)
        {
        }

        [NotNull]
        public override IReadOnlyCollection<Question> Parse()
        {
            if (_questions != null) return _questions;

            _questions = ParseQuestionGroups();

            if (CurrentToken.Type != Eof)
                throw new InvalidOperationException(CoreStrings.SyntaxError(Eof.Name, CurrentToken.Type.Name));

            return _questions;
        }

        [NotNull]
        private ReadOnlyCollection<Question> ParseQuestionGroups()
        {
            var questionGroups = new Dictionary<string, Question>();

            while (CurrentToken.Type != Eof)
            {
                while (CurrentToken.Type == NewLine)
                    Eat(NewLine);

                var questionGroup = ParseQuestionGroup();

                foreach (var question in questionGroup)
                    questionGroups.Add(question.Key, question.Value);
            }

            var questions = questionGroups
                .Select(q => q.Value)
                .OrderByDescending(q => q.Weight)
                .ToList()
                .AsReadOnly();

            return questions;
        }

        [NotNull]
        private ReadOnlyDictionary<string, Question> ParseQuestionGroup()
        {
            var questionPatterns = new Dictionary<string, Tuple<MessageNode, int>>();
            do
            {
                Eat(Plus);

                var weight = 1;
                var textContainer = new MessageNode();

                while (CurrentToken.Type != NewLine)
                {
                    if (CurrentToken.Type == Star)
                    {
                        var wildcard = ParseWildcard();
                        textContainer.Append(wildcard);

                        continue;
                    }
                    if (CurrentToken.Type == Word)
                    {
                        var text = ParseText();
                        textContainer.Append(text);

                        continue;
                    }
                    if (CurrentToken.Type == LParen)
                    {
                        var alternative = ParseAlternative();
                        textContainer.Append(alternative);

                        continue;
                    }
                    if (CurrentToken.Type == LSquare)
                    {
                        if (Peek().Type == Word)
                        {
                            var option = ParseOption();
                            textContainer.Append(option);
                        }
                        else
                        {
                            var optionWildcard = ParseOptionWildcard();
                            textContainer.Append(optionWildcard);
                        }

                        continue;
                    }

                    weight = ParseWeight();

                    break;
                }

                Eat(NewLine);

                questionPatterns.Add(textContainer.ToString(), new Tuple<MessageNode, int>(textContainer, weight));
            } while (CurrentToken.Type == Plus);

            var anwserPatterns = new List<KeyValuePair<TextNode, int>>();
            do
            {
                Eat(Hyphen);

                var weight = 1;

                var text = ParseText();

                if (CurrentToken.Type == LBrace)
                    weight = ParseWeight();

                if (CurrentToken.Type == NewLine)
                    Eat(NewLine);

                anwserPatterns.Add(new KeyValuePair<TextNode, int>(text, weight));
            } while (CurrentToken.Type == Hyphen);

            var answers = anwserPatterns
                .Select(pattern => new Answer(pattern.Value, pattern.Key.ToString()))
                .OrderByDescending(a => a.Weight)
                .ToList();

            var questions = questionPatterns
                .Select(pattern => new Question(pattern.Value.Item2, pattern.Key, answers))
                .ToDictionary(q => q.Pattern, q => q)
                .AsReadOnly();

            return questions;
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
        private OptionWildcardNode ParseOptionWildcard()
        {
            Eat(LSquare);

            Eat(Star);

            Eat(RSquare);

            var optionWildcard = new OptionWildcardNode();

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
        private ReadOnlyCollection<TextNode> ParseVertiTexts()
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

            return texts.AsReadOnly();
        }

        [NotNull]
        private string ParseWord()
        {
            var word = CurrentToken.Value;

            Eat(Word);

            return (string) word;
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