﻿using System.Linq;
using System.Threading.Tasks;
using Kiota.Builder.CodeDOM;
using Kiota.Builder.Configuration;
using Kiota.Builder.Refiners;

using Xunit;

namespace Kiota.Builder.Tests.Refiners
{
    public class PhpLanguageRefinerTests
    {
        private readonly CodeNamespace root = CodeNamespace.InitRootNamespace();

        [Fact]
        public async Task ReplacesRequestBuilderPropertiesByMethods()
        {
            var model = root.AddClass(new CodeClass
            {
                Name = "userRequestBuilder",
                Kind = CodeClassKind.RequestBuilder
            }).First();

            var requestBuilder = model.AddProperty(new CodeProperty
            {
                Name = "breaks", 
                Kind = CodePropertyKind.RequestBuilder,
                Type = new CodeType
                {
                    Name = "string"
                }
            }).First();
            await ILanguageRefiner.Refine(new GenerationConfiguration {Language = GenerationLanguage.PHP}, root);
            Assert.Equal("breaks", requestBuilder.Name);
            Assert.Equal("userRequestBuilder", model.Name);
        }

        [Fact]
        public async Task PrefixReservedWordPropertyNamesWith()
        {
            var model = root.AddClass(new CodeClass
            {
                Name = "userRequestBuilder",
                Kind = CodeClassKind.RequestBuilder
            }).First();

            var property = model.AddProperty(new CodeProperty
            {
                Name = "continue", 
                Kind = CodePropertyKind.RequestBuilder,
                Type = new CodeType
                {
                    Name = "string"
                }
            }).First();
            
            await ILanguageRefiner.Refine(new GenerationConfiguration {Language = GenerationLanguage.PHP}, root);
            Assert.Equal("EscapedContinue",property.Name);
        }
        
        [Fact]
        public async Task ReplacesBinaryWithNativeType()
        {
            var model = root.AddClass(new CodeClass
            {
                Name = "model",
                Kind = CodeClassKind.Model
            }).First();
            var method = model.AddMethod(new CodeMethod
            {
                Name = "method"
            }).First();
            method.ReturnType = new CodeType
            {
                Name = "binary"
            };
            await ILanguageRefiner.Refine(new GenerationConfiguration { Language = GenerationLanguage.PHP}, root);
            Assert.Equal("StreamInterface", method.ReturnType.Name);
        }

        [Fact]
        public async Task AddsDefaultImports() {
            var model = root.AddClass(new CodeClass
            {
                Name = "model",
                Kind = CodeClassKind.Model
            }).First();
            var requestBuilder = root.AddClass(new CodeClass
            {
                Name = "rb",
                Kind = CodeClassKind.RequestBuilder,
            }).First();
            await ILanguageRefiner.Refine(new GenerationConfiguration { Language = GenerationLanguage.PHP }, root);
            Assert.NotEmpty(model.StartBlock.Usings);
        }

        [Fact]
        public async Task ChangesBackingStoreParameterTypeInApiClientConstructor()
        {
            var apiClientClass = new CodeClass { Name = "ApiClient", Kind = CodeClassKind.Custom };
            var constructor = new CodeMethod { Name = "ApiClientConstructor", Kind = CodeMethodKind.ClientConstructor};
            var backingStoreParameter = new CodeParameter
            {
                Name = "BackingStore",
                Kind = CodeParameterKind.BackingStore,
                Type = new CodeType
                {
                    Name = "IBackingStoreFactory",
                    IsExternal = true
                }
            };
            constructor.AddParameter(backingStoreParameter);
            constructor.DeserializerModules = new() {"Microsoft\\Kiota\\Serialization\\Deserializer"};
            constructor.SerializerModules = new() {"Microsoft\\Kiota\\Serialization\\Serializer"};
            apiClientClass.AddMethod(constructor);

            root.AddClass(apiClientClass);
            await ILanguageRefiner.Refine(new GenerationConfiguration { Language = GenerationLanguage.PHP, UsesBackingStore = true}, root);
            Assert.Equal("BackingStoreFactory", backingStoreParameter.Type.Name);
            Assert.Equal("null", backingStoreParameter.DefaultValue);
        }
    }
}
