﻿using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.SemanticCodeInterpreter
{
    public class IndividualNode : TextContainerNode
    {
        public IndividualNode([NotNull] string className, [NotNull] string individualName) : base("a")
        {
            Check.NotEmpty(className, nameof(className));
            Check.NotEmpty(individualName, nameof(individualName));

            Class = className;
            Individual = individualName;
            AddAttribute("href", $"{SemanticParser.IndividualLink}{Individual}");

            var icon = new TextContainerNode("span");
            icon.AddAttribute("class", "glyphicon glyphicon-info-sign");
            Prepend(new TextNode("&nbsp;"));
            Prepend(icon);
        }

        [NotNull]
        public string Class { get; }

        [NotNull]
        public string Individual { get; }

        [NotNull]
        public string Href => Attributes["href"];
    }
}