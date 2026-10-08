// Copyright (C) 2026 jmh
// SPDX-License-Identifier: GPL-3.0-only

using Foundation;
using UIKit;
using Serilog;

namespace Stratum.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : UIResponder, IUIApplicationDelegate
    {
        [Export("window")]
        public UIWindow Window { get; set; }

        [Export("application:didFinishLaunchingWithOptions:")]
        public bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.NSLog()
                .CreateLogger();

            Log.Information("Stratum iOS launching");

            return true;
        }

        [Export("application:configurationForConnectingSceneSession:options:")]
        public UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
        {
            return new UISceneConfiguration("Default Configuration", connectingSceneSession.Role);
        }

        [Export("application:didDiscardSceneSessions:")]
        public void DidDiscardSceneSessions(UIApplication application, NSSet<UISceneSession> sceneSessions)
        {
        }
    }
}
