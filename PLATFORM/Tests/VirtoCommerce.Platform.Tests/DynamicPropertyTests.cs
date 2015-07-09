using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Data.DynamicProperties;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using VirtoCommerce.Platform.Data.Repositories;

namespace VirtoCommerce.Platform.Tests
{
    public class WithDynamicProperties : IHasDynamicProperties
    {
        public ICollection<DynamicProperty> DynamicProperties { get; set; }
    }

    [TestClass]
    public class DynamicPropertyTests
    {
        [TestMethod]
        public void GetObjectTypes()
        {
            var service = GetDynamicPropertyService();
            var typeNames = service.GetObjectTypes();
        }

        [TestMethod]
        public void SaveLoadPropertiesAndValues()
        {
            var service = GetDynamicPropertyService();

            // Delete existing properties
            var existingTypeProperties = service.GetProperties("TestObjectType");
            var propertyIds = existingTypeProperties.Select(p => p.Id).ToArray();
            service.DeleteProperties(propertyIds);

            // Create properties
            var typeProperties = new[]
            {
                new DynamicProperty
                {
                    ObjectType = "TestObjectType",
                    Name = "Color",
                    ValueType = DynamicPropertyValueType.ShortText,
                    DisplayNames = new[]
                    {
                        new DynamicPropertyName
                        {
                            Locale = "en-US", Name = "Color"
                        },
                        new DynamicPropertyName
                        {
                            Locale = "ru-RU", Name = "Цвет"
                        },
                    },
                    IsDictionary = true,
                    DictionaryItems = new[]
                    {
                        new DynamicPropertyDictionaryItem
                        {
                            Name = "Red",
                            DictionaryValues = new[]
                            {
                                new DynamicPropertyDictionaryValue
                                {
                                    Locale = "en-US", Value = "Red"
                                },
                                new DynamicPropertyDictionaryValue
                                {
                                    Locale = "ru-RU", Value = "Красный"
                                },
                            },
                        },
                        new DynamicPropertyDictionaryItem
                        {
                            Name = "Blue",
                            DictionaryValues = new[]
                            {
                                new DynamicPropertyDictionaryValue
                                {
                                    Locale = "en-US", Value = "Blue"
                                },
                                new DynamicPropertyDictionaryValue
                                {
                                    Locale = "ru-RU", Value = "Синий"
                                },
                            },
                        },
                    },
                },
                new DynamicProperty
                {
                    ObjectType = "TestObjectType",
                    Name = "SingleValueProperty",
                    ValueType = DynamicPropertyValueType.ShortText,
                    DisplayNames = new[]
                    {
                        new DynamicPropertyName
                        {
                            Locale = "en-US", Name = "Single Value Property"
                        },
                        new DynamicPropertyName
                        {
                            Locale = "ru-RU", Name = "Свойство с одним значением"
                        },
                    },
                },
                new DynamicProperty
                {
                    ObjectType = "TestObjectType",
                    Name = "Array",
                    ValueType = DynamicPropertyValueType.ShortText,
                    IsArray = true,
                },
            };

            service.SaveProperties(typeProperties);

            existingTypeProperties = service.GetProperties("TestObjectType");
            var arrayProperty = existingTypeProperties.First(p => p.Name == "Array");
            var singleValueProperty = existingTypeProperties.First(p => p.Name == "SingleValueProperty");

            var colorProperty = existingTypeProperties.First(p => p.Name == "Color");
            var redColor = colorProperty.DictionaryItems.First(i => i.Name == "Red");
            var blueColor = colorProperty.DictionaryItems.First(i => i.Name == "Blue");

            // Rename property
            var renamedProperties = new[] { existingTypeProperties[0] };
            var originalName = renamedProperties[0].Name;
            renamedProperties[0].Name = "NewName";
            service.SaveProperties(renamedProperties);

            // Return original name
            renamedProperties[0].Name = originalName;
            service.SaveProperties(renamedProperties);

            var objectProperties = new[]
            {
                new DynamicProperty
                {
                    Id = colorProperty.Id,
                    ObjectId = "111",
                    ObjectValues = new[] { new DynamicPropertyObjectValue { DictionaryItemId = redColor.Id } },
                },
                new DynamicProperty
                {
                    Id = colorProperty.Id,
                    ObjectId = "222",
                    ObjectValues = new[] { new DynamicPropertyObjectValue { DictionaryItemId = blueColor.Id } },
                },
                new DynamicProperty
                {
                    Id = singleValueProperty.Id,
                    ObjectType = "TestObjectType",
                    ObjectId = "111",
                    ValueType = DynamicPropertyValueType.ShortText,
                    ObjectValues = new[]
                    {
                        new DynamicPropertyObjectValue
                        {
                            Locale = "en-US",
                            Value = "Black",
                        },
                        new DynamicPropertyObjectValue
                        {
                            Locale = "ru-RU",
                            Value = "Чёрный",
                        },
                    },
                },
                new DynamicProperty
                {
                    Id = singleValueProperty.Id,
                    ObjectType = "TestObjectType",
                    ObjectId = "222",
                    ValueType = DynamicPropertyValueType.ShortText,
                    ObjectValues = new[]
                    {
                        new DynamicPropertyObjectValue
                        {
                            Locale = "en-US",
                            Value = "White",
                        },
                        new DynamicPropertyObjectValue
                        {
                            Locale = "ru-RU",
                            Value = "Белый",
                        },
                    },
                },
                new DynamicProperty
                {
                    Id = arrayProperty.Id,
                    ObjectType = "TestObjectType",
                    ObjectId = "222",
                    ValueType = DynamicPropertyValueType.ShortText,
                    IsArray = true,
                    ObjectValues = new[]
                    {
                        new DynamicPropertyObjectValue
                        {
                            Locale = "en-US",
                            ArrayValues = new[]
                            {
                                "qwerty",
                                "asdfgh",
                            }
                        },
                        new DynamicPropertyObjectValue
                        {
                            Locale = "ru-RU",
                            ArrayValues = new[]
                            {
                                "йцукен",
                                "фывапр",
                            }
                        },
                    },
                },
            };

            service.SaveObjectValues(objectProperties);

            var objectProperties1 = service.GetObjectValues("TestObjectType", "111");
            var objectProperties2 = service.GetObjectValues("TestObjectType", "222");
        }

        private IDynamicPropertyService GetDynamicPropertyService()
        {
            return new DynamicPropertyService(() => new PlatformRepository("VirtoCommerce", new EntityPrimaryKeyGeneratorInterceptor(), new AuditableInterceptor()));
        }
    }
}
