using System;
using System.Linq;

namespace Medoz.Markdown;

public class Parser
{
    public IEnumerable<Token>Parse(string markdownText)
    {
        Lexer l = new();
        List<Token> tokens = l.Analize(markdownText).ToList();

        // showTree(tokens);

        for(int i = tokens.Count() - 1; 0 <= i; i--)
        {
            Token token = tokens[i];
            if(token.Parent is null) continue;
            if(token.Parent.ElementType == ElementType.Root) continue;
            token.Parent.Content = token.ToString() + token.Parent.Content;
            tokens.RemoveAt(i);
        }
        return tokens;
    }

    private void showTree(List<Token> tokens)
    {
        Console.WriteLine("show tree");
        foreach(var t in tokens)
        {
            Console.WriteLine($"{t.Id} |{t.ToString()}");
        }
    }
}
