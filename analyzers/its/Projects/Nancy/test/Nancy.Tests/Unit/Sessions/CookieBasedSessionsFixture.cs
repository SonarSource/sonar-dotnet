namespace Nancy.Tests.Unit.Sessions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using FakeItEasy;
    using Nancy.Bootstrapper;
    using Nancy.Cryptography;
    using Nancy.Helpers;
    using Nancy.IO;
    using Nancy.Session;
    using Nancy.Tests.Fakes;
    using Xunit;

    public class CookieBasedSessionsFixture
    {
        private const string ValidData = "PzQmxxriR9Ht9a1rJMC1yAr5Ty+CIIE1/fXX/B2e32wtf8+KzGaBMOW6Ks9xb503haQnFTm9Z9QsJgoUPClwU6Ke25HRjXdKY7RQIG4XXT7APU/NV3KAJZuwbJObv22PR/yKbkTOEYAvn/m7FLM6jxvn5lCCw75Kw0vNvKiTbyE5ijh5EY4XfQHGt+5s6vrehh26reJBuXiY4hPwopGbOsvHNNw3HpbQPmut1qHiqX/w8naD2vuFmX0Dckv1Kkf+K3zG2BVHtXi+C3kufmUR+l/1C7p4Y7r+6n9+7o7Bf8aWkMNWdAA704+xrT+Zyy5NkKkFGMtNmju6bv+6PH1H6hwXs5QUSWcj3Ke9XQvWaYBHCkBdIvY4FvMZUeQfw5oaECST+Zz+nMxAQ5TgOrvVo6Zr0B+CF+COvXeyhX7YbbM=";
        private const string ValidHmac = "QWc9gRZbQ6pK5ECs2Zp4E5WenpQ/XXgYUDPY46WihPY=";

        private readonly IEncryptionProvider fakeEncryptionProvider;
        private readonly CookieBasedSessions cookieStore;
        private readonly IHmacProvider fakeHmacProvider;
        private readonly IObjectSerializer fakeObjectSerializer;
        private readonly AesEncryptionProvider aesEncryptionProvider;
        private readonly DefaultHmacProvider defaultHmacProvider;
        private readonly IObjectSerializer defaultObjectSerializer;

        public CookieBasedSessionsFixture()
        {
            this.fakeEncryptionProvider = A.Fake<IEncryptionProvider>();
            this.fakeHmacProvider = A.Fake<IHmacProvider>();
            this.fakeObjectSerializer = new FakeObjectSerializer();
            this.cookieStore = new CookieBasedSessions(this.fakeEncryptionProvider, this.fakeHmacProvider, this.fakeObjectSerializer);

            this.aesEncryptionProvider = new AesEncryptionProvider(new PassphraseKeyGenerator("password", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 1000));
            this.defaultHmacProvider = new DefaultHmacProvider(new PassphraseKeyGenerator("anotherpassword", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 1000));
            this.defaultObjectSerializer = new DefaultObjectSerializer();
        }

        [Fact]
        public void Should_save_nothing_if_the_session_is_null()
        {
            var response = new Response();

            cookieStore.Save(null, response);

            response.Cookies.Count.ShouldEqual(0);
        }

        [Fact]
        public void Should_save_nothing_if_the_session_has_not_changed()
        {
            var response = new Response();

            cookieStore.Save(new Session(new Dictionary<string, object> { { "key", "value" } }), response);

            response.Cookies.Count.ShouldEqual(0);
        }

        [Fact]
        public void Should_save_the_session_cookie()
        {
            var response = new Response();
            var session = new Session(new Dictionary<string, object>
                                      {
                                          {"key1", "val1"},
                                      });
            session["key2"] = "val2";
            A.CallTo(() => this.fakeEncryptionProvider.Encrypt("key1=val1;key2=val2;")).Returns("encrypted=key1=val1;key2=val2;");

            cookieStore.Save(session, response);

            response.Cookies.Count.ShouldEqual(1);
            var cookie = response.Cookies.First();
            cookie.Name.ShouldEqual(this.cookieStore.CookieName);
            cookie.Value.ShouldEqual("encrypted%3dkey1%3dval1%3bkey2%3dval2%3b");
            cookie.Expires.ShouldBeNull();
            cookie.Path.ShouldBeNull();
            cookie.Domain.ShouldBeNull();
        }

        [Fact]
        public void Should_save_cookie_as_http_only()
        {
            var response = new Response();
            var session = new Session();
            session["key 1"] = "val=1";
            A.CallTo(() => this.fakeEncryptionProvider.Encrypt("key+1=val%3d1;")).Returns("encryptedkey+1=val%3d1;");

            cookieStore.Save(session, response);

            response.Cookies.First().HttpOnly.ShouldEqual(true);
        }

        [Fact]
        public void Should_saves_url_safe_keys_and_values()
        {
            var response = new Response();
            var session = new Session();
            session["key 1"] = "val=1";
            A.CallTo(() => this.fakeEncryptionProvider.Encrypt("key+1=val%3d1;")).Returns("encryptedkey+1=val%3d1;");

            cookieStore.Save(session, response);

            response.Cookies.First().Value.ShouldEqual("encryptedkey%2b1%3dval%253d1%3b");
        }

        [Fact]
        public void Should_load_an_empty_session_if_no_session_cookie_exists()
        {
            var request = CreateRequest(null);

            var result = cookieStore.Load(request);

            result.Count.ShouldEqual(0);
        }

        [Fact]
        public void Should_load_an_empty_session_if_session_cookie_is_invalid()
        {
            //given
            var inputValue = ValidHmac.Substring(0, 5); //invalid Hmac
            inputValue = HttpUtility.UrlEncode(inputValue);
            var store = new CookieBasedSessions(this.aesEncryptionProvider, this.defaultHmacProvider, this.defaultObjectSerializer);
            var request = new Request("GET", "/", "http");
            request.Cookies.Add(store.CookieName, inputValue);

            //when
            var result = store.Load(request);

            //then
            result.Count.ShouldEqual(0);
        }

        [Fact]
        public void Should_load_a_single_valued_session()
        {
            var request = CreateRequest("encryptedkey1=value1");
            A.CallTo(() => this.fakeEncryptionProvider.Decrypt("encryptedkey1=value1")).Returns("key1=value1;");

            var session = cookieStore.Load(request);

            session.Count.ShouldEqual(1);
            session["key1"].ShouldEqual("value1");
        }

        [Fact]
        public void Should_load_a_multi_valued_session()
        {
            var request = CreateRequest("encryptedkey1=value1;key2=value2");
            A.CallTo(() => this.fakeEncryptionProvider.Decrypt("encryptedkey1=value1;key2=value2")).Returns("key1=value1;key2=value2");

            var session = cookieStore.Load(request);

            session.Count.ShouldEqual(2);
            session["key1"].ShouldEqual("value1");
            session["key2"].ShouldEqual("value2");
        }

        [Fact]
        public void Should_load_properly_decode_the_url_safe_session()
        {
            var request = CreateRequest(HttpUtility.UrlEncode("encryptedkey+1=val%3D1;"));
            A.CallTo(() => this.fakeEncryptionProvider.Decrypt("encryptedkey+1=val%3D1;")).Returns("key+1=val%3D1;");

            var session = cookieStore.Load(request);

            session.Count.ShouldEqual(1);
            session["key 1"].ShouldEqual("val=1");
        }

        [Fact]
        public void Should_add_pre_and_post_hooks_when_enabled()
        {
            var beforePipeline = new BeforePipeline();
            var afterPipeline = new AfterPipeline();
            var hooks = A.Fake<IPipelines>();
            A.CallTo(() => hooks.BeforeRequest).Returns(beforePipeline);
            A.CallTo(() => hooks.AfterRequest).Returns(afterPipeline);

            CookieBasedSessions.Enable(hooks, new CryptographyConfiguration(this.fakeEncryptionProvider, this.fakeHmacProvider));

            beforePipeline.PipelineDelegates.Count().ShouldEqual(1);
            afterPipeline.PipelineItems.Count().ShouldEqual(1);
        }

        [Fact]
        public void Should_only_not_add_response_cookie_if_it_has_not_changed()
        {
            var beforePipeline = new BeforePipeline();
            var afterPipeline = new AfterPipeline();
            var hooks = A.Fake<IPipelines>();
            A.CallTo(() => hooks.BeforeRequest).Returns(beforePipeline);
            A.CallTo(() => hooks.AfterRequest).Returns(afterPipeline);
            CookieBasedSessions.Enable(hooks, new CryptographyConfiguration(this.fakeEncryptionProvider, this.fakeHmacProvider)).WithSerializer(this.fakeObjectSerializer);
            var request = CreateRequest("encryptedkey1=value1");
            A.CallTo(() => this.fakeEncryptionProvider.Decrypt("encryptedkey1=value1")).Returns("key1=value1;");
            var response = A.Fake<Response>();
            var nancyContext = new NancyContext() { Request = request, Response = response };
            beforePipeline.Invoke(nancyContext, new CancellationToken());

            afterPipeline.Invoke(nancyContext, new CancellationToken());

            response.Cookies.Count.ShouldEqual(0);
        }

        [Fact]
        public void Should_add_response_cookie_if_it_has_changed()
        {
            var beforePipeline = new BeforePipeline();
            var afterPipeline = new AfterPipeline();
            var hooks = A.Fake<IPipelines>();
            A.CallTo(() => hooks.BeforeRequest).Returns(beforePipeline);
            A.CallTo(() => hooks.AfterRequest).Returns(afterPipeline);
            CookieBasedSessions.Enable(hooks, new CryptographyConfiguration(this.fakeEncryptionProvider, this.fakeHmacProvider)).WithSerializer(this.fakeObjectSerializer);
            var request = CreateRequest("encryptedkey1=value1");
            A.CallTo(() => this.fakeEncryptionProvider.Decrypt("encryptedkey1=value1")).Returns("key1=value1;");
            var response = A.Fake<Response>();
            var nancyContext = new NancyContext() { Request = request, Response = response };
            beforePipeline.Invoke(nancyContext, new CancellationToken());
            request.Session["Testing"] = "Test";

            afterPipeline.Invoke(nancyContext, new CancellationToken());

            response.Cookies.Count.ShouldEqual(1);
        }

        [Fact]
        public void Should_call_formatter_on_load()
        {
            var fakeFormatter = A.Fake<IObjectSerializer>();
            A.CallTo(() => this.fakeEncryptionProvider.Decrypt("encryptedkey1=value1")).Returns("key1=value1;");
            var store = new CookieBasedSessions(this.fakeEncryptionProvider, this.fakeHmacProvider, fakeFormatter);
            var request = CreateRequest("encryptedkey1=value1", false);

            store.Load(request);

            A.CallTo(() => fakeFormatter.Deserialize("value1")).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void Should_call_the_formatter_on_save()
        {
            var response = new Response();
            var session = new Session(new Dictionary<string, object>());
            session["key1"] = "value1";
            var fakeFormatter = A.Fake<IObjectSerializer>();
            var store = new CookieBasedSessions(this.fakeEncryptionProvider, this.fakeHmacProvider, fakeFormatter);

            store.Save(session, response);

            A.CallTo(() => fakeFormatter.Serialize("value1")).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void Should_set_formatter_when_using_formatter_selector()
        {
            var beforePipeline = new BeforePipeline();
            var afterPipeline = new AfterPipeline();
            var hooks = A.Fake<IPipelines>();
            A.CallTo(() => hooks.BeforeRequest).Returns(beforePipeline);
            A.CallTo(() => hooks.AfterRequest).Returns(afterPipeline);
            var fakeFormatter = A.Fake<IObjectSerializer>();
            A.CallTo(() => this.fakeEncryptionProvider.Decrypt("encryptedkey1=value1")).Returns("key1=value1;");
            CookieBasedSessions.Enable(hooks, new CryptographyConfiguration(this.fakeEncryptionProvider, this.fakeHmacProvider)).WithSerializer(fakeFormatter);
            var request = CreateRequest("encryptedkey1=value1");
            var nancyContext = new NancyContext() { Request = request };

            beforePipeline.Invoke(nancyContext, new CancellationToken());

            A.CallTo(() => fakeFormatter.Deserialize(A<string>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void Should_be_able_to_save_a_complex_object_to_session()
        {
            var response = new Response();
            var session = new Session(new Dictionary<string, object>());
            var payload = new DefaultSessionObjectFormatterFixture.Payload(27, true, "Test string");
            var store = new CookieBasedSessions(this.aesEncryptionProvider, this.defaultHmacProvider, this.defaultObjectSerializer);
            session["testObject"] = payload;

            store.Save(session, response);

            response.Cookies.Count.ShouldEqual(1);
            var cookie = response.Cookies.First();
            cookie.Name.ShouldEqual(store.CookieName);
            cookie.Value.ShouldNotBeNull();
            cookie.Value.ShouldNotBeEmpty();
        }

        [Fact]
        public void Should_be_able_to_load_an_object_previously_saved_to_session()
        {
            var response = new Response();
            var session = new Session(new Dictionary<string, object>());
            var payload = new DefaultSessionObjectFormatterFixture.Payload(27, true, "Test string");
            var store = new CookieBasedSessions(this.aesEncryptionProvider, this.defaultHmacProvider, this.defaultObjectSerializer);
            session["testObject"] = payload;
            store.Save(session, response);
            var request = new Request("GET", "/", "http");
            request.Cookies.Add(response.Cookies.First().Name, response.Cookies.First().Value);

            var result = store.Load(request);

            result["testObject"].ShouldEqual(payload);
        }

        [Fact]
        public void Should_encrypt_data()
        {
            var response = new Response();
            var session = new Session(new Dictionary<string, object>
                                      {
                                          {"key1", "val1"},
                                      });
            session["key2"] = "val2";

            cookieStore.Save(session, response);

            A.CallTo(() => this.fakeEncryptionProvider.Encrypt(A<string>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void Should_generate_hmac()
        {
            var response = new Response();
            var session = new Session(new Dictionary<string, object>
                                      {
                                          {"key1", "val1"},
                                      });
            session["key2"] = "val2";

            cookieStore.Save(session, response);

            A.CallTo(() => this.fakeHmacProvider.GenerateHmac(A<string>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void Should_load_valid_test_data()
        {
            // Given
            var payload = new DefaultSessionObjectFormatterFixture.Payload
            {
                BoolValue = true
            };

            var cookieData = GenerateCookieData(new Dictionary<string, object>
            {
                { "key1", payload }
            });

            var store =
                new CookieBasedSessions(this.aesEncryptionProvider, this.defaultHmacProvider, this.defaultObjectSerializer);

            var request =
                new Request("GET", "/", "http");

            request.Cookies.Add(store.CookieName, cookieData.ToString());

            // When
            var result = store.Load(request);

            // Then
            result.Count.ShouldEqual(1);
            result.First().Value.ShouldBeOfType(typeof(DefaultSessionObjectFormatterFixture.Payload));
        }

        [Fact]
        public void Should_return_blank_session_if_hmac_changed()
        {
            var inputValue = "b" + ValidHmac.Substring(1) + ValidData;
            inputValue = HttpUtility.UrlEncode(inputValue);
            var store = new CookieBasedSessions(this.aesEncryptionProvider, this.defaultHmacProvider, this.defaultObjectSerializer);
            var request = new Request("GET", "/", "http");
            request.Cookies.Add(store.CookieName, inputValue);

            var result = store.Load(request);

            result.Count.ShouldEqual(0);
        }

        [Fact]
        public void Should_return_blank_session_if_hmac_missing()
        {
            var inputValue = ValidData;
            inputValue = HttpUtility.UrlEncode(inputValue);
            var store = new CookieBasedSessions(this.aesEncryptionProvider, this.defaultHmacProvider, this.defaultObjectSerializer);
            var request = new Request("GET", "/", "http");
            request.Cookies.Add(store.CookieName, inputValue);

            var result = store.Load(request);

            result.Count.ShouldEqual(0);
        }

        [Fact]
        public void Should_return_blank_session_if_encrypted_data_modified()
        {
            var inputValue = ValidHmac + ValidData.Substring(0, ValidData.Length - 1) + "Z";
            inputValue = HttpUtility.UrlEncode(inputValue);
            var store = new CookieBasedSessions(this.aesEncryptionProvider, this.defaultHmacProvider, this.defaultObjectSerializer);
            var request = new Request("GET", "/", "http");
            request.Cookies.Add(store.CookieName, inputValue);

            var result = store.Load(request);

            result.Count.ShouldEqual(0);
        }

        [Fact]
        public void Should_return_blank_session_if_encrypted_data_are_invalid_but_contain_semicolon_when_decrypted()
        {
            var bogusEncrypted = this.aesEncryptionProvider.Encrypt("foo;bar");
            var inputValue = ValidHmac + bogusEncrypted;
            inputValue = HttpUtility.UrlEncode(inputValue);
            var store = new CookieBasedSessions(this.aesEncryptionProvider, this.defaultHmacProvider, this.defaultObjectSerializer);
            var request = new Request("GET", "/", "http");
            request.Cookies.Add(store.CookieName, inputValue);

            var result = store.Load(request);

            result.Count.ShouldEqual(0);
        }

        [Fact]
        public void Should_use_CookieName_when_config_provides_cookiename_value()
        {
            //Given
            var cryptoConfig = new CryptographyConfiguration(this.fakeEncryptionProvider, this.fakeHmacProvider);
            var storeConfig = new CookieBasedSessionsConfiguration(cryptoConfig)
            {
                CookieName = "NamedCookie",
                Serializer = this.fakeObjectSerializer
            };
            var store = new CookieBasedSessions(storeConfig);

            //When
            var response = new Response();
            var session = new Session(new Dictionary<string, object>
                                        {
                                            {"key1", "val1"},
                                        });
            session["key2"] = "val2";
            store.Save(session, response);

            //Then
            response.Cookies.ShouldHave(c => c.Name == storeConfig.CookieName);
        }

        [Fact]
        public void Should_set_Domain_when_config_provides_domain_value()
        {
            //Given
            var cryptoConfig = new CryptographyConfiguration(this.fakeEncryptionProvider, this.fakeHmacProvider);
            var storeConfig = new CookieBasedSessionsConfiguration(cryptoConfig)
            {
                Domain = ".nancyfx.org",
                Serializer = this.fakeObjectSerializer
            };
            var store = new CookieBasedSessions(storeConfig);

            //When
            var response = new Response();
            var session = new Session(new Dictionary<string, object>
                                        {
                                            {"key1", "val1"},
                                        });
            session["key2"] = "val2";
            store.Save(session, response);

            //Then
            var cookie = response.Cookies.First(c => c.Name == storeConfig.CookieName);
            cookie.Domain.ShouldEqual(storeConfig.Domain);
        }

        [Fact]
        public void Should_set_Path_when_config_provides_path_value()
        {
            //Given
            var cryptoConfig = new CryptographyConfiguration(this.fakeEncryptionProvider, this.fakeHmacProvider);
            var storeConfig = new CookieBasedSessionsConfiguration(cryptoConfig)
            {
                Path = "/",
                Serializer = this.fakeObjectSerializer
            };
            var store = new CookieBasedSessions(storeConfig);

            //When
            var response = new Response();
            var session = new Session(new Dictionary<string, object>
                                          {
                                              {"key1", "val1"},
                                          });
            session["key2"] = "val2";
            store.Save(session, response);

            //Then
            var cookie = response.Cookies.First(c => c.Name == storeConfig.CookieName);
            cookie.Path.ShouldEqual(storeConfig.Path);
        }

        private class CookieData
        {
            public string Data { get; set; }

            public string Hmac { get; set; }

            public override string ToString()
            {
                return HttpUtility.UrlEncode(string.Concat(this.Hmac, this.Data));
            }
        }

        private CookieData GenerateCookieData(string key, object data)
        {
            return this.GenerateCookieData(new Dictionary<string, object>
            {
                { key, data }
            });
        }

        private CookieData GenerateCookieData(IDictionary<string, object> data)
        {
            var sb = new StringBuilder();

            foreach (var key in data.Keys)
            {
                sb.Append(HttpUtility.UrlEncode(key));
                sb.Append("=");

                var objectString = this.defaultObjectSerializer.Serialize(data[key]);

                sb.Append(HttpUtility.UrlEncode(objectString));
                sb.Append(";");
            }

            var encryptedData =
                this.aesEncryptionProvider.Encrypt(sb.ToString());

            var hmacBytes =
                this.defaultHmacProvider.GenerateHmac(encryptedData);

            return new CookieData
            {
                Data = encryptedData,
                Hmac = Convert.ToBase64String(hmacBytes)
            };
        }

        private Request CreateRequest(string sessionValue, bool load = true)
        {
            var headers = new Dictionary<string, IEnumerable<string>>(1);

            if (!string.IsNullOrEmpty(sessionValue))
            {
                headers.Add("cookie", new[] { this.cookieStore.CookieName + "=" + HttpUtility.UrlEncode(sessionValue) });
            }

            var request = new Request("GET", new Url { Path = "/", Scheme = "http", Port = 9001, BasePath = "goku.power" }, CreateRequestStream(), headers);

            if (load)
            {
                cookieStore.Load(request);
            }

            return request;
        }

        private static RequestStream CreateRequestStream()
        {
            return CreateRequestStream(new MemoryStream());
        }

        private static RequestStream CreateRequestStream(Stream stream)
        {
            return RequestStream.FromStream(stream, 0, 1, true);
        }
    }
}
