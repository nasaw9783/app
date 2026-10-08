// Copyright (C) 2026 jmh
// SPDX-License-Identifier: GPL-3.0-only

using System;
using Foundation;
using UIKit;
using Stratum.iOS.Service;
using Stratum.iOS.Storage;

namespace Stratum.iOS.ViewController
{
    public class SettingsViewController : UITableViewController
    {
        private readonly IosBiometricService _biometricService;
        private readonly IosSecureStorage _secureStorage;
        private UISwitch _biometricSwitch;

        public SettingsViewController() : base(UITableViewStyle.InsetGrouped)
        {
            _biometricService = Dependencies.Resolve<IosBiometricService>();
            _secureStorage = Dependencies.Resolve<IosSecureStorage>();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Title = "Settings";
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(
                UIBarButtonSystemItem.Done,
                (s, e) => DismissViewController(true, null)
            );
        }

        public override nint NumberOfSections(UITableView tableView) => 2;

        public override nint RowsInSection(UITableView tableView, nint section) => section == 0 ? 1 : 2;

        public override string TitleForHeader(UITableView tableView, nint section) =>
            section == 0 ? "Security" : "About";

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = new UITableViewCell(UITableViewCellStyle.Value1, "SettingsCell");

            if (indexPath.Section == 0 && indexPath.Row == 0)
            {
                cell.TextLabel.Text = "Require Face ID / Touch ID";
                _biometricSwitch = new UISwitch
                {
                    On = _secureStorage.Get("use_biometrics") == "true"
                };
                _biometricSwitch.ValueChanged += BiometricSwitchChanged;
                cell.AccessoryView = _biometricSwitch;
            }
            else if (indexPath.Section == 1 && indexPath.Row == 0)
            {
                cell.TextLabel.Text = "Stratum for iOS";
                cell.DetailTextLabel.Text = "v1.0.0";
            }
            else if (indexPath.Section == 1 && indexPath.Row == 1)
            {
                cell.TextLabel.Text = "Core Engine";
                cell.DetailTextLabel.Text = ".NET 10 (Stratum.Core)";
            }

            return cell;
        }

        private async void BiometricSwitchChanged(object sender, EventArgs e)
        {
            if (_biometricSwitch.On)
            {
                var success = await _biometricService.AuthenticateAsync("Verify biometric access to enable security lock");
                if (success)
                {
                    _secureStorage.Set("use_biometrics", "true");
                }
                else
                {
                    _biometricSwitch.On = false;
                    _secureStorage.Set("use_biometrics", "false");
                }
            }
            else
            {
                _secureStorage.Set("use_biometrics", "false");
            }
        }
    }
}
