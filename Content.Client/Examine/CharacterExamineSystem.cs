using Content.Client.Examine.UI;
using Content.Shared.Examine;
using Content.Shared.Humanoid;
using Content.Shared.Verbs;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Shared.Utility;
using System.Text.RegularExpressions;

namespace Content.Client.Examine;

/// <summary>
/// Adds a "Character" examine button to humanoid entities that opens a character info window
/// </summary>
public sealed class CharacterExamineSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystem _examine = default!;
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

    private readonly Dictionary<NetEntity, CharacterDetailWindow> _openWindows = new();
    private static readonly Regex UrlRegex = new Regex(@"https?://[^\s]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HumanoidAppearanceComponent, GetVerbsEvent<ExamineVerb>>(OnGetExamineVerbs);
        SubscribeNetworkEvent<CharacterInfoEvent>(HandleCharacterInfo);
    }

    private void OnGetExamineVerbs(EntityUid uid, HumanoidAppearanceComponent component, GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        args.Verbs.Add(new ExamineVerb
        {
            Text = Loc.GetString("character-examine-verb"),
            Icon = new SpriteSpecifier.Texture(new ("/Textures/Interface/VerbIcons/sentient.svg.192dpi.png")), // TODO: Create custom character icon
            Act = () => OpenCharacterWindow(uid),
            ClientExclusive = true,
            ShowOnExamineTooltip = true,
            CloseMenu = false
        });
    }

    private void OpenCharacterWindow(EntityUid uid)
    {
        var netEntity = GetNetEntity(uid);

        // Close existing window for this entity if it exists
        if (_openWindows.TryGetValue(netEntity, out var existingWindow))
        {
            existingWindow.Close();
            _openWindows.Remove(netEntity);
        }

        // Create and show new window
        var window = new CharacterDetailWindow();
        _openWindows[netEntity] = window;

        window.OnClose += () =>
        {
            _openWindows.Remove(netEntity);
        };

        window.OpenCentered();

        // Request character info from server
        RaiseNetworkEvent(new RequestCharacterInfoEvent { Entity = netEntity });
    }

    private void HandleCharacterInfo(CharacterInfoEvent message)
    {
        if (!_openWindows.TryGetValue(message.Entity, out var window))
            return;

        // Set character info
        window.SetCharacterInfo(message.CharacterName, message.JobTitle);

        // Set description with clickable URLs
        var descriptionMessage = new FormattedMessage();
        if (!string.IsNullOrWhiteSpace(message.Description))
        {
            descriptionMessage.AddMarkup(ConvertUrlsToLinks(message.Description));
        }
        else
        {
            descriptionMessage.AddText(Loc.GetString("character-window-no-description"));
        }
        window.SetDescription(descriptionMessage);

        // Set consent text with clickable URLs
        var consentMessage = new FormattedMessage();
        if (!string.IsNullOrWhiteSpace(message.ConsentText))
        {
            consentMessage.AddMarkup(ConvertUrlsToLinks(message.ConsentText));
        }
        else
        {
            consentMessage.AddText(Loc.GetString("character-window-no-consent"));
        }
        window.SetConsent(consentMessage);
    }

    /// <summary>
    /// Converts plain text URLs to clickable [url] markup tags
    /// </summary>
    private string ConvertUrlsToLinks(string text)
    {
        return UrlRegex.Replace(text, match => $"[url]{match.Value}[/url]");
    }
}

