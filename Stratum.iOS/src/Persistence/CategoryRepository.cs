// Copyright (C) 2026 jmh
// SPDX-License-Identifier: GPL-3.0-only

using Stratum.Core.Entity;
using Stratum.Core.Persistence;

namespace Stratum.iOS.Persistence
{
    public class CategoryRepository : AsyncRepository<Category, string>, ICategoryRepository
    {
        public CategoryRepository(Database database) : base(database)
        {
        }
    }
}
