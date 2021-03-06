﻿#region
using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.ApiClient.DataContracts.Cart;
using VirtoCommerce.Web.Models;
using Data = VirtoCommerce.ApiClient.DataContracts.Cart;

#endregion

namespace VirtoCommerce.Web.Convertors
{
    public static class CartConverters
    {
        #region Public Methods and Operators
        public static Data.ShoppingCart AsServiceModel(this Cart cart, string currency)
        {
            var ret = new Data.ShoppingCart { Id = cart.Key };
            ret.Currency = currency;

            if (cart.Items != null && cart.Items.Any())
            {
                ret.Items = new List<Data.CartItem>(cart.Items.Select(x => x.AsServiceModel()));
                //cart.Items.ForEach(i => ret.Items.Add(i.AsServiceModel()));
            }
            else
            {
                ret.Items = new List<Data.CartItem>();
            }

            return ret;
        }

        public static Data.CartItem AsServiceModel(this LineItem item)
        {
            var ret = new Data.CartItem
                      {
                          Id = item.Id,
                          ProductCode = item.Handle,
                          ProductId = item.ProductId,
                          Name = item.Title,
                          PlacedPrice = item.Price,
                          RequiredShipping = item.RequiresShipping,
                          ExtendedPrice = item.Price,
                          Quantity = item.Quantity,
                          ImageUrl = item.Image,
                          CategoryId = "fake",
                          CatalogId = "fake",
                          TaxIncluded = item.Taxable,
                          TaxTotal = item.TaxAmount,
                          TaxType = item.TaxType
                      };

            return ret;
        }

        public static Cart AsWebModel(this Data.ShoppingCart cart)
        {
            var ret = new Cart { Key = cart.Id };

            if (cart.Items != null && cart.Items.Any())
            {
                ret.Items.AddRange(cart.Items.Select(x => x.AsWebModel()));
            }

            return ret;
        }

        public static Checkout AsCheckoutWebModel(
            this Data.ShoppingCart cart, VirtoCommerce.ApiClient.DataContracts.Orders.CustomerOrder order = null)
        {
            var checkoutModel = new Checkout();

            checkoutModel.TaxLines = new List<TaxLine>();

            if (cart.Addresses != null)
            {
                var billingAddress = cart.Addresses.FirstOrDefault(a => a.Type == AddressType.Billing);
                if (billingAddress != null)
                {
                    checkoutModel.BillingAddress = billingAddress.AsCartWebModel();
                    checkoutModel.Email = billingAddress.Email;
                }

                var shippingAddress = cart.Addresses.FirstOrDefault(a => a.Type == AddressType.Shipping);
                if (shippingAddress != null)
                {
                    checkoutModel.ShippingAddress = shippingAddress.AsCartWebModel();
                    checkoutModel.Email = shippingAddress.Email;
                }
            }

            checkoutModel.BuyerAcceptsMarketing = true;
            checkoutModel.Currency = cart.Currency;
            checkoutModel.CustomerId = cart.CustomerId;

            if (cart.Discounts != null)
            {
                checkoutModel.Discounts = new List<VirtoCommerce.Web.Models.Discount>();

                foreach (var discount in cart.Discounts)
                {
                    checkoutModel.Discounts.Add(new VirtoCommerce.Web.Models.Discount
                    {
                        Amount = (decimal)discount.DiscountAmount,
                        Code = discount.PromotionId,
                        Id = discount.Id
                    });
                }
            }

            checkoutModel.Email = "";

            // TODO: Gift cards

            checkoutModel.Id = cart.Id;

            if (cart.Items != null)
            {
                checkoutModel.LineItems = new List<LineItem>();

                foreach (var item in cart.Items)
                {
                    checkoutModel.LineItems.Add(item.AsWebModel());
                }

                var taxableItems = cart.Items;//.Where(i => i.TaxIncluded);
                if (taxableItems.Count() > 0)
                {
                    var lineItemsTax = new TaxLine
                    {
                        Title = "Line items",
                        Price = taxableItems.Sum(i => i.TaxTotal),
                        Rate = taxableItems.Where(i => i.TaxDetails != null).Sum(i => i.TaxDetails.Sum(td => td.Rate))
                    };

                    if (checkoutModel.TaxLines == null)
                    {
                        checkoutModel.TaxLines = new List<TaxLine>();
                    }

                    checkoutModel.TaxLines.Add(lineItemsTax);
                }
            }

            checkoutModel.Name = cart.Name;
            //checkoutModel.Note = cart.Note;

            if (order != null)
            {
                checkoutModel.Order = order.AsWebModel();
            }

            if (cart.Payments != null)
            {
                var payment = cart.Payments.FirstOrDefault(); // Remake for multipayment

                if (payment != null)
                {
                    checkoutModel.PaymentMethod = new Models.PaymentMethod
                    {
                        Code = payment.PaymentGatewayCode
                    };
                }
            }

            if (cart.Shipments != null)
            {
                var shipment = cart.Shipments.FirstOrDefault(); // Remake for multishipment

                if (shipment != null)
                {
                    checkoutModel.ShippingMethod = new ShippingMethod
                    {
                        Handle = shipment.ShipmentMethodCode,
                        Price = shipment.ShippingPrice,
                        Title = shipment.ShipmentMethodCode,
                        TaxTotal = shipment.TaxTotal,
                        TaxType = shipment.TaxType
                    };
                }

                var taxableShipments = cart.Shipments;//.Where(s => s.TaxIncluded);
                if (taxableShipments.Count() > 0)
                {
                    var shippingTax = new TaxLine
                    {
                        Title = "Shipping",
                        Price = cart.Shipments.Sum(s => s.TaxTotal),
                        Rate = taxableShipments.Where(s => s.TaxDetails != null).Sum(i => i.TaxDetails.Sum(td => td.Rate))
                    };

                    if (checkoutModel.TaxLines == null)
                    {
                        checkoutModel.TaxLines = new List<TaxLine>();
                    }

                    checkoutModel.TaxLines.Add(shippingTax);
                }
            }

            // Transactions

            return checkoutModel;
        }

        public static VirtoCommerce.Web.Models.PaymentMethod AsWebModel(this Data.PaymentMethod paymentMethod)
        {
            return new VirtoCommerce.Web.Models.PaymentMethod
            {
                Code = paymentMethod.GatewayCode,
                Description = paymentMethod.Description,
                LogoUrl = paymentMethod.IconUrl,
                Title = paymentMethod.Name,
                Priority = paymentMethod.Priority,
                Type = paymentMethod.Type,
                Group = paymentMethod.Group
            };
        }

        public static Data.ShoppingCart AsServiceModel(this Checkout checkoutModel, ref Data.ShoppingCart cart)
        {
            cart.Addresses = new List<Data.Address>();
            if (checkoutModel.BillingAddress != null)
            {
                var billingAddress = checkoutModel.BillingAddress.AsCartServiceModel();
                billingAddress.Email = checkoutModel.Email;
                billingAddress.Type = AddressType.Billing;

                cart.Addresses.Add(billingAddress);
            }
            if (checkoutModel.ShippingAddress != null)
            {
                var shippingAddress = checkoutModel.ShippingAddress.AsCartServiceModel();
                shippingAddress.Email = checkoutModel.Email;
                shippingAddress.Type = AddressType.Shipping;

                cart.Addresses.Add(shippingAddress);
            }

            cart.Currency = checkoutModel.Currency;
            cart.CustomerId = checkoutModel.CustomerId;

            // DISCOUNTS

            if (checkoutModel.LineItems != null)
            {
                cart.Items = new List<CartItem>();
                foreach (var lineItemModel in checkoutModel.LineItems)
                {
                    cart.Items.Add(lineItemModel.AsServiceModel());
                }
            }

            //cart.Note = checkoutModel.Note;

            if (checkoutModel.PaymentMethod != null)
            {
                cart.Payments = new List<Payment>();
                cart.Payments.Add(new Payment
                {
                    Currency = checkoutModel.Currency,
                    PaymentGatewayCode = checkoutModel.PaymentMethod.Code,
                    Amount = checkoutModel.TotalPrice
                });
            }

            if (checkoutModel.ShippingMethod != null)
            {
                cart.Shipments = new List<Shipment>();
                cart.Shipments.Add(new Shipment
                {
                    Currency = checkoutModel.Currency,
                    ShipmentMethodCode = checkoutModel.ShippingMethod.Handle,
                    ShippingPrice = checkoutModel.ShippingMethod.Price,
                    TaxType = checkoutModel.ShippingMethod.TaxType
                });
            }

            cart.ShippingTotal = checkoutModel.ShippingPrice;
            cart.SubTotal = checkoutModel.SubtotalPrice;
            cart.TaxTotal = checkoutModel.TaxPrice;
            cart.Total = checkoutModel.TotalPrice;

            return cart;
        }

        public static LineItem AsWebModel(this Data.CartItem item)
        {
            var product = new Product
                          {
                              Title = item.Name,
                              Description = item.Name,
                              Excerpt = item.Name,
                              Handle = item.ProductId
                          };

            var variant = new Variant();

            var ret = new LineItem
                      {
                          Id = item.Id,
                          Handle = item.ProductCode,
                          ProductId = item.ProductId,
                          Title = item.Name,
                          Price = item.PlacedPrice,
                          Quantity = item.Quantity,
                          RequiresShipping = item.RequiredShipping,
                          Sku = item.ProductCode,
                          VariantId = item.ProductId,
                          Image = item.ImageUrl,
                          Variant = variant,
                          Product = product,
                          Url = String.Format("/products/{0}", item.ProductId),
                          Taxable = item.TaxIncluded,
                          TaxAmount = item.TaxTotal,
                          TaxType = item.TaxType
                      };

            return ret;
        }

        public static CustomerAddress AsCartWebModel(this Address address)
        {
            return new CustomerAddress
            {
                Address1 = address.Line1,
                Address2 = address.Line2,
                City = address.City,
                Company = address.Organization,
                Country = address.CountryName,
                CountryCode = address.CountryCode,
                ProvinceCode = address.RegionId,
                Province = address.RegionName,
                FirstName = address.FirstName,
                LastName = address.LastName,
                Phone = address.Phone,
                Zip = address.PostalCode
            };
        }

        public static Address AsCartServiceModel(this CustomerAddress customerAddress)
        {
            return new Address
            {
                City = customerAddress.City,
                CountryCode = customerAddress.CountryCode,
                CountryName = customerAddress.Country,
                RegionName = customerAddress.Province,
                FirstName = customerAddress.FirstName,
                LastName = customerAddress.LastName,
                Line1 = customerAddress.Address1,
                Line2 = customerAddress.Address2,
                Organization = customerAddress.Company,
                Phone = customerAddress.Phone,
                PostalCode = customerAddress.Zip,
                Zip = customerAddress.Zip // TODO: Resolve
            };
        }
        #endregion
    }
}