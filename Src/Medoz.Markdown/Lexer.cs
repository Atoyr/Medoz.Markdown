using System;
using System.Text.RegularExpressions;

namespace Medoz.Markdown;

public class Lexer
{
    protected int Id { get; private set; } = 0;
    protected LexerStatus Status { get; private set; }
    protected List<ElementRegexResult> Pool { get; private set; }

    public Lexer() 
    {
        Status = LexerStatus.Neutral;
        Pool = new();
    }

    protected enum LexerStatus
    {
        Neutral,
        List,
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

    protected Token GenerateParagraphToken(Token? parent, int id = 0)
    {
        return new Token()
        {
            Id = id,
               ElementType = ElementType.Paragraph,
               Parent = parent,
               Content = string.Empty
        };
    }

    protected Token GenerateBrToken(Token? parent, int id = 0)
    {
        return new Token()
        {
            Id = id,
            ElementType = ElementType.NewLine,
            Parent = parent,
            Content = string.Empty
        };
    }

    protected Token GenerateTableToken(Token? parent, int id = 0)
    {
        return new Token()
        {
            Id = id,
               ElementType = ElementType.Table,
               Parent = parent,
               Content = string.Empty
        };
    }

    public IEnumerable<Token> Tokenized(string markdownText)
    {
        string[] mdArray = markdownText.Split(new string[]{ "\r\n", "\r", "\n"}, StringSplitOptions.None);
        return Tokenized(mdArray);
    }

    public IEnumerable<Token> Tokenized(IEnumerable<string> mdArray)
    {
        List<Token> elements = new();
        Token parent = GenerateRootToken();
        elements.Add(parent);
        Status = LexerStatus.Neutral;
        LexerStatus nextStatus = LexerStatus.Neutral;
        Pool.Clear();

        foreach(string mdRow in mdArray)
        {
            // if (string.IsNullOrEmpty(mdRow))
            // {
            //     Status = LexerStatus.Neutral;
            //     elements.Add(GenerateBrToken(parent));
            //     continue;
            // }

            ElementRegexResult analizedResult = AnalizedText(mdRow);

            switch(Status)
            {
                case LexerStatus.Neutral:
                    elements.AddRange(NeutralStatusAction(parent, analizedResult, elements.Last().Id));
                    break;
                case LexerStatus.List:
                    if(analizedResult.ElementType == ElementType.Ul || analizedResult.ElementType == ElementType.Ol)
                    {
                        Pool.Add(analizedResult);
                        break;
                    }
                    
                    elements.AddRange(TokenizedList(parent, Pool, elements.Last().Id));
                    Pool.Clear();

                    elements.AddRange(NeutralStatusAction(parent, analizedResult, elements.Last().Id));
                    break;
                case LexerStatus.Table:
                    if(analizedResult.ElementType == ElementType.Table)
                    {
                        Pool.Add(analizedResult);
                        break;
                    }

                    Console.WriteLine("Pool");
                    foreach( var r in Pool)
                    {
                        Console.WriteLine(r.Content);
                    }

                    elements.AddRange(TokenizedTable(parent, Pool, elements.Last().Id));
                    Pool.Clear();

                    elements.AddRange(NeutralStatusAction(parent, analizedResult, elements.Last().Id));
                    break;
                case LexerStatus.Code:
                    // TODO
                    
                    break;
                default:
                    Pool.Add(analizedResult);
                    break;
            }
        }

        if(Pool.Any())
        {
            switch(Status)
            {
                case LexerStatus.Neutral:
                    Token p = GenerateParagraphToken(parent, elements.Last().Id);
                    elements.Add(p);
                    elements.AddRange(TokenizedInlineText(p, string.Join(' ', Pool.Select(x => x.Content).ToArray()), elements.Last().Id));
                    break;
                case LexerStatus.List:
                    elements.AddRange(TokenizedList(parent, Pool, elements.Last().Id));
                    break;
                case LexerStatus.Table:
                    // TODO
                    break;
                case LexerStatus.Code:
                    // TODO
                    break;
                default:
                    break;
            }

        }
        Pool.Clear();
        return elements;
    }

    private IEnumerable<Token> NeutralStatusAction(Token? parent, ElementRegexResult analizedResult, int id = 0)
    {
        List<Token> elements = new();
        Status = LexerStatus.Neutral;
        if(analizedResult.ElementType == ElementType.Text)
        {
            Pool.Add(analizedResult);
            return elements;
        }

        if(Pool.Any())
        {
            Token p = GenerateParagraphToken(parent, ++id);
            elements.Add(p);
            elements.AddRange(TokenizedInlineText(p, string.Join(null, Pool.Select(x => x.Content).ToArray()), elements.Last().Id));

            Pool.Clear();
        }

        if(ElementRegex.BlockElementRegexs.Any(x => x.ElementType == analizedResult.ElementType))
        {
            elements.AddRange(TokenizedBlock(parent, analizedResult, elements.LastOrDefault()?.Id ?? id));
            return elements;
        }

        if(analizedResult.ElementType == ElementType.Code)
        {
            Status = LexerStatus.Code;
            return elements;
        }

        Pool.Add(analizedResult);

        if(analizedResult.ElementType == ElementType.Ul || analizedResult.ElementType == ElementType.Ol )
        {
            Status = LexerStatus.List;
        }
        else if(analizedResult.ElementType == ElementType.Table)
        {
            Status = LexerStatus.Table;
            Console.WriteLine("Status");
        }
        return elements;
    }

    protected ElementRegexResult AnalizedText(string text)
    {
        ElementRegexResult result = new(text);
        MatchCollection mc = default;

        IEnumerable<ElementRegexResult> blockResult = 
            ElementRegex.BlockElementRegexs
            .Select(x => new ElementRegexResult(x.ElementType, text, Regex.Matches(text, x.Pattern)))
            .Where(x => x.IsMatch);
        if (blockResult.Any())
        {
            ElementRegexResult r = blockResult.First();
            result.ElementType = r.ElementType;
            result.Matches = r.Matches;
            return result;
        }

        mc = Regex.Matches(text, ElementRegex.UnorderedListRegex.Pattern);
        if(0 < mc.Count)
        {
            result.ElementType = ElementRegex.UnorderedListRegex.ElementType;
            result.Matches = mc;
            return result;
        }

        mc = Regex.Matches(text, ElementRegex.OrderedListRegex.Pattern);
        if(0 < mc.Count)
        {
            result.ElementType = ElementRegex.OrderedListRegex.ElementType;
            result.Matches = mc;
            return result;
        }

        mc = Regex.Matches(text, ElementRegex.TableHeadBodyRegex.Pattern);
        Console.WriteLine("Analizez Table");
        Console.WriteLine(text);
        Console.WriteLine(mc.Count);
        if(0 < mc.Count)
        {
            result.ElementType = ElementType.Table;
            result.Matches = mc;
            return result;
        }

        return result;
    }

    protected IEnumerable<Token> TokenizedInlineText(Token parent, string text, int id = 0)
    {
        id++;
        List<Token> elements = new();

        IEnumerable<ElementRegexResult> inlineResults =
            ElementRegex.InlineElementRegexs
            .Select(x => new ElementRegexResult(x.ElementType, text, Regex.Matches(text, x.Pattern)))
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
                    Id = id,
                       ElementType = ElementType.Text,
                       Parent = parent,
                       Content = text.Substring(0, er.Match.Index - 1)
                };
                elements.Add(token);
                text = text.Substring(er.Match.Index);

                id++;
            }

            Token newParent = new()
            {
                Id = id, 
                   ElementType = er.ElementType,
                   Parent = parent,
                   Content = string.Empty
            };
            elements.Add(newParent);
            elements.AddRange(TokenizedInlineText(newParent, er.Match.Groups[1].Value, id));
            text = text.Substring(er.Match.Length);
            if ( text.Count() > 0 )
            {
                elements.AddRange(TokenizedInlineText(parent, text, id));
            }
        }
        else
        {
            Token token = new()
            {
                Id = id,
                   ElementType = ElementType.Text,
                   Parent = parent,
                   Content = text
            };
            elements.Add(token);
        }
        return elements;
    }

    protected IEnumerable<Token> TokenizedBlock(Token parent, ElementRegexResult regexResult, int id = 0)
    {
        id++;
        List<Token> elements = new();

        Token newParent = new()
        {
            Id = id,
               ElementType = regexResult.ElementType,
               Parent = parent,
               Content = string.Empty
        };
        elements.Add(newParent);
        elements.AddRange(TokenizedInlineText(newParent, regexResult.Match.Groups[1].Value, id));
        return elements;
    }

    protected IEnumerable<Token> TokenizedList(Token parent, IEnumerable<ElementRegexResult> analizedResult, int id = 0)
    {
        List<Token> elements = new();

        foreach(var r in analizedResult)
        {
            Token p = elements.Any() ? elements.Last().Parent.Parent : parent;

            if(r.ElementType == ElementType.Ul)
            {
                elements.AddRange(TokenizedUnorderedList(p, r, id));
            }
            else
            {
                elements.AddRange(TokenizedOrderedList(p, r, id));
            }
        }
        return elements;
    }

    protected IEnumerable<Token> TokenizedUnorderedList(Token parent, ElementRegexResult result, int id = 0)
    {
        List<Token> elements = new();
        id++;

        Token? ulToken = null;
        Token liToken = new()
        {
            Id = id,
            ElementType = ElementType.Li,
            Content = string.Empty
        };

        if (parent.ElementType == ElementType.Root)
        {
            foreach(Group g in result.Match.Groups)
            {
                Console.WriteLine(g.Value);
            }
            ulToken = new()
            {
                Id = id,
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
                    Id = id,
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
            id++;
            liToken.Id++;
        }
        elements.Add(liToken);
        elements.AddRange(TokenizedInlineText(liToken, result.Match.Groups[3].Value, id));
        Status = LexerStatus.List;
        return elements;
    }
    
    protected IEnumerable<Token> TokenizedOrderedList(Token parent, ElementRegexResult result, int id = 0)
    {
        List<Token> elements = new();
        id++;

        Token? olToken = null;
        Token liToken = new()
        {
            Id = id,
            ElementType = ElementType.Li,
            Content = string.Empty
        };

        if (parent.ElementType == ElementType.Root)
        {
            olToken = new()
            {
                Id = id,
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
                    Id = id,
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
            id++;
            liToken.Id++;
        }
        elements.Add(liToken);
        elements.AddRange(TokenizedInlineText(liToken, result.Match.Groups[3].Value, id));
        Status = LexerStatus.List;
        return elements;
    }

    protected bool CheckAndAddTableToken(string text)
    {
        // Match tableMatch = Regex.Match(text, ElementRegex.TableHeadBodyRegex.Pattern);
        // if(tableMatch.Success)
        // {
        //     if (Status == LexerStatus.Table)
        //     {
        //         TableRowRegexResult.Add(new ElementRegexResult(ElementType.TableBody, tableMatch, text));
        //     }
        //     else
        //     {
        //         TableRowRegexResult.Add(new ElementRegexResult(ElementType.TableHead, tableMatch, text));
        //         Status = LexerStatus.Table;
        //     }
        //     return true;
        // }
        return false;
    }

    protected IEnumerable<Token> TokenizedTable(Token parent, IEnumerable<ElementRegexResult> analizedResults, int id = 0)
    {
        List<Token> elements = new();
        if(!analizedResults.Any())
        {
            return elements;
        }

        Token tableToken = GenerateTableToken(parent, ++id);

        ElementRegexResult before = analizedResults.First();
        List<string> align = new();

        // ヘッダ作成
        Token tHeadToken = new()
            {
                Id = ++id,
                   ElementType = ElementType.THead,
                   Parent = tableToken,
                   Content = string.Empty
            };
        Token tBodyToken = new()
            {
                Id = 0,
                   ElementType = ElementType.TBody,
                   Parent = tableToken,
                   Content = string.Empty
            };

        for(int i = 1; i < analizedResults.Count(); i++)
        {
            ElementRegexResult r = analizedResults.ElementAt(i);
            Console.WriteLine($"HEAD {i}");

            if(align.Count == 0 )
            {
                // ヘッダとAlignの数が一致することを前提とする
                if ( before.Matches.Count == analizedResults.ElementAt(i).Matches.Count)
                {
                    align.Clear();
                    foreach(Match m in analizedResults.ElementAt(i).Matches)
                    {
                        if(!m.Success)
                        {
                            align.Clear();
                            break;
                        }

                        Match alignMatch = Regex.Match(m.Groups[i].Value, ElementRegex.TableAlignRegex.Pattern);

                        if (alignMatch.Success)
                        {
                            string s = m.Groups[i].Value;
                            char f = s.First();
                            char l = s.Last();

                            if(f == ':' && l == ':')
                            {
                                align.Add("center");
                            }
                            else if(l == ':')
                            {
                                align.Add("right");
                            }
                            else
                            {
                                align.Add("left");
                            }
                        }
                        else
                        {
                            align.Clear();
                            break;
                        }
                    }

                    if(align.Count == 0 || align.Count != analizedResults.ElementAt(i).Matches.Count)
                    {
                        align.Clear();
                    }
                }

                if(align.Count > 0)
                {
                    elements.Add(tableToken);
                    elements.Add(tHeadToken);

                    Token trToken = new()
                        {
                            Id = ++id,
                               ElementType = ElementType.TR,
                               Parent = tHeadToken,
                               Content = string.Empty
                        };
                    elements.Add(trToken);
                    for(int j = 0; j < align.Count; j++)
                    {
                        Token thToken = new()
                        {
                            Id = ++id,
                               ElementType = ElementType.TH,
                               Parent = trToken,
                               Content = before.Matches[j].Groups[1].Value
                        };
                        thToken.AddAttribute("style", $"text-align: {align[j]};");
                        elements.Add(thToken);
                    }
                    tBodyToken.Id = ++id;
                    elements.Add(tBodyToken);
                }
            }
            else
            {
                Console.WriteLine($"DTIL {i}");
                Token trToken = new()
                    {
                        Id = ++id,
                           ElementType = ElementType.TR,
                           Parent = tBodyToken,
                           Content = string.Empty
                    };
                elements.Add(trToken);
                for(int j = 0; j < align.Count; j++)
                {
                    Token tdToken = new()
                    {
                        Id = ++id,
                           ElementType = ElementType.TD,
                           Parent = trToken,
                           Content = r.Matches[j]?.Groups[1]?.Value ?? string.Empty
                    };
                    tdToken.AddAttribute("style", $"text-align: {align[j]};");
                    elements.Add(tdToken);
                }
            }
            before = r;
        }

        foreach(var t in elements)
        {
            Console.WriteLine($"{t.Id} |{t.ToString()}");
        }
        return elements;
    }

}

