using System.Linq;
using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Shared.Doors.Components;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Localization;

namespace Content.Client.Doors;

public sealed class AdvancedAirlockWindow : DefaultWindow
{
    private readonly Label _statusLabel;
    private readonly Label _ownerLabel;
    private readonly BoxContainer _authorizedListContainer;
    private readonly LineEdit _addUserLineEdit;
    private readonly Button _addUserButton;
    private readonly Button _claimButton;
    private readonly Button _resetButton;

    public event Action? OnClaimPressed;
    public event Action<string>? OnAddUserPressed;
    public event Action<string>? OnRemoveUserPressed;
    public event Action? OnResetPressed;

    public AdvancedAirlockWindow()
    {
        MinSize = new Vector2(400, 300);

        var mainContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Children =
            {
                new BoxContainer
                {
                    Orientation = BoxContainer.LayoutOrientation.Vertical,
                    Margin = new Thickness(10),
                    Children =
                    {
                        new Label
                        {
                            Text = Loc.GetString("advanced-airlock-window-status-label"),
                            StyleClasses = { "LabelBig" }
                        },
                        (_statusLabel = new Label
                        {
                            Text = Loc.GetString("advanced-airlock-unclaimed"),
                            Margin = new Thickness(0, 5)
                        }),
                        (_ownerLabel = new Label
                        {
                            Visible = false,
                            Margin = new Thickness(0, 5)
                        }),
                        (_claimButton = new Button
                        {
                            Text = Loc.GetString("advanced-airlock-claim-button"),
                            Margin = new Thickness(0, 10, 0, 0)
                        }),
                        new Label
                        {
                            Text = Loc.GetString("advanced-airlock-authorized-users-label"),
                            StyleClasses = { "LabelBig" },
                            Margin = new Thickness(0, 20, 0, 0),
                            Visible = false
                        },
                        new ScrollContainer
                        {
                            MinSize = new Vector2(0, 150),
                            Margin = new Thickness(0, 5),
                            Children =
                            {
                                (_authorizedListContainer = new BoxContainer
                                {
                                    Orientation = BoxContainer.LayoutOrientation.Vertical
                                })
                            }
                        },
                        new BoxContainer
                        {
                            Orientation = BoxContainer.LayoutOrientation.Horizontal,
                            Margin = new Thickness(0, 10, 0, 0),
                            Children =
                            {
                                (_addUserLineEdit = new LineEdit
                                {
                                    PlaceHolder = Loc.GetString("advanced-airlock-add-user-placeholder"),
                                    HorizontalExpand = true
                                }),
                                (_addUserButton = new Button
                                {
                                    Text = Loc.GetString("advanced-airlock-add-button"),
                                    Margin = new Thickness(5, 0, 0, 0)
                                })
                            }
                        },
                        (_resetButton = new Button
                        {
                            Text = Loc.GetString("advanced-airlock-reset-button"),
                            Margin = new Thickness(0, 10, 0, 0),
                            Modulate = Color.FromHex("#d4380d")
                        })
                    }
                }
            }
        };

        Contents.AddChild(mainContainer);

        _claimButton.OnPressed += _ => OnClaimPressed?.Invoke();
        _addUserButton.OnPressed += _ => OnAddUser();
        _addUserLineEdit.OnTextEntered += _ => OnAddUser();
        _resetButton.OnPressed += _ => OnResetPressed?.Invoke();
    }

    private void OnAddUser()
    {
        var userName = _addUserLineEdit.Text.Trim();
        if (string.IsNullOrWhiteSpace(userName))
            return;

        OnAddUserPressed?.Invoke(userName);
        _addUserLineEdit.Clear();
    }

    public void UpdateState(AdvancedAirlockBuiState state)
    {
        // Update status label
        if (state.IsClaimed)
        {
            _statusLabel.Text = Loc.GetString("advanced-airlock-claimed-status");
            _statusLabel.Modulate = Color.FromHex("#20c997");
        }
        else
        {
            _statusLabel.Text = Loc.GetString("advanced-airlock-unclaimed");
            _statusLabel.Modulate = Color.White;
        }

        // Update owner label
        if (state.OwnerName != null)
        {
            _ownerLabel.Visible = true;
            var jobTitle = string.IsNullOrEmpty(state.OwnerJobTitle) 
                ? Loc.GetString("advanced-airlock-no-job")
                : state.OwnerJobTitle;
            _ownerLabel.Text = Loc.GetString("advanced-airlock-owner-label", 
                ("name", state.OwnerName), 
                ("job", jobTitle));
        }
        else
        {
            _ownerLabel.Visible = false;
        }

        // Update claim button visibility
        _claimButton.Visible = !state.IsClaimed;

        // Update authorized users list
        _authorizedListContainer.RemoveAllChildren();
        
        if (state.IsOwner && state.IsClaimed)
        {
            // Show management controls for owner
            _addUserLineEdit.Visible = true;
            _addUserButton.Visible = true;
            _resetButton.Visible = true;

            foreach (var user in state.AuthorizedUsers.OrderBy(u => u))
            {
                var userRow = new BoxContainer
                {
                    Orientation = BoxContainer.LayoutOrientation.Horizontal,
                    Margin = new Thickness(0, 2),
                    Children =
                    {
                        new Label
                        {
                            Text = user,
                            HorizontalExpand = true,
                            ClipText = true
                        },
                        new Button
                        {
                            Text = Loc.GetString("advanced-airlock-remove-button"),
                            Modulate = Color.FromHex("#d4380d")
                        }
                    }
                };

                var removeButton = (Button)userRow.Children.Last();
                var userName = user;
                removeButton.OnPressed += _ => OnRemoveUserPressed?.Invoke(userName);

                _authorizedListContainer.AddChild(userRow);
            }

            if (!state.AuthorizedUsers.Any())
            {
                _authorizedListContainer.AddChild(new Label
                {
                    Text = Loc.GetString("advanced-airlock-no-authorized-users"),
                    Modulate = Color.Gray
                });
            }
        }
        else
        {
            // Hide management controls for non-owners
            _addUserLineEdit.Visible = false;
            _addUserButton.Visible = false;
            _resetButton.Visible = false;

            if (state.AuthorizedUsers.Any())
            {
                foreach (var user in state.AuthorizedUsers.OrderBy(u => u))
                {
                    _authorizedListContainer.AddChild(new Label
                    {
                        Text = user,
                        Margin = new Thickness(0, 2)
                    });
                }
            }
            else if (state.IsClaimed)
            {
                _authorizedListContainer.AddChild(new Label
                {
                    Text = Loc.GetString("advanced-airlock-no-authorized-users"),
                    Modulate = Color.Gray
                });
            }
        }
    }
}
