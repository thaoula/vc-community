﻿using System;
using System.Management.Automation;
using VirtoCommerce.Foundation.Frameworks;
using VirtoCommerce.PowerShell.AppConfig;
using VirtoCommerce.Foundation.Data.AppConfig;

namespace VirtoCommerce.PowerShell.DatabaseSetup.Cmdlet
{
	[CLSCompliant(false)]
	[Cmdlet(VerbsData.Publish, "Virto-AppConfig-Database", SupportsShouldProcess = true, DefaultParameterSetName = "DbConnection")]
	public class PublishAppConfigDatabase : DatabaseCommand
	{
        public override void Publish(string dbconnection, string data, bool sample, string strategy = SqlDbConfiguration.SqlAzureExecutionStrategy)
		{
			base.Publish(dbconnection, data, sample, strategy);
			PublishWithScope(dbconnection, data, sample, null);
		}

		public void PublishWithScope(string dbconnection, string data, bool sample, string scope)
		{
			string connection = dbconnection;
			SafeWriteDebug("ConnectionString: " + connection);
           
			using (var db = new EFAppConfigRepository(connection))
			{
				SqlAppConfigDatabaseInitializer initializer;

				if (sample)
				{
					SafeWriteVerbose("Running sample scripts");
					initializer = new SqlAppConfigSampleDatabaseInitializer();
				}
				else
				{
					SafeWriteVerbose("Running minimum scripts");
					initializer = new SqlAppConfigDatabaseInitializer();
				}

				initializer.Scope = scope;
				initializer.InitializeDatabase(db);
			}
		}
	}
}
