// Copyright (C) 2026 jmh
// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Threading.Tasks;
using Foundation;
using LocalAuthentication;
using Serilog;

namespace Stratum.iOS.Service
{
    public class IosBiometricService
    {
        private readonly ILogger _log = Log.ForContext<IosBiometricService>();

        public bool CanAuthenticate()
        {
            var context = new LAContext();
            return context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out _);
        }

        public LABiometryType GetBiometryType()
        {
            var context = new LAContext();
            if (context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out _))
            {
                return context.BiometryType;
            }
            return LABiometryType.None;
        }

        public async Task<bool> AuthenticateAsync(string reason)
        {
            var context = new LAContext();
            NSError authError;

            if (context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out authError))
            {
                try
                {
                    var result = await context.EvaluatePolicyAsync(
                        LAPolicy.DeviceOwnerAuthenticationWithBiometrics,
                        reason
                    );

                    if (!result.Item1 && result.Item2 != null)
                    {
                        _log.Warning("Biometric authentication failed: {Message}", result.Item2.LocalizedDescription);
                    }

                    return result.Item1;
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Biometric authentication error");
                    return false;
                }
            }

            _log.Information("Biometrics not available on this device");
            return false;
        }
    }
}
