using System;
using System.Text.RegularExpressions;

namespace Medoz.Markdown;

public class Lexer
{
    protected int Id { get; private set; } = 0;
    protected LexerStatus Status { get; private set; }
    protected enum LexerStatus
    {
        Neutral,
        UnorderedList,
        OrderedList,
        Table,
        Code
    }

    protected Token GenerateRootToken()
    {
        Id = 0;
        return new Token()
        {
            Id = 0,
            ElementType = ElementType.Root,
            Parent = null,
            Content = string.Empty
        };
    }

    public IEnumerable<Token> Analize(string markdownText)
    {
        string[] mdArray = markdownText.Split(new string[]{ "\r\n", "\r", "\n"}, StringSplitOptions.None);
        List<Token> elements = new();
        Token parent = GenerateRootToken();
        elements.Add(parent);
        Status = LexerStatus.Neutral;
        LexerStatus nextStatus = LexerStatus.Neutral;

        foreach(string mdRow in mdArray)
        {
            IEnumerable<Token> retElements = default;

            switch(Status)
            {
                case LexerStatus.Neutral:
                    if(Regex.Match(mdRow, ElementRegex.UnorderedListRegex.Pattern).Success)
                    {
                        retElements = TokenizeUnorderedList(parent, mdRow);
                    }
                    else if (Regex.Match(mdRow, ElementRegex.OrderedListRegex.Pattern).Success)
                    {
                        retElements = TokenizeOrderedList(parent, mdRow);
                    }
                    else
                    {
                        retElements = Tokenize(parent, mdRow);
                    }
                    break;
                case LexerStatus.UnorderedList:
                    if(Regex.Match(mdRow, ElementRegex.UnorderedListRegex.Pattern).Success)
                    {
                        retElements = TokenizeUnorderedList(elements.Last().Parent.Parent, mdRow);
                    }
                    else if (Regex.Match(mdRow, ElementRegex.OrderedListRegex.Pattern).Success)
                    {
                        retElements = TokenizeOrderedList(parent, mdRow);
                    }
                    else
                    {
                        Status = LexerStatus.Neutral;
                        retElements = Tokenize(parent, mdRow);
                    }
                    break;
                case LexerStatus.OrderedList:
                    if(Regex.Match(mdRow, ElementRegex.UnorderedListRegex.Pattern).Success)
                    {
                        retElements = TokenizeUnorderedList(parent, mdRow);
                    }
                    else if (Regex.Match(mdRow, ElementRegex.OrderedListRegex.Pattern).Success)
                    {
                        retElements = TokenizeOrderedList(parent, mdRow);
                    }
                    else
                    {
                        retElements = Tokenize(parent, mdRow);
                    }
                    break;
                case LexerStatus.Table:
                    // TODO
                    retElements = Tokenize(parent, mdRow);
                    break;
                case LexerStatus.Code:
                    // TODO
                    retElements = Tokenize(parent, mdRow);
                    break;
                default:
                    // TODO
                    retElements = Tokenize(parent, mdRow);
                    break;
            }

            Console.WriteLine("return tree");
            foreach(var t in retElements)
            {
                Console.WriteLine($"{t.Id} |{t.ToString()}");
            }
            elements.AddRange(retElements);
        }
        return elements;
    }

    protected IEnumerable<Token> Tokenize(Token parent, string text)
    {
        Id++;
        List<Token> elements = new();

        // 親がrootの場合
        if (parent.ElementType == ElementType.Root)
        {
            IEnumerable<ElementRegexResult> blockResult = 
                ElementRegex.BlockElementRegexs
                .Select(x => new ElementRegexResult(x.ElementType, Regex.Match(text, x.Pattern)))
                .Where(x => x.IsMatch);
            if (blockResult.Any())
            {
                // block
                ElementRegexResult er = blockResult.First();
                Token newParent = new()
                {
                   Id = Id,
                   ElementType = er.ElementType,
                   Parent = parent,
                   Content = string.Empty
                };
                elements.Add(newParent);
                elements.AddRange(Tokenize(newParent, er.Match.Groups[1].Value));
            }
            else
            {
                // paragraph
                Token newParent = new()
                {
                   Id = Id,
                   ElementType = ElementType.Paragraph,
                   Parent = parent,
                   Content = string.Empty
                };
                elements.Add(newParent);
                elements.AddRange(Tokenize(newParent, text));
            }
        }
        else
        {
            // inline Parse
            IEnumerable<ElementRegexResult> inlineResults =
                ElementRegex.InlineElementRegexs
                .Select(x => new ElementRegexResult(x.ElementType, Regex.Match(text, x.Pattern)))
                .Where(x => x.IsMatch)
                .OrderBy(x => x.Match.Index);
            if (inlineResults.Any())
            {
                // inline
                ElementRegexResult er = inlineResults.First();
                if ( er.Match.Index > 0 )
                {
                    Token token = new()
                    {
                        Id = Id,
                        ElementType = ElementType.Text,
                        Parent = parent,
                        Content = text.Substring(0, er.Match.Index - 1)
                    };
                    elements.Add(token);
                    text = text.Substring(er.Match.Index);
                    
                    Id++;
                }

                Token newParent = new()
                {
                    Id = Id, 
                    ElementType = er.ElementType,
                    Parent = parent,
                    Content = string.Empty
                };
                elements.Add(newParent);
                elements.AddRange(Tokenize(newParent, er.Match.Groups[1].Value));
                text = text.Substring(er.Match.Length);
                if ( text.Count() > 0 )
                {
                    elements.AddRange(Tokenize(parent, text));
                }
            }
            else
            {
                Token token = new()
                {
                    Id = Id,
                    ElementType = ElementType.Text,
                    Parent = parent,
                    Content = text
                };
                elements.Add(token);
            }
        }
        return elements;
    }

    protected IEnumerable<Token> TokenizeUnorderedList(Token parent, string text)
    {
        List<Token> elements = new();
        // 親がrootまたはUlではない場合は処理しない
        if (parent.ElementType != ElementType.Root && parent.ElementType != ElementType.Ul)
        {
            return elements;
        }

        ElementRegexResult result = new ElementRegexResult(ElementRegex.UnorderedListRegex.ElementType, Regex.Match(text, ElementRegex.UnorderedListRegex.Pattern));
        if (!result.IsMatch)
        {
            return elements;
        }

        Id++;

        if (Status == LexerStatus.Neutral)
        {
            Token ulToken = new()
            {
                Id = Id,
                ElementType = ElementType.Ul,
                Parent = parent,
                Content = string.Empty,
                Attribute = new(){ Key = "space", Value = result.Match.Groups[1].Value }
            };
            elements.Add(ulToken);
            Id++;
            Token liToken = new()
            {
                Id = Id,
                ElementType = ElementType.Li,
                Parent = ulToken,
                Content = string.Empty,
            };
            elements.Add(liToken);
            elements.AddRange(Tokenize(liToken, result.Match.Groups[3].Value));
        }
        else
        {
            if(parent.Attribute is not null && parent.Attribute.Key == "space")
            {
                Id++;
                Token liToken = new()
                {
                    Id = Id,
                    ElementType = ElementType.Li,
                    Content = string.Empty,
                };
                switch(parent.Attribute.Value.Count().CompareTo(result.Match.Groups[1].Value.Count()))
                {
                    case 0 :
                        liToken.Parent = parent;
                        elements.Add(liToken);
                        elements.AddRange(Tokenize(liToken, result.Match.Groups[3].Value));
                        break;
                    case 1 :
                        if(parent.Parent.ElementType != ElementType.Ul)
                        {
                            liToken.Parent = parent;
                            elements.Add(liToken);
                            elements.AddRange(Tokenize(liToken, result.Match.Groups[3].Value));
                        }
                        else
                        {
                            liToken.Parent = parent.Parent;
                            elements.Add(liToken);
                            elements.AddRange(Tokenize(liToken, result.Match.Groups[3].Value));
                        }
                        break;
                    case -1:
                        Token ulToken = new()
                        {
                            Id = Id,
                               ElementType = ElementType.Ul,
                               Parent = parent,
                               Content = string.Empty,
                               Attribute = new(){ Key = "space", Value = result.Match.Groups[1].Value }
                        };
                        elements.Add(ulToken);
                        Id++;
                        liToken.Parent = ulToken;
                        elements.Add(liToken);
                        elements.AddRange(Tokenize(liToken, result.Match.Groups[3].Value));
                        break;
                }
            }
            else
            {
                Id++;
                Token liToken = new()
                {
                    Id = Id,
                       ElementType = ElementType.Li,
                       Parent = parent,
                       Content = string.Empty,
                };
                elements.Add(liToken);
                elements.AddRange(Tokenize(liToken, result.Match.Groups[2].Value));
            }
        }
        Status = LexerStatus.UnorderedList;
        return elements;
    }

    protected IEnumerable<Token> TokenizeOrderedList(Token parent, string text)
    {
        List<Token> elements = new();
        // 親がrootではない場合
        if (parent.ElementType != ElementType.Root)
        {
            return elements;
        }

        ElementRegexResult result = new ElementRegexResult(ElementRegex.OrderedListRegex.ElementType, Regex.Match(text, ElementRegex.OrderedListRegex.Pattern));
        if (!result.IsMatch)
        {
            return elements;
        }

        return elements;
    }
}

