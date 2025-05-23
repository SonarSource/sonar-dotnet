﻿namespace Nancy.Tests.Functional.Tests
{
    using System;
    using System.Threading.Tasks;
    using Nancy.Bootstrapper;
    using Nancy.Testing;
    using Nancy.Tests.Functional.Modules;

    using Xunit;

    public class JsonpTests
    {
        private readonly INancyBootstrapper bootstrapper;

        private readonly Browser browser;

        public JsonpTests()
        {
            this.bootstrapper = new ConfigurableBootstrapper(
                    configuration =>
                        {
                            configuration.Modules(new Type[] { typeof(JsonpTestModule) });
                        });

            this.browser = new Browser(bootstrapper);
        }

        [Fact]
        public async Task Ensure_that_Jsonp_hook_does_not_affect_normal_responses()
        {
            var result = await browser.Get("/test/string", c =>
            {
                c.HttpRequest();
            });

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("Normal Response", result.Body.AsString());
        }

        [Fact]
        public async Task Ensure_that_dynamic_string_parameters_are_serialized_as_strings()
        {
            var result = await browser.Get("/test/something", c => c.HttpRequest());
            var actual = result.Body.AsString();

            Assert.Equal(@"{""name"":""something""}", actual);
        }

        [Fact]
        public async Task Ensure_that_Jsonp_hook_does_not_affect_a_normal_json_response()
        {
            var result = await browser.Get("/test/json", c =>
            {
                c.HttpRequest();
            });

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("true", result.Body.AsString());
            Assert.Equal("application/json; charset=utf-8", result.Context.Response.ContentType);
        }

        [Fact]
        public async Task Ensure_that_Jsonp_hook_should_pad_a_json_response_when_callback_is_present()
        {
            var result = await browser.Get("/test/json", with =>
            {
                with.HttpRequest();
                with.Query("callback", "myCallback");
            });

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("myCallback(true);", result.Body.AsString());
            Assert.Equal("application/javascript; charset=utf-8", result.Context.Response.ContentType);
        }
    }
}
