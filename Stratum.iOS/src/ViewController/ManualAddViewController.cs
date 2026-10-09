// Copyright (C) 2026 jmh
// SPDX-License-Identifier: GPL-3.0-only

using System;
using UIKit;
using Stratum.Core;
using Stratum.Core.Entity;
using Stratum.Core.Service;

namespace Stratum.iOS.ViewController
{
    public class ManualAddViewController : UIViewController
    {
        private readonly UITextField _issuerField = new();
        private readonly UITextField _usernameField = new();
        private readonly UITextField _secretField = new();
        private readonly UISegmentedControl _typeSegment = new(new[] { "TOTP", "HOTP" });

        public event Action OnAuthenticatorAdded;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "Add Account";
            View.BackgroundColor = UIColor.SystemGroupedBackground;

            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(
                UIBarButtonSystemItem.Cancel,
                (s, e) => DismissViewController(true, null)
            );

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(
                UIBarButtonSystemItem.Save,
                (s, e) => SaveAccount()
            );

            SetupForm();
        }

        private void SetupForm()
        {
            var stack = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                Spacing = 16,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            ConfigureTextField(_issuerField, "Service (e.g. GitHub, Google)");
            ConfigureTextField(_usernameField, "Account (e.g. user@example.com)");
            ConfigureTextField(_secretField, "Secret Key (Base32)");
            _secretField.AutocapitalizationType = UITextAutocapitalizationType.AllCharacters;
            _secretField.AutocorrectionType = UITextAutocorrectionType.No;

            _typeSegment.SelectedSegment = 0;

            stack.AddArrangedSubview(new UILabel { Text = "Account Information", Font = UIFont.PreferredFontForTextStyle(UIFontTextStyle.Headline) });
            stack.AddArrangedSubview(_issuerField);
            stack.AddArrangedSubview(_usernameField);
            stack.AddArrangedSubview(_secretField);
            stack.AddArrangedSubview(new UILabel { Text = "Type", Font = UIFont.PreferredFontForTextStyle(UIFontTextStyle.Subheadline) });
            stack.AddArrangedSubview(_typeSegment);

            View.AddSubview(stack);

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                stack.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 20),
                stack.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor, 20),
                stack.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor, -20)
            });
        }

        private static void ConfigureTextField(UITextField field, string placeholder)
        {
            field.Placeholder = placeholder;
            field.BorderStyle = UITextBorderStyle.RoundedRect;
            field.BackgroundColor = UIColor.SecondarySystemGroupedBackground;
            field.ClearButtonMode = UITextFieldViewMode.WhileEditing;
            field.HeightAnchor.ConstraintEqualTo(44).Active = true;
        }

        private async void SaveAccount()
        {
            var secret = _secretField.Text?.Trim().Replace(" ", "").ToUpperInvariant();
            if (string.IsNullOrEmpty(secret))
            {
                ShowError("Please enter the secret key.");
                return;
            }

            var auth = new Authenticator
            {
                Issuer = _issuerField.Text?.Trim(),
                Username = _usernameField.Text?.Trim(),
                Secret = secret,
                Type = _typeSegment.SelectedSegment == 0 ? AuthenticatorType.Totp : AuthenticatorType.Hotp,
                Algorithm = Stratum.Core.Generator.HashAlgorithm.Sha1,
                Digits = 6,
                Period = 30
            };

            try
            {
                auth.Validate();
                var service = Dependencies.Resolve<IAuthenticatorService>();
                await service.AddAsync(auth);

                OnAuthenticatorAdded?.Invoke();
                DismissViewController(true, null);
            }
            catch (Exception ex)
            {
                ShowError($"Validation error: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            var alert = UIAlertController.Create("Error", message, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
            PresentViewController(alert, true, null);
        }
    }
}
