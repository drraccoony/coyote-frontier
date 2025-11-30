using System.Diagnostics.CodeAnalysis;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Input;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Utility;

namespace Content.Client.Examine;

/// <summary>
/// Markup tag for creating clickable URL links
/// </summary>
public sealed class UrlLinkTag : IMarkupTagHandler
{
    [Dependency] private readonly IUriOpener _uriOpener = default!;

    public string Name => "url";

    public bool TryCreateControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        if (!node.Value.TryGetString(out var text))
        {
            control = null;
            return false;
        }

        // Get URL from attribute, or use the text itself as the URL
        var url = text;
        if (node.Attributes.TryGetValue("link", out var linkParameter) && linkParameter.TryGetString(out var linkUrl))
        {
            url = linkUrl;
        }

        var label = new Label
        {
            Text = text,
            MouseFilter = Control.MouseFilterMode.Stop,
            FontColorOverride = Color.LightBlue,
            DefaultCursorShape = Control.CursorShape.Hand
        };

        label.OnMouseEntered += _ => label.FontColorOverride = Color.Cyan;
        label.OnMouseExited += _ => label.FontColorOverride = Color.LightBlue;
        label.OnKeyBindDown += args => OnKeybindDown(args, url);

        control = label;
        return true;
    }

    private void OnKeybindDown(GUIBoundKeyEventArgs args, string url)
    {
        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        _uriOpener.OpenUri(url);
    }
}
