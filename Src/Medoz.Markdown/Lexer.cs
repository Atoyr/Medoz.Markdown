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

    protected Token GenerateBrToken(Token? parent)
    {
        return new Token()
        {
            Id = this.Id,
            ElementType = ElementType.NewLine,
            Parent = parent,
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
            if (string.IsNullOrEmpty(mdRow))
            {
                Status = LexerStatus.Neutral;
                elements.Add(GenerateBrToken(parent));
                continue;
            }

            IEnumerable<Token> retElements = default;

            switch(Status)
            {
                case LexerStatus.Neutral:
                    retElements = TokenizeList(parent, mdRow);
                    break;
                case LexerStatus.UnorderedList:
                case LexerStatus.OrderedList:
                    retElements = TokenizeList(elements.Last().Parent.Parent, mdRow);
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
                    retElements = Tokenize(parent, mdRow);
                    break;
            }

            if(!retElements.Any())
            {
                Status = LexerStatus.Neutral;
                retElements = Tokenize(parent, mdRow);
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

    protected IEnumerable<Token> TokenizeList(Token parent, string text)
    {
        List<Token> elements = new();
        ElementRegexResult ulResult = new ElementRegexResult(ElementRegex.UnorderedListRegex.ElementType, Regex.Match(text, ElementRegex.UnorderedListRegex.Pattern));
        ElementRegexResult olResult = new ElementRegexResult(ElementRegex.OrderedListRegex.ElementType, Regex.Match(text, ElementRegex.OrderedListRegex.Pattern));
        if (ulResult.IsMatch)
        {
            elements.AddRange(TokenizeUnorderedList(parent, ulResult));
        }
        else if (olResult.IsMatch)
        {
            elements.AddRange(TokenizeOrderedList(parent, olResult));
        }

        return elements;
    }

    protected IEnumerable<Token> TokenizeUnorderedList(Token parent, ElementRegexResult result)
    {
        List<Token> elements = new();
        Id++;

        Token? ulToken = null;
        Token liToken = new()
        {
            Id = Id,
            ElementType = ElementType.Li,
            Content = string.Empty
        };

        if (Status == LexerStatus.Neutral)
        {
            foreach(Group g in result.Match.Groups)
            {
                Console.WriteLine(g.Value);
            }
            ulToken = new()
            {
                Id = Id,
                ElementType = ElementType.Ul,
                Parent = parent,
                Content = result.Match.Groups[1].Value
            };
            liToken.Parent = ulToken;
        }
        else
        {
            if(parent.Content.Length == result.Match.Groups[1].Value.Length)
            {
                liToken.Parent = parent;
            }
            else if (parent.Content.Length < result.Match.Groups[1].Value.Length)
            {
                ulToken = new()
                {
                    Id = Id,
                       ElementType = ElementType.Ul,
                       Parent = parent,
                       Content = result.Match.Groups[1].Value
                };
                liToken.Parent = ulToken;
            }
            else
            {
                Token tempParent = parent;

                while(true)
                {
                    if(tempParent.Parent is null)
                    {
                        liToken.Parent = tempParent;
                        break;
                    }

                    if(tempParent.Parent.ElementType != ElementType.Ul && tempParent.Parent.ElementType != ElementType.Ol)
                    {
                        liToken.Parent = tempParent;
                        break;
                    }

                    if(tempParent.Parent.Content.Length < result.Match.Groups[1].Value.Length)
                    {
                        liToken.Parent = tempParent;
                        break;
                    }
                    tempParent = tempParent.Parent;
                }
            }
        }

        if (ulToken is not null)
        {
            elements.Add(ulToken);
            Id++;
            liToken.Id++;
        }
        elements.Add(liToken);
        elements.AddRange(Tokenize(liToken, result.Match.Groups[3].Value));
        Status = LexerStatus.UnorderedList;
        return elements;
    }
    
    protected IEnumerable<Token> TokenizeOrderedList(Token parent, ElementRegexResult result)
    {
        List<Token> elements = new();
        Id++;

        Token? olToken = null;
        Token liToken = new()
        {
            Id = Id,
            ElementType = ElementType.Li,
            Content = string.Empty
        };

        if (Status == LexerStatus.Neutral)
        {
            olToken = new()
            {
                Id = Id,
                ElementType = ElementType.Ol,
                Parent = parent,
                Content = result.Match.Groups[1].Value
            };
            liToken.Parent = olToken;
        }
        else
        {
            if(parent.Content.Length == result.Match.Groups[1].Value.Length)
            {
                liToken.Parent = parent;
            }
            else if (parent.Content.Length < result.Match.Groups[1].Value.Length)
            {
                olToken = new()
                {
                    Id = Id,
                       ElementType = ElementType.Ol,
                       Parent = parent,
                       Content = result.Match.Groups[1].Value
                };
                liToken.Parent = olToken;
            }
            else
            {
                Token tempParent = parent;

                while(true)
                {
                    if(tempParent.Parent is null)
                    {
                        olToken.Parent = tempParent;
                        break;
                    }

                    if(tempParent.Parent.ElementType != ElementType.Ul && tempParent.Parent.ElementType != ElementType.Ol)
                    {
                        olToken.Parent = tempParent;
                        break;
                    }

                    if(tempParent.Parent.Content.Length < result.Match.Groups[1].Value.Length)
                    {
                        olToken.Parent = tempParent;
                        break;
                    }
                    tempParent = tempParent.Parent;
                }
            }
        }

        if (olToken is not null)
        {
            elements.Add(olToken);
            Id++;
            liToken.Id++;
        }
        elements.Add(liToken);
        elements.AddRange(Tokenize(liToken, result.Match.Groups[3].Value));
        Status = LexerStatus.OrderedList;
        return elements;
    }

    protected IEnumerable<Token> TokenizeTable(Token parent, string text)
    {
        List<Token> elements = new();

        return elements;
    }
}

