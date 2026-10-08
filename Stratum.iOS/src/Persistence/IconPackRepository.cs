// Copyright (C) 2026 jmh
// SPDX-License-Identifier: GPL-3.0-only

using Stratum.Core.Entity;
using Stratum.Core.Persistence;

namespace Stratum.iOS.Persistence
{
    public class IconPackRepository : AsyncRepository<IconPack, string>, IIconPackRepository
    {
        public IconPackRepository(Database database) : base(database)
        {
        }
    }
}
