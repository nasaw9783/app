// Copyright (C) 2026 jmh
// SPDX-License-Identifier: GPL-3.0-only

using Stratum.Core.Entity;
using Stratum.Core.Persistence;

namespace Stratum.iOS.Persistence
{
    public class CustomIconRepository : AsyncRepository<CustomIcon, string>, ICustomIconRepository
    {
        public CustomIconRepository(Database database) : base(database)
        {
        }
    }
}
