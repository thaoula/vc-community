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
                    Name = "SingleValueProperty",
                    ValueType = DynamicPropertyValueType.ShortText,
                    LocalizedNames = new[]
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
                    Name = "ArrayProperty",
                    ValueType = DynamicPropertyValueType.ShortText,
                    IsArray = true,
                },
            };

            service.SaveProperties(typeProperties);

            existingTypeProperties = service.GetProperties("TestObjectType");
            var singleValueProperty = existingTypeProperties.First(p => p.Name == "SingleValueProperty");
            var arrayProperty = existingTypeProperties.First(p => p.Name == "ArrayProperty");

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
                    Id = singleValueProperty.Id,
                    ObjectType = "TestObjectType",
                    ObjectId = "111",
                    ValueType = DynamicPropertyValueType.ShortText,
                    Values = new[]
                    {
                        new DynamicPropertyValue
                        {
                            Locale = "en-US",
                            Value = "Black",
                        },
                        new DynamicPropertyValue
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
                    Values = new[]
                    {
                        new DynamicPropertyValue
                        {
                            Locale = "en-US",
                            Value = "White",
                        },
                        new DynamicPropertyValue
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
                    Values = new[]
                    {
                        new DynamicPropertyValue
                        {
                            Locale = "en-US",
                            ArrayValues = new[]
                            {
                                "qwerty",
                                "asdfgh",
                            }
                        },
                        new DynamicPropertyValue
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

            service.SaveObjectProperties(objectProperties);

            var objectProperties1 = service.GetObjectProperties("TestObjectType", "111");
            var objectProperties2 = service.GetObjectProperties("TestObjectType", "222");
        }

        private IDynamicPropertyService GetDynamicPropertyService()
        {
            return new DynamicPropertyService(() => new PlatformRepository("VirtoCommerce", new EntityPrimaryKeyGeneratorInterceptor(), new AuditableInterceptor()));
        }
    }
}
