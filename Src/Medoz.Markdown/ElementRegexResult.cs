using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Medoz.Markdown;

public class ElementRegexResult
{
    public ElementType ElementType { get; set; }
    public string Content { get; set; }
    public Match? Match { get; private set; }

    private MatchCollection? _matches;
    public MatchCollection? Matches 
    {
        get => _matches;
        set
        {
            _matches = value;
            Match = _matches?.FirstOrDefault();
        }
    }

    public bool IsMatch
    {
        get => Match is not null && Match.Success;
    }

    private ElementRegexResult() { }

    public ElementRegexResult(string content)
    {
        ElementType = ElementType.Text;
        Content = content;
    }

    public ElementRegexResult(ElementType type, string content, MatchCollection? matches)
    {
        ElementType = type;
        Content = content;
        Matches = matches;
    }
}

