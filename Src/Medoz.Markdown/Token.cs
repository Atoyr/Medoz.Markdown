using System;
using System.Text;

namespace Medoz.Markdown;

public class Token 
{
    public int Id { get; set; }
    public Token? Parent { get; set; }
    public ElementType ElementType { get; set; }
    public string Content { get; set; }
    public IEnumerable<Attribute> Attributes { get; private set; } = new List<Attribute>();

    public Token AddAttribute(string key, string value)
    {
        (Attributes as List<Attribute>).Add(new Attribute(){ Key = key, Value = value });
        return this;
    }

    private string AttributeString()
    {
        StringBuilder sb = new();
        foreach(var a in Attributes)
        {
            sb.Append($" {a.Key}=\"{a.Value}\"");
        }
        return sb.ToString();
    }

    // public override string ToString() => $"{Id} : {ElementType.GetTypeName()} : {Content}";
    public override string ToString()
    {
        switch(ElementType)
        {
            case ElementType.Root:
                return Content;
            case ElementType.Paragraph:
                return $"<p {AttributeString()}>{Content}</p>";
            case ElementType.H1:
                return $"<h1 {AttributeString()}>{Content}</h1>";
            case ElementType.H2:
                return $"<h2 {AttributeString()}>{Content}</h2>";
            case ElementType.H3:
                return $"<h3 {AttributeString()}>{Content}</h3>";
            case ElementType.H4:
                return $"<h4 {AttributeString()}>{Content}</h4>";
            case ElementType.H5:
                return $"<h5 {AttributeString()}>{Content}</h5>";
            case ElementType.H6:
                return $"<h6 {AttributeString()}>{Content}</h6>";
            case ElementType.Text:
                return Content;
            case ElementType.Strong:
                return $"<strong {AttributeString()}>{Content}</strong>";
            case ElementType.Italic:
                return $"<italic {AttributeString()}>{Content}</italic>";
            case ElementType.Strike:
                return $"<strike {AttributeString()}>{Content}</strike>";
            case ElementType.Link:
                return "Link";
            case ElementType.Image:
                return "Image";
            case ElementType.Ul:
                return $"<ul {AttributeString()}>{Content}</ul>";
            case ElementType.Li:
                return $"<li {AttributeString()}>{Content}</li>";
            case ElementType.Ol:
                return $"<ol {AttributeString()}>{Content}</ol>";
            case ElementType.Table:
                return $"<table {AttributeString()}>{Content}</table>";
            case ElementType.THead:
                return $"<thead {AttributeString()}>{Content}</thead>";
            case ElementType.TBody:
                return $"<tbody {AttributeString()}>{Content}</tbody>";
            case ElementType.TR:
                return $"<tr {AttributeString()}>{Content}</tr>";
            case ElementType.TH:
                return $"<th {AttributeString()}>{Content}</th>";
            case ElementType.TD:
                return $"<td {AttributeString()}>{Content}</td>";
            case ElementType.Code:
                return $"<code {AttributeString()}>{Content}</code>";
            case ElementType.BlockQuote:
                return "BlockQuote";
            case ElementType.NewLine:
                return "<br />";
            default:
                return Content;
        }
    }
}
