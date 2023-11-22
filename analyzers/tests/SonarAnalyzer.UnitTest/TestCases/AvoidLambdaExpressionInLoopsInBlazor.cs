using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Linq;
using System.Collections.Generic;

class LambdaInLoopInMethod
{
    void Method()
    {
        for (int i = 0; i < 10; i++)
        {
            var a = () => { }; // Compliant - Not in blazor
        }
    }
}

public class LambdaComponent : ComponentBase
{
    private List<Button> Buttons { get; } = new();

    private class Button
    {
        public string? Id { get; } = Guid.NewGuid().ToString();
        public Action<MouseEventArgs> Action { get; set; } = e => { };
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        foreach (var button in Buttons)
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, e => button.Action(e))); // Noncompliant
            builder.SetKey(button.Id);
            builder.AddMarkupContent(2, "\r\n        Button ");
            builder.AddContent(3, button.Id);
            builder.CloseElement();
        }

        foreach (var button in Buttons)
        {
            builder.OpenElement(4, "button");
            builder.AddAttribute(5, "onclick", new EventCallback(this, (MouseEventArgs e) => button.Action(e))); // Noncompliant
            builder.SetKey(button.Id);
            builder.AddMarkupContent(6, "\r\n        Button ");
            builder.AddContent(7, button.Id);
            builder.CloseElement();
        }

        foreach (var button in Buttons)
        {
            builder.OpenElement(8, "button");
            builder.AddAttribute(9, "onclick", (MouseEventArgs e) => button.Action(e)); // Noncompliant
            builder.SetKey(button.Id);
            builder.AddMarkupContent(10, "\r\n        Button ");
            builder.AddContent(11, button.Id);
            builder.CloseElement();
        }

        foreach (var button in Buttons)
        {
            builder.OpenElement(8, "button");
            builder.AddAttribute(9, "onclick", (MouseEventArgs e) => button.Action(e)); // Noncompliant
            builder.SetKey(button.Id);
            builder.AddMarkupContent(10, "\r\n        Button ");
            builder.AddContent(11, button.Id);
            builder.CloseElement();
        }

        foreach (var button in Buttons)
        {
            builder.OpenElement(12, "button");
            builder.AddMultipleAttributes(13, new Dictionary<string, object>()
            {
                { "onclick", (MouseEventArgs e) => button.Action(e) } // Noncompliant
            });
            builder.AddMarkupContent(14, "\r\n        Button");
            builder.CloseElement();
        }

        foreach (var button in Buttons.OrderByDescending(x => x.Id)) { } // Compliant, the lambda is executed outside of the loop
    }
}
