using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorSample.Pages.S6802;

public class LambdaInLoopInMethod
{
    public void Method()
    {
        for (int i = 0; i < 10; i++)
        {
            var a = () => { };
        }
    }
}

public class LambdaComponent : ComponentBase
{
    private List<Button> Buttons { get; } = new();

    private sealed class Button
    {
        public string? Id { get; } = Guid.NewGuid().ToString();
        public Action<MouseEventArgs> Action { get; set; } = e => { };
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        foreach (var button in Buttons)
        {
            builder.OpenElement(12, "button");
            builder.AddMultipleAttributes(13, new Dictionary<string, object>
            {
                { "onclick", (MouseEventArgs e) => button.Action(e) }
            });
            builder.AddMarkupContent(14, "\r\n        Button");
            builder.CloseElement();
        }
    }
}
