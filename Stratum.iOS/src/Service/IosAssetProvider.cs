// Copyright (C) 2026 jmh
// SPDX-License-Identifier: GPL-3.0-only

using System.IO;
using System.Threading.Tasks;
using Foundation;
using Stratum.Core;

namespace Stratum.iOS.Service
{
    public class IosAssetProvider : IAssetProvider
    {
        public async Task<byte[]> ReadBytesAsync(string path)
        {
            var bundlePath = NSBundle.MainBundle.PathForResource(
                Path.GetFileNameWithoutExtension(path),
                Path.GetExtension(path).TrimStart('.')
            ) ?? Path.Combine(NSBundle.MainBundle.ResourcePath, path);

            if (File.Exists(bundlePath))
            {
                return await File.ReadAllBytesAsync(bundlePath);
            }

            return null;
        }

        public async Task<string> ReadStringAsync(string path)
        {
            var bytes = await ReadBytesAsync(path);
            return bytes != null ? System.Text.Encoding.UTF8.GetString(bytes) : null;
        }
    }
}
