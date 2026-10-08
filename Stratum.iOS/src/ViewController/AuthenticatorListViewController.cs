// Copyright (C) 2026 jmh
// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using UIKit;
using Stratum.Core.Entity;
using Stratum.Core.Generator;
using Stratum.Core.Service;
using Stratum.iOS.Service;
using Stratum.iOS.Storage;
using Stratum.iOS.View;
using Serilog;

namespace Stratum.iOS.ViewController
{
    public class AuthenticatorListViewController : UIViewController, IUITableViewDataSource, IUITableViewDelegate, IUISearchResultsUpdating
    {
        private readonly ILogger _log = Log.ForContext<AuthenticatorListViewController>();
        private readonly Database _database;

        private IAuthenticatorService _authService;
        private IosBiometricService _biometricService;
        private IosSecureStorage _secureStorage;

        private UITableView _tableView;
        private UISearchController _searchController;
        private NSTimer _refreshTimer;
        private UILabel _emptyStateLabel;

        private List<Authenticator> _allAuthenticators = new();
        private List<Authenticator> _filteredAuthenticators = new();
        private readonly Dictionary<string, IGenerator> _generators = new();

        public AuthenticatorListViewController(Database database)
        {
            _database = database;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "Stratum";
            View.BackgroundColor = UIColor.SystemGroupedBackground;

            _authService = Dependencies.Resolve<IAuthenticatorService>();
            _biometricService = Dependencies.Resolve<IosBiometricService>();
            _secureStorage = Dependencies.Resolve<IosSecureStorage>();

            SetupNavigation();
            SetupSearch();
            SetupTableView();
            SetupEmptyState();
        }

        public override async void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            await CheckBiometricsAndLoadAsync();
            StartTimer();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            StopTimer();
        }

        private void SetupNavigation()
        {
            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(
                UIImage.GetSystemImage("gearshape"),
                UIBarButtonItemStyle.Plain,
                (s, e) => ShowSettings()
            );

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(
                UIBarButtonSystemItem.Add,
                (s, e) => ShowAddActionSheet()
            );
        }

        private void SetupSearch()
        {
            _searchController = new UISearchController(searchResultsController: null)
            {
                SearchResultsUpdater = this,
                ObscuresBackgroundDuringPresentation = false
            };
            _searchController.SearchBar.Placeholder = "Search authenticators";
            NavigationItem.SearchController = _searchController;
            DefinesPresentationContext = true;
        }

        private void SetupTableView()
        {
            _tableView = new UITableView(CGRect.Empty, UITableViewStyle.InsetGrouped)
            {
                DataSource = this,
                Delegate = this,
                TranslatesAutoresizingMaskIntoConstraints = false,
                RowHeight = UITableView.AutomaticDimension,
                EstimatedRowHeight = 110,
                SeparatorStyle = UITableViewCellSeparatorStyle.None,
                BackgroundColor = UIColor.Clear
            };

            _tableView.RegisterClassForCellReuse(typeof(AuthenticatorCell), AuthenticatorCell.CellId);

            View.AddSubview(_tableView);

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _tableView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                _tableView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _tableView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _tableView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
            });
        }

        private void SetupEmptyState()
        {
            _emptyStateLabel = new UILabel
            {
                Text = "No 2FA accounts added yet.\nTap + to scan a QR code.",
                TextColor = UIColor.SecondaryLabel,
                TextAlignment = UITextAlignment.Center,
                Lines = 0,
                Font = UIFont.PreferredFontForTextStyle(UIFontTextStyle.Body),
                TranslatesAutoresizingMaskIntoConstraints = false,
                Hidden = true
            };

            View.AddSubview(_emptyStateLabel);

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _emptyStateLabel.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                _emptyStateLabel.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor),
                _emptyStateLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor, 32),
                _emptyStateLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor, -32)
            });
        }

        private async Task CheckBiometricsAndLoadAsync()
        {
            var isLocked = _secureStorage.Get("use_biometrics") == "true";
            if (isLocked)
            {
                var authenticated = await _biometricService.AuthenticateAsync("Unlock Stratum Authenticator");
                if (!authenticated)
                {
                    _allAuthenticators.Clear();
                    _filteredAuthenticators.Clear();
                    _tableView.ReloadData();
                    return;
                }
            }

            if (!await _database.IsOpenAsync(Database.Origin.ViewController))
            {
                await _database.OpenAsync(null, Database.Origin.ViewController);
            }

            await ReloadDataAsync();
        }

        private async Task ReloadDataAsync()
        {
            try
            {
                _allAuthenticators = await _authService.GetAllAsync();
                _generators.Clear();

                foreach (var auth in _allAuthenticators)
                {
                    _generators[auth.Secret] = GeneratorFactory.Create(auth);
                }

                FilterAccounts(_searchController?.SearchBar?.Text);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to load authenticators");
            }
        }

        private void FilterAccounts(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                _filteredAuthenticators = new List<Authenticator>(_allAuthenticators);
            }
            else
            {
                var query = searchText.Trim().ToLowerInvariant();
                _filteredAuthenticators = _allAuthenticators
                    .Where(a => (a.Issuer != null && a.Issuer.ToLowerInvariant().Contains(query)) ||
                                (a.Username != null && a.Username.ToLowerInvariant().Contains(query)))
                    .ToList();
            }

            _emptyStateLabel.Hidden = _allAuthenticators.Count > 0;
            _tableView.ReloadData();
        }

        public void UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            FilterAccounts(searchController.SearchBar.Text);
        }

        private void StartTimer()
        {
            StopTimer();
            _refreshTimer = NSTimer.CreateRepeatingScheduledTimer(1.0, _ =>
            {
                UpdateVisibleCells();
            });
        }

        private void StopTimer()
        {
            _refreshTimer?.Invalidate();
            _refreshTimer = null;
        }

        private void UpdateVisibleCells()
        {
            if (_tableView?.IndexPathsForVisibleRows == null) return;

            foreach (var indexPath in _tableView.IndexPathsForVisibleRows)
            {
                if (_tableView.CellAt(indexPath) is AuthenticatorCell cell && cell.Authenticator != null)
                {
                    if (_generators.TryGetValue(cell.Authenticator.Secret, out var generator))
                    {
                        cell.UpdateCode(generator);
                    }
                }
            }
        }

        public nint NumberOfSections(UITableView tableView) => 1;

        public nint RowsInSection(UITableView tableView, nint section) => _filteredAuthenticators.Count;

        public UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (AuthenticatorCell)tableView.DequeueReusableCell(AuthenticatorCell.CellId, indexPath);
            var auth = _filteredAuthenticators[indexPath.Row];

            if (!_generators.TryGetValue(auth.Secret, out var generator))
            {
                generator = GeneratorFactory.Create(auth);
                _generators[auth.Secret] = generator;
            }

            cell.Bind(auth, generator);
            cell.OnCopied = OnAccountCopied;

            return cell;
        }

        public void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            tableView.DeselectRow(indexPath, true);
            var auth = _filteredAuthenticators[indexPath.Row];
            OnAccountCopied(auth);
        }

        private void OnAccountCopied(Authenticator auth)
        {
            if (_generators.TryGetValue(auth.Secret, out var generator))
            {
                var code = generator.GenerateCode();
                UIPasteboard.General.String = code;

                var feedback = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Medium);
                feedback.Prepare();
                feedback.ImpactOccurred();

                ShowToast($"Copied code for {auth.Issuer ?? auth.Username}");
            }
        }

        private void ShowToast(string message)
        {
            var alert = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);
            PresentViewController(alert, true, () =>
            {
                Task.Delay(1000).ContinueWith(_ =>
                {
                    InvokeOnMainThread(() => alert.DismissViewController(true, null));
                });
            });
        }

        private void ShowAddActionSheet()
        {
            var actionSheet = UIAlertController.Create("Add Account", null, UIAlertControllerStyle.ActionSheet);

            actionSheet.AddAction(UIAlertAction.Create("Scan QR Code", UIAlertActionStyle.Default, _ =>
            {
                var qrVc = new QrScannerViewController();
                qrVc.OnAuthenticatorAdded += async () => await ReloadDataAsync();
                var nav = new UINavigationController(qrVc);
                PresentViewController(nav, true, null);
            }));

            actionSheet.AddAction(UIAlertAction.Create("Enter Key Manually", UIAlertActionStyle.Default, _ =>
            {
                var manualVc = new ManualAddViewController();
                manualVc.OnAuthenticatorAdded += async () => await ReloadDataAsync();
                var nav = new UINavigationController(manualVc);
                PresentViewController(nav, true, null);
            }));

            actionSheet.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            PresentViewController(actionSheet, true, null);
        }

        private void ShowSettings()
        {
            var settingsVc = new SettingsViewController();
            var nav = new UINavigationController(settingsVc);
            PresentViewController(nav, true, null);
        }
    }
}
