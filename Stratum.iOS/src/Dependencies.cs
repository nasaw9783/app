// Copyright (C) 2026 jmh
// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.Generic;
using Autofac;
using Stratum.Core;
using Stratum.Core.Backup.Encryption;
using Stratum.Core.Comparer;
using Stratum.Core.Entity;
using Stratum.Core.Persistence;
using Stratum.Core.Service;
using Stratum.Core.Service.Impl;
using Stratum.iOS.Persistence;
using Stratum.iOS.Service;
using Stratum.iOS.Storage;

namespace Stratum.iOS
{
    public static class Dependencies
    {
        private static IContainer _container;

        public static void Init(Database database)
        {
            _container = CreateContainer(database);
        }

        public static IContainer CreateContainer(Database database)
        {
            var builder = new ContainerBuilder();
            RegisterAll(builder, database);
            return builder.Build();
        }

        private static void RegisterAll(ContainerBuilder builder, Database database)
        {
            builder.RegisterInstance(database).SingleInstance().ExternallyOwned();

            builder.RegisterType<IosSecureStorage>().SingleInstance();
            builder.RegisterType<IosBiometricService>().SingleInstance();
            builder.RegisterType<IosAssetProvider>().As<IAssetProvider>().SingleInstance();
            builder.RegisterType<IosIconResolver>().As<IIconResolver>().SingleInstance();

            builder.RegisterType<StrongBackupEncryption>().As<IBackupEncryption>().SingleInstance();
            builder.RegisterType<LegacyBackupEncryption>().As<IBackupEncryption>().SingleInstance();
            builder.RegisterType<NoBackupEncryption>().As<IBackupEncryption>().SingleInstance();

            RegisterRepositories(builder);
            RegisterServices(builder);
        }

        private static void RegisterRepositories(ContainerBuilder builder)
        {
            builder.RegisterType<AuthenticatorRepository>().As<IAuthenticatorRepository>().SingleInstance();
            builder.RegisterType<CategoryRepository>().As<ICategoryRepository>().SingleInstance();
            builder.RegisterType<AuthenticatorCategoryRepository>().As<IAuthenticatorCategoryRepository>().SingleInstance();
            builder.RegisterType<CustomIconRepository>().As<ICustomIconRepository>().SingleInstance();
            builder.RegisterType<IconPackRepository>().As<IIconPackRepository>().SingleInstance();
            builder.RegisterType<IconPackEntryRepository>().As<IIconPackEntryRepository>().SingleInstance();
        }

        private static void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<AuthenticatorComparer>().As<IEqualityComparer<Authenticator>>().SingleInstance();
            builder.RegisterType<CategoryComparer>().As<IEqualityComparer<Category>>().SingleInstance();
            builder.RegisterType<AuthenticatorCategoryComparer>().As<IEqualityComparer<AuthenticatorCategory>>().SingleInstance();

            builder.RegisterType<AuthenticatorService>().As<IAuthenticatorService>().SingleInstance();
            builder.RegisterType<BackupService>().As<IBackupService>().SingleInstance();
            builder.RegisterType<CategoryService>().As<ICategoryService>().SingleInstance();
            builder.RegisterType<CustomIconService>().As<ICustomIconService>().SingleInstance();
            builder.RegisterType<IconPackService>().As<IIconPackService>().SingleInstance();
            builder.RegisterType<ImportService>().As<IImportService>().SingleInstance();
            builder.RegisterType<RestoreService>().As<IRestoreService>().SingleInstance();
        }

        public static T Resolve<T>() where T : class
        {
            return _container.Resolve<T>();
        }

        public static IEnumerable<T> ResolveAll<T>() where T : class
        {
            return _container.Resolve<IEnumerable<T>>();
        }
    }
}
