// Copyright (C) 2026 jmh
// SPDX-License-Identifier: GPL-3.0-only

using Foundation;
using UIKit;
using Stratum.iOS.ViewController;

namespace Stratum.iOS
{
    [Register("SceneDelegate")]
    public class SceneDelegate : UIResponder, IUIWindowSceneDelegate
    {
        [Export("window")]
        public UIWindow Window { get; set; }

        [Export("scene:willConnectToSession:options:")]
        public void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
        {
            if (scene is not UIWindowScene windowScene)
            {
                return;
            }

            var database = new Database();
            Dependencies.Init(database);

            Window = new UIWindow(windowScene);
            var rootVc = new AuthenticatorListViewController(database);
            var nav = new UINavigationController(rootVc);
            
            nav.NavigationBar.PrefersLargeTitles = true;
            nav.NavigationBar.TintColor = UIColor.SystemBlue;

            Window.RootViewController = nav;
            Window.MakeKeyAndVisible();
        }

        [Export("sceneDidDisconnect:")]
        public void DidDisconnect(UIScene scene)
        {
        }

        [Export("sceneDidBecomeActive:")]
        public void DidBecomeActive(UIScene scene)
        {
        }

        [Export("sceneWillResignActive:")]
        public void WillResignActive(UIScene scene)
        {
        }

        [Export("sceneWillEnterForeground:")]
        public void WillEnterForeground(UIScene scene)
        {
        }

        [Export("sceneDidEnterBackground:")]
        public void DidEnterBackground(UIScene scene)
        {
        }
    }
}
