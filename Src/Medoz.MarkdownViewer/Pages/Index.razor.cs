using System;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using ElectronNET.API;
using ElectronNET.API.Entities;

using Medoz.Markdown;

namespace Medoz.MarkdownViewer.Pages;

public partial class Index : ComponentBase
{

    private string _text { get; set; }

    private IEnumerable<Token>? _tokens { get; set; }

    async void Load() 
    {
        Parser p = new();
        _tokens = p.Parse(_text);
        foreach(var t in _tokens)
        {
            Console.WriteLine(t.ToString());
        }
        StateHasChanged();
    }
}
