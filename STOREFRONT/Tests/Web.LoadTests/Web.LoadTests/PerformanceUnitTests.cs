using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtoCommerce.ApiClient;
using VirtoCommerce.ApiClient.Extensions;

namespace Web.LoadTests
{
    [TestClass]
    public class PerformanceUnitTests
    {
        [TestMethod]
        public void LoadAllStores()
        {
            for (int index = 0; index < 500; index++)
            {
                var client = ClientContext.Clients.CreateStoreClient();
                var stores = client.GetStoresAsync().Result;
            }
            //var stores2 = client.GetStoresAsync().Result;
        }
    }
}
