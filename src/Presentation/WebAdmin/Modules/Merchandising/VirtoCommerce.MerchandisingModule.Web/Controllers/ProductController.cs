﻿using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using VirtoCommerce.CatalogModule.Services;
using VirtoCommerce.Foundation.Catalogs.Search;
using VirtoCommerce.MerchandisingModule.Web.Model;
using VirtoCommerce.MerchandisingModule.Web.Converters;
using VirtoCommerce.Foundation.Search.Schemas;
using VirtoCommerce.Foundation.Search;
using System.Web.Http.Description;
using moduleModel = VirtoCommerce.CatalogModule.Model;
using webModel = VirtoCommerce.MerchandisingModule.Web.Model;
using VirtoCommerce.MerchandisingModule.Web.Binders;
using Microsoft.Practices.Unity;

namespace VirtoCommerce.MerchandisingModule.Web.Controllers
{
	[RoutePrefix("api/mp/{catalog}/{language}/products")]
	public class ProductController : ApiController
	{
		private readonly IItemService _itemService;
		private readonly ISearchProvider _searchService;
		private readonly ISearchConnection _searchConnection;

		public ProductController(IItemService itemService,
									   ISearchProvider indexedSearchProvider,
									   ISearchConnection searchConnection)
		{
			_searchService = indexedSearchProvider;
			_searchConnection = searchConnection;
			_itemService = itemService;
		}

		/// <summary>
		/// GET: api/mp/apple/en-us/products?q='some keyword'&outline=apple/catalog
		/// </summary>
		/// <param name="catalog"></param>
		/// <param name="criteria"></param>
		/// <param name="language"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("")]
		[ResponseType(typeof(webModel.GenericSearchResult<webModel.CatalogItem>))]
		public IHttpActionResult Search(string catalog, [ModelBinder(typeof(CatalogItemSearchCriteriaBinder))] CatalogItemSearchCriteria criteria,
										string language = "en-us")
		{
			criteria.Locale = language;
			criteria.Catalog = catalog;
			var result = _searchService.Search(_searchConnection.Scope, criteria) as SearchResults;
			var items = result.GetKeyAndOutlineFieldValueMap<string>();

			var retVal = new webModel.GenericSearchResult<webModel.CatalogItem>();
			retVal.TotalCount = result.TotalCount;
			//Load products 
			foreach (var productId in items.Keys)
			{
				var product = _itemService.GetById(productId, moduleModel.ItemResponseGroup.ItemAssets | moduleModel.ItemResponseGroup.ItemInfo);
				if (product != null)
				{
					var webModelProduct = product.ToWebModel();

					var searchTags = items[productId];

					webModelProduct.Outline = searchTags[criteria.OutlineField].ToString().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
															   .FirstOrDefault(x => x.StartsWith(criteria.Catalog, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
					retVal.Items.Add(webModelProduct);
				}
			}
			return Ok(retVal);
		}

		[HttpGet]
		[ResponseType(typeof(webModel.Product))]
		[Route("{productId}")]
		public IHttpActionResult GetProduct(string productId)
		{
			var product = _itemService.GetById(productId, moduleModel.ItemResponseGroup.ItemLarge);
			var retVal = product.ToWebModel();
			return Ok(retVal);
		}
	}
}