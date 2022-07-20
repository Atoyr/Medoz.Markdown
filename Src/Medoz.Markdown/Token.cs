using System;

namespace Medoz.Markdown;

public class Token 
{
    public int Id { get; set; }
    public Token Parent { get; set; }
    public string ElementType { get; set; }
    public string Content { get; set; }
}
