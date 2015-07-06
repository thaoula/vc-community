using System;
using System.Collections.Generic;
using System.Globalization;
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
        public void SaveLoadTypeProperties()
        {
            var typeProperties = new[]
            {
                new DynamicProperty
                {
                    ObjectType = "TestObjectType",
                    Name = "LocalizedProperty",
                    ValueType = DynamicPropertyValueType.ShortText,
                    IsLocaleDependent = true,
                    LocalizedNames = new[]
                    {
                        new DynamicPropertyName
                        {
                            Locale = "en-US", Name = "Localized Property"
                        },
                        new DynamicPropertyName
                        {
                            Locale = "ru-RU", Name = "Локализованное свойство"
                        },
                    },
                },
                new DynamicProperty
                {
                    ObjectType = "TestObjectType",
                    Name = "GenericProperty",
                    ValueType = DynamicPropertyValueType.ShortText,
                    IsArray = true,
                },
            };

            var service = GetDynamicPropertyService();
            service.SaveTypeProperties(typeProperties);

            var typeProperties1 = service.GetTypeProperties("TestObjectType");
        }

        [TestMethod]
        public void SaveLoadObjectProperties()
        {
            var objectProperties = new[]
            {
                new DynamicProperty
                {
                    Name = "LocalizedProperty",
                    ObjectType = "TestObjectType",
                    ObjectId = "111",
                    ValueType = DynamicPropertyValueType.ShortText,
                    Value = "Blue",
                    IsLocaleDependent = true,
                    LocalizedValues = new []
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
                    Name = "LocalizedProperty",
                    ObjectType = "TestObjectType",
                    ObjectId = "222",
                    ValueType = DynamicPropertyValueType.ShortText,
                    Value = "Green",
                    IsLocaleDependent = true,
                    LocalizedValues = new []
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
                    Name = "GenericProperty",
                    ObjectType = "TestObjectType",
                    ObjectId = "222",
                    ValueType = DynamicPropertyValueType.ShortText,
                    IsArray = true,
                    ArrayValues = new []
                    {
                        "qwerty",
                        "asdfgh",
                    }
                },
            };

            var service = GetDynamicPropertyService();
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
