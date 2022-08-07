using System;

namespace Medoz.Markdown;

public class Token 
{
    public int Id { get; set; }
    public Token? Parent { get; set; }
    public ElementType ElementType { get; set; }
    public string Content { get; set; }
    public Attribute? Attribute { get; set; }

    // public override string ToString() => $"{Id} : {ElementType.GetTypeName()} : {Content}";
    public override string ToString()
    {
        switch(ElementType)
        {
            case ElementType.Root:
                return Content;
            case ElementType.Paragraph:
                return $"<p>{Content}</p>";
            case ElementType.H1:
                return $"<h1>{Content}</h1>";
            case ElementType.H2:
                return $"<h2>{Content}</h2>";
            case ElementType.H3:
                return $"<h3>{Content}</h3>";
            case ElementType.H4:
                return $"<h4>{Content}</h4>";
            case ElementType.H5:
                return $"<h5>{Content}</h5>";
            case ElementType.H6:
                return $"<h6>{Content}</h6>";
            case ElementType.Text:
                return Content;
            case ElementType.Strong:
                return $"<strong>{Content}</strong>";
            case ElementType.Italic:
                return $"<italic>{Content}</italic>";
            case ElementType.Strike:
                return $"<strike>{Content}</strike>";
            case ElementType.Link:
                return "Link";
            case ElementType.Image:
                return "Image";
            case ElementType.Ul:
                return $"<ul>{Content}</ul>";
            case ElementType.Li:
                return $"<li>{Content}</li>";
            case ElementType.Ol:
                return $"<ol>{Content}</ol>";
            case ElementType.Table:
                return "Table";
            case ElementType.Code:
                return $"<code>{Content}</code>";
            case ElementType.BlockQuote:
                return "BlockQuote";
            default:
                return Content;
        }
    }
}
