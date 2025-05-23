﻿namespace Nancy.Tests.Functional.Tests
{
    using System;
    using System.Threading.Tasks;
    using Nancy.Bootstrapper;
    using Nancy.Testing;
    using Nancy.Tests.Functional.Modules;

    using Xunit;

    public class SerializeTests
    {
        private readonly INancyBootstrapper bootstrapper;

        private readonly Browser browser;

        public SerializeTests()
        {
            this.bootstrapper = new ConfigurableBootstrapper(
                    configuration => configuration.Modules(new Type[] { typeof(SerializeTestModule) }));

            this.browser = new Browser(bootstrapper);
        }

        [Fact]
        public async Task Should_return_JSON_serialized_form()
        {
            //Given
            var response = await browser.Post("/serializedform", (with) =>
            {
                with.HttpRequest();
                with.Accept("application/json");
                with.FormValue("SomeString", "Hi");
                with.FormValue("SomeInt", "1");
                with.FormValue("SomeBoolean", "true");
            });

            //When
            var actualModel = response.Body.DeserializeJson<EchoModel>();

            //Then
            Assert.Equal("Hi", actualModel.SomeString);
            Assert.Equal(1, actualModel.SomeInt);
            Assert.True(actualModel.SomeBoolean);
        }

        [Fact]
        public async Task Should_return_JSON_with_parameterised_constructor_from_serialized_form()
        {
            //Given
            var response = await browser.Post("/serializedform", (with) =>
            {
                with.HttpRequest();
                with.Accept("application/json");
                with.FormValue("SomeString", "Hi");
                with.FormValue("SomeInt", "1");
                with.FormValue("SomeBoolean", "true");
            });

            //When
            var actualModel = response.Body.DeserializeJson<ParameterisedConstructorEchoModel>();

            //Then
            Assert.Equal("Hi", actualModel.SomeString);
            Assert.Equal(1, actualModel.SomeInt);
            Assert.True(actualModel.SomeBoolean);
        }

        [Fact]
        public async Task Should_return_JSON_serialized_querystring()
        {
            //Given
            var response = await browser.Get("/serializedquerystring", (with) =>
            {
                with.HttpRequest();
                with.Accept("application/json");
                with.Query("SomeString", "Hi");
                with.Query("SomeInt", "1");
                with.Query("SomeBoolean", "true");
            });

            //When
            var actualModel = response.Body.DeserializeJson<EchoModel>();

            //Then
            Assert.Equal("Hi", actualModel.SomeString);
            Assert.Equal(1, actualModel.SomeInt);
            Assert.True(actualModel.SomeBoolean);
        }

        public class EchoModel
        {
            public string SomeString { get; set; }
            public int SomeInt { get; set; }
            public bool SomeBoolean { get; set; }
        }

        public class ParameterisedConstructorEchoModel
        {
            public ParameterisedConstructorEchoModel(string someString, int someInt, bool someBoolean)
            {
                this.SomeString = someString;
                this.SomeInt = someInt;
                this.SomeBoolean = someBoolean;
            }

            public string SomeString { get; set; }
            public int SomeInt { get; set; }
            public bool SomeBoolean { get; set; }
        }
    }
}
