// Copyright (C) 2026 jmh
// SPDX-License-Identifier: GPL-3.0-only

using System;
using CoreGraphics;
using Foundation;
using UIKit;
using Stratum.Core.Entity;
using Stratum.Core.Generator;

namespace Stratum.iOS.View
{
    public class AuthenticatorCell : UITableViewCell
    {
        public const string CellId = "AuthenticatorCell";

        private readonly UILabel _issuerLabel = new();
        private readonly UILabel _accountLabel = new();
        private readonly UILabel _codeLabel = new();
        private readonly CircularProgressView _progressView = new(new CGRect(0, 0, 36, 36));
        private readonly UIButton _copyButton = new(UIButtonType.System);

        public Authenticator Authenticator { get; private set; }
        public Action<Authenticator> OnCopied { get; set; }

        public AuthenticatorCell(IntPtr handle) : base(handle)
        {
            Setup();
        }

        [Export("initWithStyle:reuseIdentifier:")]
        public AuthenticatorCell(UITableViewCellStyle style, string reuseIdentifier) : base(style, reuseIdentifier)
        {
            Setup();
        }

        private void Setup()
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;
            BackgroundColor = UIColor.SecondarySystemGroupedBackground;
            ContentView.Layer.CornerRadius = 12;
            ContentView.ClipsToBounds = true;

            // Issuer label
            _issuerLabel.Font = UIFont.PreferredFontForTextStyle(UIFontTextStyle.Headline);
            _issuerLabel.TextColor = UIColor.Label;
            _issuerLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            // Account label
            _accountLabel.Font = UIFont.PreferredFontForTextStyle(UIFontTextStyle.Subheadline);
            _accountLabel.TextColor = UIColor.SecondaryLabel;
            _accountLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            // 2FA Code label
            _codeLabel.Font = UIFont.MonospacedDigitSystemFontOfSize(28, UIFontWeight.Bold);
            _codeLabel.TextColor = UIColor.SystemBlue;
            _codeLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            // Copy button
            _copyButton.SetImage(UIImage.GetSystemImage("doc.on.doc"), UIControlState.Normal);
            _copyButton.TintColor = UIColor.SecondaryLabel;
            _copyButton.TranslatesAutoresizingMaskIntoConstraints = false;
            _copyButton.TouchUpInside += (s, e) =>
            {
                if (Authenticator != null)
                {
                    OnCopied?.Invoke(Authenticator);
                }
            };

            _progressView.TranslatesAutoresizingMaskIntoConstraints = false;

            ContentView.AddSubview(_issuerLabel);
            ContentView.AddSubview(_accountLabel);
            ContentView.AddSubview(_codeLabel);
            ContentView.AddSubview(_progressView);
            ContentView.AddSubview(_copyButton);

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _issuerLabel.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor, 12),
                _issuerLabel.LeadingAnchor.ConstraintEqualTo(ContentView.LeadingAnchor, 16),
                _issuerLabel.TrailingAnchor.ConstraintLessThanOrEqualTo(_progressView.LeadingAnchor, -12),

                _accountLabel.TopAnchor.ConstraintEqualTo(_issuerLabel.BottomAnchor, 2),
                _accountLabel.LeadingAnchor.ConstraintEqualTo(_issuerLabel.LeadingAnchor),
                _accountLabel.TrailingAnchor.ConstraintLessThanOrEqualTo(_progressView.LeadingAnchor, -12),

                _codeLabel.TopAnchor.ConstraintEqualTo(_accountLabel.BottomAnchor, 8),
                _codeLabel.LeadingAnchor.ConstraintEqualTo(_issuerLabel.LeadingAnchor),
                _codeLabel.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor, -12),

                _copyButton.CenterYAnchor.ConstraintEqualTo(_codeLabel.CenterYAnchor),
                _copyButton.LeadingAnchor.ConstraintEqualTo(_codeLabel.TrailingAnchor, 12),
                _copyButton.WidthAnchor.ConstraintEqualTo(32),
                _copyButton.HeightAnchor.ConstraintEqualTo(32),

                _progressView.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                _progressView.TrailingAnchor.ConstraintEqualTo(ContentView.TrailingAnchor, -16),
                _progressView.WidthAnchor.ConstraintEqualTo(36),
                _progressView.HeightAnchor.ConstraintEqualTo(36)
            });
        }

        public void Bind(Authenticator authenticator, IGenerator generator)
        {
            Authenticator = authenticator;

            _issuerLabel.Text = string.IsNullOrWhiteSpace(authenticator.Issuer) ? authenticator.Username : authenticator.Issuer;
            _accountLabel.Text = !string.IsNullOrWhiteSpace(authenticator.Issuer) ? authenticator.Username : "";
            _accountLabel.Hidden = string.IsNullOrWhiteSpace(_accountLabel.Text);

            UpdateCode(generator);
        }

        public void UpdateCode(IGenerator generator)
        {
            if (generator == null) return;

            var code = generator.GenerateCode();
            _codeLabel.Text = FormatCode(code);

            var remaining = generator.GetRemainingSeconds();
            var period = Authenticator.Period > 0 ? Authenticator.Period : 30;
            var progress = (float)remaining / period;

            _progressView.SetProgress(progress, remaining);
        }

        private static string FormatCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return "--- ---";
            if (code.Length == 6)
            {
                return $"{code[..3]} {code[3..]}";
            }
            if (code.Length == 8)
            {
                return $"{code[..4]} {code[4..]}";
            }
            return code;
        }
    }
}
