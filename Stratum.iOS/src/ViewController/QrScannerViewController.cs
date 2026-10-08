// Copyright (C) 2026 jmh
// SPDX-License-Identifier: GPL-3.0-only

using System;
using AVFoundation;
using CoreGraphics;
using Foundation;
using UIKit;
using Stratum.Core;
using Stratum.Core.Service;
using Serilog;

namespace Stratum.iOS.ViewController
{
    public class QrScannerViewController : UIViewController, IAVCaptureMetadataOutputObjectsDelegate
    {
        private readonly ILogger _log = Log.ForContext<QrScannerViewController>();
        private AVCaptureSession _captureSession;
        private AVCaptureVideoPreviewLayer _previewLayer;
        private bool _isScanned;

        public event Action OnAuthenticatorAdded;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "Scan QR Code";
            View.BackgroundColor = UIColor.Black;

            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(
                UIBarButtonSystemItem.Cancel,
                (s, e) => DismissViewController(true, null)
            );

            SetupCamera();
        }

        private void SetupCamera()
        {
            _captureSession = new AVCaptureSession();

            var videoCaptureDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
            if (videoCaptureDevice == null)
            {
                ShowError("Camera not available on this device.");
                return;
            }

            NSError error;
            var videoInput = new AVCaptureDeviceInput(videoCaptureDevice, out error);
            if (error != null || !_captureSession.CanAddInput(videoInput))
            {
                ShowError("Unable to initialize camera input.");
                return;
            }

            _captureSession.AddInput(videoInput);

            var metadataOutput = new AVCaptureMetadataOutput();
            if (_captureSession.CanAddOutput(metadataOutput))
            {
                _captureSession.AddOutput(metadataOutput);

                metadataOutput.SetDelegate(this, CoreFoundation.DispatchQueue.MainQueue);
                metadataOutput.MetadataObjectTypes = AVMetadataObjectType.QRCode;
            }
            else
            {
                ShowError("Unable to initialize QR code reader.");
                return;
            }

            _previewLayer = new AVCaptureVideoPreviewLayer(_captureSession)
            {
                Frame = View.Layer.Bounds,
                VideoGravity = AVLayerVideoGravity.ResizeAspectFill
            };
            View.Layer.AddSublayer(_previewLayer);

            _captureSession.StartRunning();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            if (_previewLayer != null)
            {
                _previewLayer.Frame = View.Layer.Bounds;
            }
        }

        [Export("captureOutput:didOutputMetadataObjects:fromConnection:")]
        public void DidOutputMetadataObjects(AVCaptureMetadataOutput captureOutput, AVMetadataObject[] metadataObjects, AVCaptureConnection connection)
        {
            if (_isScanned || metadataObjects == null || metadataObjects.Length == 0)
            {
                return;
            }

            if (metadataObjects[0] is AVMetadataMachineReadableCodeObject readableObject &&
                readableObject.Type == AVMetadataObjectType.QRCode)
            {
                var stringValue = readableObject.StringValue;
                if (!string.IsNullOrEmpty(stringValue) && stringValue.StartsWith("otpauth://", StringComparison.OrdinalIgnoreCase))
                {
                    _isScanned = true;
                    ProcessQrCode(stringValue);
                }
            }
        }

        private async void ProcessQrCode(string qrContent)
        {
            _captureSession?.StopRunning();

            var feedback = new UINotificationFeedbackGenerator();
            feedback.Prepare();

            try
            {
                var parseResult = UriParser.Parse(qrContent);
                if (parseResult?.Authenticator != null)
                {
                    var authService = Dependencies.Resolve<IAuthenticatorService>();
                    await authService.AddAsync(parseResult.Authenticator);

                    feedback.NotificationOccurred(UINotificationFeedbackType.Success);

                    var alert = UIAlertController.Create(
                        "Success",
                        $"Added {parseResult.Authenticator.Username ?? parseResult.Authenticator.Issuer}",
                        UIAlertControllerStyle.Alert
                    );
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, _ =>
                    {
                        OnAuthenticatorAdded?.Invoke();
                        DismissViewController(true, null);
                    }));

                    PresentViewController(alert, true, null);
                    return;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to parse OTP QR code");
            }

            feedback.NotificationOccurred(UINotificationFeedbackType.Error);
            ShowError("Invalid 2FA QR Code. Please check the code and try again.");
            _isScanned = false;
            _captureSession?.StartRunning();
        }

        private void ShowError(string message)
        {
            var alert = UIAlertController.Create("Scan Error", message, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
            PresentViewController(alert, true, null);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (_captureSession != null && _captureSession.IsRunning)
            {
                _captureSession.StopRunning();
            }
        }
    }
}
