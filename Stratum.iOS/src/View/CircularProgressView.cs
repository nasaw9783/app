// Copyright (C) 2026 jmh
// SPDX-License-Identifier: GPL-3.0-only

using System;
using CoreAnimation;
using CoreGraphics;
using UIKit;

namespace Stratum.iOS.View
{
    public class CircularProgressView : UIView
    {
        private readonly CAShapeLayer _circleLayer = new();
        private readonly CAShapeLayer _progressLayer = new();
        private readonly UILabel _timeLabel = new();

        public CircularProgressView(CGRect frame) : base(frame)
        {
            Setup();
        }

        private void Setup()
        {
            BackgroundColor = UIColor.Clear;

            var center = new CGPoint(Bounds.Width / 2, Bounds.Height / 2);
            var radius = (Bounds.Width - 4) / 2;
            var startAngle = (nfloat)(-Math.PI / 2);
            var endAngle = (nfloat)(Math.PI * 1.5);

            var path = UIBezierPath.FromArc(center, radius, startAngle, endAngle, true);

            // Background track
            _circleLayer.Path = path.CGPath;
            _circleLayer.StrokeColor = UIColor.SystemGray5.CGColor;
            _circleLayer.FillColor = UIColor.Clear.CGColor;
            _circleLayer.LineWidth = 3f;
            Layer.AddSublayer(_circleLayer);

            // Active progress ring
            _progressLayer.Path = path.CGPath;
            _progressLayer.StrokeColor = UIColor.SystemBlue.CGColor;
            _progressLayer.FillColor = UIColor.Clear.CGColor;
            _progressLayer.LineWidth = 3f;
            _progressLayer.StrokeEnd = 1.0f;
            Layer.AddSublayer(_progressLayer);

            // Remaining seconds label
            _timeLabel.Frame = Bounds;
            _timeLabel.TextAlignment = UITextAlignment.Center;
            _timeLabel.Font = UIFont.MonospacedDigitSystemFontOfSize(11, UIFontWeight.Medium);
            _timeLabel.TextColor = UIColor.SecondaryLabel;
            AddSubview(_timeLabel);
        }

        public void SetProgress(float progress, int remainingSeconds)
        {
            _progressLayer.StrokeEnd = (nfloat)Math.Clamp(progress, 0.0, 1.0);
            _timeLabel.Text = remainingSeconds.ToString();

            if (remainingSeconds <= 5)
            {
                _progressLayer.StrokeColor = UIColor.SystemRed.CGColor;
                _timeLabel.TextColor = UIColor.SystemRed;
            }
            else
            {
                _progressLayer.StrokeColor = UIColor.SystemBlue.CGColor;
                _timeLabel.TextColor = UIColor.SecondaryLabel;
            }
        }
    }
}
