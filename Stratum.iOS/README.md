# Stratum for iOS

Stratum for iOS is the native Apple iOS client for **Stratum**, sharing business logic, cryptography, OTP generation, and database schemas directly with `Stratum.Core`.

## 📱 Features

- **OTP Engine**: Directly powered by `Stratum.Core` supporting TOTP, HOTP, Steam, mOTP, and Yandex.
- **Biometric Security**: Protect authentication codes using Face ID / Touch ID via iOS `LocalAuthentication`.
- **Hardware-backed Storage**: Master keys and preferences secured via Apple iOS `Keychain`.
- **Database**: Local encrypted database powered by SQLite & SQLCipher.
- **QR Code Scanning**: High-speed camera scanner using iOS `AVFoundation` with instant `otpauth://` URI parsing.
- **Modern UI**: Clean iOS interface matching iOS Human Interface Guidelines with smooth countdown rings and dark mode support.

## 🏗️ Architecture

```
Stratum.sln
 ├── Stratum.Core/           (Shared C# logic: Generators, Crypto, Converters, Services)
 ├── Stratum.Droid/          (Android Client)
 ├── Stratum.WearOS/         (Wear OS Client)
 └── Stratum.iOS/            (iOS Client)
      ├── src/Program.cs
      ├── src/AppDelegate.cs
      ├── src/SceneDelegate.cs
      ├── src/Database.cs
      ├── src/Dependencies.cs
      ├── src/Persistence/   (SQLite repositories matching Stratum.Core interfaces)
      ├── src/Storage/       (iOS Keychain wrapper)
      ├── src/Service/       (Face ID & Asset Providers)
      ├── src/View/          (AuthenticatorCell, CircularProgressView)
      └── src/ViewController/(AuthenticatorList, QrScanner, ManualAdd, Settings)
```

## 🛠️ How to Build and Run

### Option 1: On macOS / With Pair to Mac (Visual Studio / JetBrains Rider)

1. Ensure .NET 10 SDK and iOS workload are installed:
   ```bash
   dotnet workload install ios
   ```
2. Open `Stratum.sln` or build the iOS project directly:
   ```bash
   # Build for iOS Simulator (x64 / arm64)
   dotnet build Stratum.iOS/Stratum.iOS.csproj -f net10.0-ios -c Debug -r iossimulator-arm64

   # Build for Device (arm64)
   dotnet build Stratum.iOS/Stratum.iOS.csproj -f net10.0-ios -c Release -r ios-arm64
   ```

### Option 2: Building from Windows using GitHub Actions

A CI/CD workflow is provided at `.github/workflows/ios.yml`. Pushing to `master` or triggering manually via `workflow_dispatch` will build `Stratum.iOS` on a GitHub-hosted macOS runner and generate downloadable build artifacts.
