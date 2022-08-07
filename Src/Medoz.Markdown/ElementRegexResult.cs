using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Medoz.Markdown;

public class ElementRegexResult
{
    public ElementType ElementType { get; set; }
    public Match Match { get; set; }

    public bool IsMatch
    {
        get => Match is not null && Match.Success;
    }

    private ElementRegexResult() { }

    public ElementRegexResult(ElementType type, Match match) 
    {
        ElementType = type;
        Match = match;
    }
}
