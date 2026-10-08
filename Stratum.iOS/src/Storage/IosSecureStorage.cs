// Copyright (C) 2026 jmh
// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Text;
using Foundation;
using Security;
using Serilog;

namespace Stratum.iOS.Storage
{
    public class IosSecureStorage
    {
        private const string ServiceName = "com.stratumauth.ios.keychain";
        private readonly ILogger _log = Log.ForContext<IosSecureStorage>();

        public string Get(string key)
        {
            try
            {
                var record = new SecRecord(SecKind.GenericPassword)
                {
                    Service = ServiceName,
                    Account = key
                };

                var match = SecKeyChain.QueryAsRecord(record, out var result);
                if (result == SecStatusCode.Success && match != null && match.ValueData != null)
                {
                    return NSString.FromData(match.ValueData, NSStringEncoding.UTF8).ToString();
                }

                return null;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to read secure key {Key}", key);
                return null;
            }
        }

        public void Set(string key, string value)
        {
            try
            {
                var record = new SecRecord(SecKind.GenericPassword)
                {
                    Service = ServiceName,
                    Account = key
                };

                // Check if existing record exists
                SecKeyChain.Remove(record);

                if (value != null)
                {
                    record.ValueData = NSData.FromString(value, NSStringEncoding.UTF8);
                    record.Accessible = SecAccessible.WhenUnlockedThisDeviceOnly;

                    var result = SecKeyChain.Add(record);
                    if (result != SecStatusCode.Success)
                    {
                        _log.Warning("Failed to add keychain item: {Status}", result);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to set secure key {Key}", key);
            }
        }

        public void Remove(string key)
        {
            try
            {
                var record = new SecRecord(SecKind.GenericPassword)
                {
                    Service = ServiceName,
                    Account = key
                };

                SecKeyChain.Remove(record);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to remove secure key {Key}", key);
            }
        }
    }
}
