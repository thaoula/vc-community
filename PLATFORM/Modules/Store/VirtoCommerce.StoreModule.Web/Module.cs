﻿using System;
using System.Data.Entity;
using System.Linq;
using Microsoft.Practices.Unity;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using VirtoCommerce.StoreModule.Data.Repositories;
using VirtoCommerce.StoreModule.Data.Services;
using VirtoCommerce.StoreModule.Web.ExportImport;

namespace VirtoCommerce.StoreModule.Web
{
    public class Module : ModuleBase, ISupportExportImportModule
    {
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

        public override void SetupDatabase(SampleDataLevel sampleDataLevel)
        {
            using (var db = new StoreRepositoryImpl("VirtoCommerce"))
            {
                IDatabaseInitializer<StoreRepositoryImpl> initializer;

                switch (sampleDataLevel)
                {
                    case SampleDataLevel.Full:
                    case SampleDataLevel.Reduced:
                        initializer = new SqlStoreSampleDatabaseInitializer(_container.Resolve<ISettingsManager>());
                        break;
                    default:
                        initializer = new SetupDatabaseInitializer<StoreRepositoryImpl, Data.Migrations.Configuration>();
                        break;
                }

                initializer.InitializeDatabase(db);
            }
        }

        public override void Initialize()
        {
            _container.RegisterType<IStoreRepository>(new InjectionFactory(c => new StoreRepositoryImpl("VirtoCommerce", new EntityPrimaryKeyGeneratorInterceptor(), new AuditableInterceptor())));
            _container.RegisterType<IStoreService, StoreServiceImpl>();
        }

        public override void PostInitialize()
        {
			var cacheManager = _container.Resolve<CacheManager>();
			var cacheSettings = new[] 
			{
				new CacheSettings("StoreModule", TimeSpan.FromHours(1))
			};
			cacheManager.AddCacheSettings(cacheSettings);
        }
        #endregion

        #region ISupportExportModule Members

		public void DoExport(System.IO.Stream outStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var exportJob = _container.Resolve<StoreExportImport>();
            exportJob.DoExport(outStream, progressCallback);
        }

		public void DoImport(System.IO.Stream inputStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var exportJob = _container.Resolve<StoreExportImport>();
            exportJob.DoImport(inputStream, progressCallback);
        }

		public string ExportDescription
		{
			get 
			{ 
				var settingManager = _container.Resolve<ISettingsManager>();
				return settingManager.GetValue("Stores.ExportImport.Description", String.Empty);
			}
		}

		#endregion
	}
}
