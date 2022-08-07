using System;

namespace Medoz.Markdown;

public enum ElementType
{
    Root,
    Paragraph,
    H1,
    H2,
    H3,
    H4,
    H5,
    H6,
    Text,
    Strong,
    Italic,
    Strike,
    Link,
    Image,
    Ul,
    Li,
    Ol,
    Table,
    Code,
    BlockQuote,
}

public static class ElementTypeEx
{
    public static string GetTypeName(this ElementType type)
    {
        switch(type)
        {
            case ElementType.Root:
                return "Root";
            case ElementType.Paragraph:
                return "Paragraph";
            case ElementType.H1:
                return "H1";
            case ElementType.H2:
                return "H2";
            case ElementType.H3:
                return "H3";
            case ElementType.H4:
                return "H4";
            case ElementType.H5:
                return "H5";
            case ElementType.H6:
                return "H6";
            case ElementType.Text:
                return "Text";
            case ElementType.Strong:
                return "Strong";
            case ElementType.Italic:
                return "Italic";
            case ElementType.Strike:
                return "Strike";
            case ElementType.Link:
                return "Link";
            case ElementType.Image:
                return "Image";
            case ElementType.Ul:
                return "Ul";
            case ElementType.Li:
                return "Li";
            case ElementType.Ol:
                return "Ol";
            case ElementType.Table:
                return "Table";
            case ElementType.Code:
                return "Code";
            case ElementType.BlockQuote:
                return "BlockQuote";
            default:
                return string.Empty;
        }
    }
}
