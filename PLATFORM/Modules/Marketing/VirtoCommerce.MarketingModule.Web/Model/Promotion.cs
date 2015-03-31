﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VirtoCommerce.Domain.Common.Expressions;
using VirtoCommerce.Foundation.Frameworks;

namespace VirtoCommerce.MarketingModule.Web.Model
{
	public class Promotion : Entity, IAuditable
	{
		public DateTime CreatedDate { get; set; }
		public string CreatedBy { get; set; }
		public DateTime? ModifiedDate { get; set; }
		public string ModifiedBy { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public PromotionType Type { get; set; }

		public string Store { get; set; }
		public string Catalog { get; set; }

		public string Description { get; set; }
		public bool IsActive { get; set; }
		public int MaxUsageCount { get; set; }
		public int MaxPersonalUsageCount { get; set; }

		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }

		public IEnumerable<ExpressionElement> ExpressionElements { get; set; }
	}
}