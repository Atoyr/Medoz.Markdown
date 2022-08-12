using System;

namespace Medoz.Markdown;

public class ElementRegex
{
    public ElementType ElementType { get; set; }
    public string Pattern { get; set; }

    public static readonly ElementRegex[] InlineElementRegexs = 
    {
        new ElementRegex(){ ElementType = ElementType.Strong, Pattern = @"\*\*(.*)\*\*" },
        new ElementRegex(){ ElementType = ElementType.Italic, Pattern = @"__(.*)__" },
        new ElementRegex(){ ElementType = ElementType.Strike, Pattern = @"~~(.*)~~" },
        new ElementRegex(){ ElementType = ElementType.Link, Pattern = @"\[(.*)\]\((.*)\)" },
        new ElementRegex(){ ElementType = ElementType.Image, Pattern = @"\!\[(.*)\]\((.+)\)" },
        new ElementRegex(){ ElementType = ElementType.Code, Pattern = @"`(.+?)`" }
    };

    public static readonly ElementRegex[] BlockElementRegexs =
    {
        new ElementRegex(){ ElementType = ElementType.H1, Pattern = @"^# (.+)$" },
        new ElementRegex(){ ElementType = ElementType.H2, Pattern = @"^## (.+)$" },
        new ElementRegex(){ ElementType = ElementType.H3, Pattern = @"^### (.+)$" },
        new ElementRegex(){ ElementType = ElementType.H4, Pattern = @"^#### (.+)$" },
        new ElementRegex(){ ElementType = ElementType.H5, Pattern = @"^##### (.+)$" },
        new ElementRegex(){ ElementType = ElementType.H6, Pattern = @"^###### (.+)$" }
    };

    public static readonly ElementRegex UnorderedListRegex = new ElementRegex(){ ElementType = ElementType.Ul, Pattern = @"^( *)([-|\*|\+] (.+))$" };
    public static readonly ElementRegex OrderedListRegex = new ElementRegex(){ ElementType = ElementType.Ol, Pattern = @"^( *)(([0-9]+)\. (.+))$" };
    public static readonly ElementRegex TableHeadBodyRegex = new ElementRegex(){ ElementType = ElementType.Table, Pattern = @"(?=\|(.+?)\|)" };
}


