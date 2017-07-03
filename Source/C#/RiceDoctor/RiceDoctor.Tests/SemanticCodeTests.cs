using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.SemanticCodeInterpreter;
using RiceDoctor.Shared;
using Xunit;

namespace RiceDoctor.Tests
{
    [Collection("Test collection")]
    public class SemanticCodeTests : IClassFixture<SemanticCodeFixture>
    {
        [NotNull] private readonly ISemanticCodeInterpreter _semanticCodeInterpreter;

        public SemanticCodeTests([NotNull] SemanticCodeFixture fixture)
        {
            Check.NotNull(fixture.SemanticCodeInterpreter, nameof(fixture), nameof(fixture.SemanticCodeInterpreter));

            _semanticCodeInterpreter = fixture.SemanticCodeInterpreter;
        }

        public static IEnumerable<string[]> MockData
        {
            get
            {
                yield return new[]
                {
                    @"
[h1]Lightweight Markup Languages[/h1]
According to [b]Wikipedia[/b]:
[quote]
A [url=""http://is.gd/gns""]lightweight markup language[/url]
is a markup language with a simple syntax, designed
to be easy for a human to enter with a simple text
editor, and easy to read in its raw form.
[/quote]
Some examples are:
[ul]
[*]Markdown
[*]Textile
[*]BBCode
[*]Wikipedia
[/ul]
Markup should also extend to [i]code[/i]:
[code]
10 PRINT ""I ROCK AT BASIC!""
20 GOTO 10
[/code]",
                    @"<div style=""white-space: pre-wrap;""><h1>Lightweight Markup Languages</h1>According to <strong>Wikipedia</strong>:<quote>A <a href=""http://is.gd/gns"">lightweight markup language</a>is a markup language with a simple syntax, designed
to be easy for a human to enter with a simple text
editor, and easy to read in its raw form.</quote>Some examples are:<ul><li>Markdown</li><li>Textile</li><li>BBCode</li><li>Wikipedia</li></ul>Markup should also extend to <em>code</em>:<code>
10 PRINT ""I ROCK AT BASIC!""
20 GOTO 10</code></div>"
                };

                yield return new[]
                {
                    @"
Some random [blue]blue[/blue] text Base[sub]64[/sub] for 1[sup]st[/sup] game
Th[[i]s is a simple [b]bold[/b], [i]italic[/i], [u]underline[/u]
[ol]
    [*]This is line 1
    [*]This is line 2
		[ol=a]
            [*]This is [orange]orange[/orange] line 2.1
			[ul=square]
				[*]This is [orange]orange[/orange] line 2.1.1
				[*]This is [green]green[/green] line 2.1.2
				[*]This is [red]red[/red] line 2.1.3
and it continues here
			[/ul]
            [*]This is [green]green[/green] line 2.2
            [*]This is [red]red[/red] line 2.3
and it continues here
        [/ol]
    [*]This is line 3
[/ol]
Another line",
                    @"<div style=""white-space: pre-wrap;"">Some random <font color=""blue"">blue</font> text Base<sub>64</sub> for 1<sup>st</sup> game
Th[i]s is a simple <strong>bold</strong>, <em>italic</em>, <u>underline</u><ol><li>This is line 1 </li><li>This is line 2<ol type=""a""><li>This is <font color=""orange"">orange</font> line 2.1<ul style=""list-style-type:square""><li>This is <font color=""orange"">orange</font> line 2.1.1</li><li>This is <font color=""green"">green</font> line 2.1.2</li><li>This is <font color=""red"">red</font> line 2.1.3
and it continues here</li></ul></li><li>This is <font color=""green"">green</font> line 2.2 </li><li>This is <font color=""red"">red</font> line 2.3
and it continues here </li></ol></li><li>This is line 3</li></ol>Another line</div>"
                };
            }
        }

        [Theory]
        [MemberData(nameof(MockData))]
        public void ParseSemanticCode([NotNull] string semanticCode, [NotNull] string expectedHtml)
        {
            Check.NotEmpty(semanticCode, nameof(semanticCode));
            Check.NotEmpty(expectedHtml, nameof(expectedHtml));

            var actualHtml = _semanticCodeInterpreter.Parse(semanticCode);

            Assert.Equal(expectedHtml, actualHtml);
        }
    }
}