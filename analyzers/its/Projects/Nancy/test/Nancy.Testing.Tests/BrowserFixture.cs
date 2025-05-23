namespace Nancy.Testing.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using Nancy.Extensions;
    using Nancy.Helpers;
    using Nancy.Session;
    using Nancy.Tests;
    using Nancy.Authentication.Forms;
    using Nancy.Configuration;
    using Nancy.Tests.xUnitExtensions;
    using Xunit;
    using FakeItEasy;

    public class BrowserFixture
    {
        private readonly Browser browser;

        public BrowserFixture()
        {
            var bootstrapper =
                new ConfigurableBootstrapper(config => config.Modules(typeof(EchoModule)));

            CookieBasedSessions.Enable(bootstrapper);

            this.browser = new Browser(bootstrapper);
        }

        [Fact]
        public async Task Should_be_able_to_send_string_in_body()
        {
            // Given
            const string thisIsMyRequestBody = "This is my request body";

            // When
            var result = await browser.Post("/", with =>
                {
                    with.HttpRequest();
                    with.Body(thisIsMyRequestBody);
                });

            // Then
            result.Body.AsString().ShouldEqual(thisIsMyRequestBody);
        }

        [Fact]
        public async Task Should_be_able_to_set_user_host_address()
        {
            // Given
            const string userHostAddress = "127.0.0.1";

            // When
            var result = await browser.Get("/userHostAddress", with =>
                {
                    with.HttpRequest();
                    with.UserHostAddress(userHostAddress);
                });

            // Then
            result.Body.AsString().ShouldEqual(userHostAddress);
        }

        [Fact]
        public async Task Should_be_able_check_is_local_ipV4()
        {
            // Given
            const string userHostAddress = "127.0.0.1";

            // When
            var result = await browser.Get("/isLocal", with =>
                {
                    with.HttpRequest();
                    with.HostName("localhost");
                    with.UserHostAddress(userHostAddress);
                });

            // Then
            result.Body.AsString().ShouldEqual("local");
        }

        [Fact]
        public async Task Should_be_able_check_is_local_ipV6()
        {
            // Given
            const string userHostAddress = "::1";

            // When
            var result = await browser.Get("/isLocal", with =>
                {
                    with.HttpRequest();
                    with.HostName("localhost");
                    with.UserHostAddress(userHostAddress);
                });

            // Then
            result.Body.AsString().ShouldEqual("local");
        }

        [Fact]
        public async Task Should_be_able_check_is_not_local()
        {
            // Given
            const string userHostAddress = "84.12.65.72";

            // When
            var result = await browser.Get("/isLocal", with =>
                {
                    with.HttpRequest();
                    with.HostName("anotherhost");
                    with.UserHostAddress(userHostAddress);
                });

            // Then
            result.Body.AsString().ShouldEqual("not-local");
        }

        [Fact]
        public async Task Should_be_able_to_send_stream_in_body()
        {
            // Given
            const string thisIsMyRequestBody = "This is my request body";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(thisIsMyRequestBody);
            writer.Flush();

            // When
            var result = await browser.Post("/", with =>
                {
                    with.HttpRequest();
                    with.Body(stream, "text/plain");
                });

            // Then
            result.Body.AsString().ShouldEqual(thisIsMyRequestBody);
        }

        [Fact]
        public async Task Should_be_able_to_send_json_in_body()
        {
            // Given
            var model = new EchoModel { SomeString = "Some String", SomeInt = 29, SomeBoolean = true };

            // When
            var result = await browser.Post("/", with =>
                {
                    with.JsonBody(model);
                });

            // Then
            var actualModel = result.Body.DeserializeJson<EchoModel>();

            actualModel.ShouldNotBeNull();
            actualModel.SomeString.ShouldEqual(model.SomeString);
            actualModel.SomeInt.ShouldEqual(model.SomeInt);
            actualModel.SomeBoolean.ShouldEqual(model.SomeBoolean);
        }

        [Fact]
        public async Task Should_be_able_to_send_xml_in_body()
        {
            // Given
            var model = new EchoModel { SomeString = "Some String", SomeInt = 29, SomeBoolean = true };

            // When
            var result = await browser.Post("/", with =>
                {
                    with.XMLBody(model);
                });

            // Then
            var actualModel = result.Body.DeserializeXml<EchoModel>();

            actualModel.ShouldNotBeNull();
            actualModel.SomeString.ShouldEqual(model.SomeString);
            actualModel.SomeInt.ShouldEqual(model.SomeInt);
            actualModel.SomeBoolean.ShouldEqual(model.SomeBoolean);
        }

        [Fact]
        public void Should_add_basic_authentication_credentials_to_the_headers_of_the_request()
        {
            // Given
            var context = new BrowserContext(A.Fake<INancyEnvironment>());

            // When
            context.BasicAuth("username", "password");

            // Then
            IBrowserContextValues values = context;

            var credentials = string.Format("{0}:{1}", "username", "password");
            var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));

            values.Headers["Authorization"].ShouldHaveCount(1);
            values.Headers["Authorization"].First().ShouldEqual("Basic " + encodedCredentials);
        }

        [Fact]
        public void Should_add_cookies_to_the_request()
        {
            // Given
            var context = new BrowserContext(A.Fake<INancyEnvironment>());

            var cookies =
                new Dictionary<string, string>
                {
                    { "CookieName", "CookieValue" },
                    { "SomeCookieName", "SomeCookieValue" }
                };

            // When
            context.Cookie(cookies);

            // Then
            IBrowserContextValues values = context;

            var cookieString = cookies.Aggregate(string.Empty, (current, cookie) => current + string.Format("{0}={1};", HttpUtility.UrlEncode(cookie.Key), HttpUtility.UrlEncode(cookie.Value)));

            values.Headers["Cookie"].ShouldHaveCount(1);
            values.Headers["Cookie"].First().ShouldEqual(cookieString);
        }

        [Fact]
        public void Should_add_cookie_to_the_request()
        {
            // Given
            var context = new BrowserContext(A.Fake<INancyEnvironment>());

            var cookies =
                new Dictionary<string, string>
                {
                    { "CookieName", "CookieValue" },
                    { "SomeCookieName", "SomeCookieValue" }
                };

            // When
            foreach (var cookie in cookies)
            {
                context.Cookie(cookie.Key, cookie.Value);
            }

            // Then
            IBrowserContextValues values = context;

            var cookieString = cookies.Aggregate(string.Empty, (current, cookie) => current + string.Format("{0}={1};", HttpUtility.UrlEncode(cookie.Key), HttpUtility.UrlEncode(cookie.Value)));

            values.Headers["Cookie"].ShouldHaveCount(1);
            values.Headers["Cookie"].First().ShouldEqual(cookieString);
        }

        [Fact]
        public async Task Should_add_cookies_to_the_request_and_get_cookies_in_response()
        {
            // Given
            var cookies =
                new Dictionary<string, string>
                {
                    { "CookieName", "CookieValue" },
                    { "SomeCookieName", "SomeCookieValue" }
                };

            // When
            var result = await browser.Get("/cookie", with =>
                {
                    with.Cookie(cookies);
                });

            // Then
            result.Cookies.Single(x => x.Name == "CookieName").Value.ShouldEqual("CookieValue");
            result.Cookies.Single(x => x.Name == "SomeCookieName").Value.ShouldEqual("SomeCookieValue");
        }

        [Fact]
        public async Task Should_add_a_cookie_to_the_request_and_get_a_cookie_in_response()
        {
            // Given, When
            var result = await browser.Get("/cookie", with => with.Cookie("CookieName", "CookieValue"));

            // Then
            result.Cookies.Single(x => x.Name == "CookieName").Value.ShouldEqual("CookieValue");
        }

        [Fact]
        public async Task Should_be_able_to_continue_with_another_request()
        {
            // Given
            const string FirstRequestBody = "This is my first request body";
            const string SecondRequestBody = "This is my second request body";
            var firstRequestStream = new MemoryStream();
            var firstRequestWriter = new StreamWriter(firstRequestStream);
            firstRequestWriter.Write(FirstRequestBody);
            firstRequestWriter.Flush();
            var secondRequestStream = new MemoryStream();
            var secondRequestWriter = new StreamWriter(secondRequestStream);
            secondRequestWriter.Write(SecondRequestBody);
            secondRequestWriter.Flush();

            // When
            await browser.Post("/", with =>
                {
                    with.HttpRequest();
                    with.Body(firstRequestStream, "text/plain");
                });

            var result = await browser.Post("/", with =>
                {
                    with.HttpRequest();
                    with.Body(secondRequestStream, "text/plain");
                });

            // Then
            result.Body.AsString().ShouldEqual(SecondRequestBody);
        }

        [Fact]
        public async Task Should_maintain_cookies_when_chaining_requests()
        {
            // Given
            // When
            await browser.Get("/session", with => with.HttpRequest());

            var result = await this.browser.Get(
                             "/session",
                             with => with.HttpRequest());

            //Then
            result.Body.AsString().ShouldEqual("Current session value is: I've created a session!");
        }

        [Fact]
        public async Task Should_maintain_cookies_even_if_not_set_on_directly_preceding_request()
        {
            // Given
            // When
            await browser.Get("/session", with => with.HttpRequest());

            await browser.Get("/nothing", with => with.HttpRequest());

            var result = await browser.Get("/session", with => with.HttpRequest());

            //Then
            result.Body.AsString().ShouldEqual("Current session value is: I've created a session!");
        }

        [Fact]
        public async Task Should_be_able_to_not_specify_delegate_for_basic_http_request()
        {
            //Given, When
            var result = await browser.Get("/type");

            //Then
            result.Body.AsString().ShouldEqual("http");
        }

        [Fact]
        public async Task Should_add_ajax_header()
        {
            //Given, When
            var result = await browser.Get("/ajax", with => with.AjaxRequest());

            //Then
            result.Body.AsString().ShouldEqual("ajax");
        }

        [Fact]
        public async Task Should_throw_an_exception_when_the_cert_couldnt_be_found()
        {
            //Given, When
            var exception = await RecordAsync.Exception(() =>
                {
                    return browser.Get("/ajax", with =>
                                         with.Certificate(
                                             StoreLocation.CurrentUser,
                                             StoreName.My,
                                             X509FindType.FindByThumbprint,
                                             "aa aa aa"));
                });

            //Then
            exception.ShouldBeOfType<InvalidOperationException>();
        }

        [Fact]
        public async Task Should_add_certificate()
        {
            //Given, When
            var result = await browser.Get("/cert", with => with.Certificate());

            //Then
            result.Context.Request.ClientCertificate.ShouldNotBeNull();
        }

        [Fact]
        public async Task Should_change_scheme_to_https_when_HttpsRequest_is_called_on_the_context()
        {
            //Given, When
            var result = await browser.Get("/", with => with.HttpsRequest());

            //Then
            result.Context.Request.Url.Scheme.ShouldEqual("https");
        }

        [Fact]
        public async Task Should_add_forms_authentication_cookie_to_the_request()
        {
            //Given
            var userId = A.Dummy<Guid>();

            var formsAuthConfig = new FormsAuthenticationConfiguration()
            {
                RedirectUrl = "/login",
                UserMapper = A.Fake<IUserMapper>(),
            };

            var encryptedId = formsAuthConfig.CryptographyConfiguration.EncryptionProvider.Encrypt(userId.ToString());
            var hmacBytes = formsAuthConfig.CryptographyConfiguration.HmacProvider.GenerateHmac(encryptedId);
            var hmacString = Convert.ToBase64String(hmacBytes);
            var cookieContents = String.Format("{1}{0}", encryptedId, hmacString);

            //When
            var response = await browser.Get("/cookie", (with) =>
                {
                    with.HttpRequest();
                    with.FormsAuth(userId, formsAuthConfig);
                });

            var cookie = response.Cookies.Single(c => c.Name == FormsAuthentication.FormsAuthenticationCookieName);
            var cookieValue = cookie.Value;

            //Then
            cookieValue.ShouldEqual(cookieContents);
        }

        [Fact]
        public async Task Should_return_JSON_serialized_form()
        {
            // Given
            var response = await browser.Post("/serializedform", (with) =>
                {
                    with.HttpRequest();
                    with.Accept("application/json");
                    with.FormValue("SomeString", "Hi");
                    with.FormValue("SomeInt", "1");
                    with.FormValue("SomeNullableInt1", "2");
                    with.FormValue("SomeNullableInt2", string.Empty);
                    with.FormValue("SomeNullableInt3", null);
                    with.FormValue("SomeBoolean", "true");
                });

            // When
            var actualModel = response.Body.DeserializeJson<EchoModel>();

            // Then
            Assert.Equal("Hi", actualModel.SomeString);
            Assert.Equal(1, actualModel.SomeInt);
            actualModel.SomeNullableInt1.ShouldEqual(2);
            actualModel.SomeNullableInt2.ShouldBeNull();
            actualModel.SomeNullableInt3.ShouldBeNull();
            Assert.True(actualModel.SomeBoolean);
        }

        [Fact]
        public async Task Should_return_JSON_serialized_querystring()
        {
            // Given
            var response = await browser.Get("/serializedquerystring", (with) =>
                {
                    with.HttpRequest();
                    with.Accept("application/json");
                    with.Query("SomeString", "Hi");
                    with.Query("SomeInt", "1");
                    with.Query("SomeNullableInt1", "2");
                    with.Query("SomeNullableInt2", string.Empty);
                    with.Query("SomeNullableInt3", null);
                    with.Query("SomeBoolean", "true");
                });

            // When
            var actualModel = response.Body.DeserializeJson<EchoModel>();

            // Then
            Assert.Equal("Hi", actualModel.SomeString);
            Assert.Equal(1, actualModel.SomeInt);
            actualModel.SomeNullableInt1.ShouldEqual(2);
            actualModel.SomeNullableInt2.ShouldBeNull();
            actualModel.SomeNullableInt3.ShouldBeNull();
            Assert.True(actualModel.SomeBoolean);
        }

        [Fact]
        public async Task Should_encode_form()
        {
            //Given, When
            var result = await browser.Post("/encoded", with =>
                {
                    with.HttpRequest();
                    with.FormValue("name", "john++");
                });

            //Then
            result.Body.AsString().ShouldEqual("john++");
        }

        [Fact]
        public async Task Should_encode_querystring()
        {
            //Given, When
            var result = await browser.Post("/encodedquerystring", with =>
                {
                    with.HttpRequest();
                    with.Query("name", "john++");
                });

            //Then
            result.Body.AsString().ShouldEqual("john++");
        }

        [Fact]
        public async Task Should_add_nancy_testing_browser_header_as_default_user_agent()
        {
            // Given
            const string expectedHeaderValue = "Nancy.Testing.Browser";

            // When
            var result = (await browser.Get("/useragent")).Body.AsString();

            // Then
            result.ShouldEqual(expectedHeaderValue);
        }

        [Fact]
        public async Task Should_override_default_user_agent_when_explicitly_defined()
        {
            // Given
            const string expectedHeaderValue = "Custom.User.Agent";

            // When
            var result = await browser.Get("/useragent", with =>
                {
                    with.Header("User-Agent", expectedHeaderValue);
                });

            var header = result.Body.AsString();

            // Then
            header.ShouldEqual(expectedHeaderValue);
        }

        [Theory]
        [InlineData("application/xml")]
        public async Task Should_return_error_message_on_cyclical_exception(string accept)
        {
            //Given
            var browser = new Browser(with =>
            {
                with.Modules(typeof(EchoModule));
                with.Configure(env =>
                {
                    env.Tracing(
                        enabled: true,
                        displayErrorTraces: true);
                });
            });

            // When
            var result = await browser.Get("/cyclical", with => with.Accept(accept));

            //Then
            var resultBody = result.Body.AsString();
            resultBody.ShouldNotBeEmpty();
        }

        [Theory]
        [InlineData("application/xml")]
        public async Task Should_return_no_error_message_on_cyclical_exception_when_disabled_error_trace(string accept)
        {
            //Given
            var browser = new Browser(with =>
            {
                with.Modules(typeof(EchoModule));
                with.Configure(env =>
                {
                    env.Tracing(
                        enabled: true,
                        displayErrorTraces: false);
                });
            });

            // When
            var result = await browser.Get("/cyclical", with => with.Accept(accept));

            //Then
            result.Body.AsString().ShouldBeEmpty();
        }

        public class EchoModel
        {
            public string SomeString { get; set; }

            public int SomeInt { get; set; }

            public int? SomeNullableInt1 { get; set; }

            public int? SomeNullableInt2 { get; set; }

            public int? SomeNullableInt3 { get; set; }

            public bool SomeBoolean { get; set; }
        }

        public class Category
        {
            public string Name { get; set; }

            public ICollection<Product> Products { get; set; }
        }

        public class Product
        {
            public string Name { get; set; }

            public Category Category { get; set; }
        }

        public class EchoModule : NancyModule
        {
            public EchoModule()
            {
                Get("/cyclical", args =>
                {
                    var category = new Category();
                    category.Name = "Electronics";

                    var product = new Product();
                    product.Name = "iPad";
                    product.Category = category;

                    category.Products = new Collection<Product>(new List<Product>(new[] { product }));

                    return product;
                });

                Post("/", args =>
                {
                    var body = new StreamReader(this.Context.Request.Body).ReadToEnd();
                    return new Response
                    {
                        Contents = stream =>
                        {
                            var writer = new StreamWriter(stream);
                            writer.Write(body);
                            writer.Flush();
                        }
                    };
                });

                Get("/cookie", args =>
                {
                    var response = (Response)"Cookies";

                    foreach (var cookie in this.Request.Cookies)
                    {
                        response.WithCookie(cookie.Key, cookie.Value);
                    }

                    return response;
                });

                Get("/nothing", args => string.Empty);

                Get("/userHostAddress", args => this.Request.UserHostAddress);

                Get("/isLocal", args => this.Request.IsLocal() ? "local" : "not-local");

                Get("/session", args =>
                {
                    var value = Session["moo"] ?? "";

                    var output = "Current session value is: " + value;

                    if (string.IsNullOrEmpty(value.ToString()))
                    {
                        Session["moo"] = "I've created a session!";
                    }

                    var response = (Response)output;

                    return response;
                });

                Get("/useragent", args => this.Request.Headers.UserAgent);

                Get("/type", args => this.Request.Url.Scheme.ToLower());

                Get("/ajax", args => this.Request.IsAjaxRequest() ? "ajax" : "not-ajax");

                Post("/encoded", args => (string)this.Request.Form.name);

                Post("/encodedquerystring", args => (string)this.Request.Query.name);

                Post("/serializedform", args =>
                {
                    IDictionary<string, object> data = Request.Form.ToDictionary();

                    return data;
                });

                Get("/serializedquerystring", args =>
                {
                    IDictionary<string, object> data = Request.Query.ToDictionary();

                    return data;
                });
            }
        }
    }
}
