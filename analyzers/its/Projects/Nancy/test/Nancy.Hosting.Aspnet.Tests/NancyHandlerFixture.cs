namespace Nancy.Hosting.Aspnet.Tests
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    using FakeItEasy;

    using Nancy.Cookies;
    using Nancy.Helpers;

    using Xunit;

    public class NancyHandlerFixture
    {
        private readonly NancyHandler handler;
        private readonly HttpContextBase context;
        private readonly HttpRequestBase request;
        private readonly HttpResponseBase response;
        private readonly INancyEngine engine;
        private readonly NameValueCollection formData;

        public NancyHandlerFixture()
        {
            this.context = A.Fake<HttpContextBase>();
            this.request = A.Fake<HttpRequestBase>();
            this.response = A.Fake<HttpResponseBase>();
            this.engine = A.Fake<INancyEngine>();
            this.handler = new NancyHandler(engine);
            this.formData = new NameValueCollection();

            A.CallTo(() => this.request.Form).ReturnsLazily(() => this.formData);
            A.CallTo(() => this.request.Url).Returns(new Uri("http://www.foo.com"));
            A.CallTo(() => this.request.InputStream).Returns(new MemoryStream());
            A.CallTo(() => this.request.Headers).Returns(new NameValueCollection());
            A.CallTo(() => this.request.AppRelativeCurrentExecutionFilePath).Returns("~/foo");

            A.CallTo(() => this.context.Request).Returns(this.request);
            A.CallTo(() => this.context.Response).Returns(this.response);
            A.CallTo(() => this.response.OutputStream).Returns(new MemoryStream());
        }

        [Fact]
        public async Task Should_invoke_engine_with_requested_method()
        {
            // Given
            var nancyContext = new NancyContext {Response = new Response()};
            A.CallTo(() => this.request.HttpMethod).Returns("POST");
            A.CallTo(() => this.engine.HandleRequest(
                                        A<Request>.Ignored,
                                        A<Func<NancyContext, NancyContext>>.Ignored,
                                        A<CancellationToken>.Ignored))
                                      .Returns(Task.FromResult(nancyContext));

            // When
            await this.handler.ProcessRequest(this.context);

            // Then
            A.CallTo(() => this.engine.HandleRequest(A<Request>
                .That
                .Matches(x => x.Method.Equals("POST")), A<Func<NancyContext, NancyContext>>.Ignored, A<CancellationToken>.Ignored))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_output_the_responses_cookies()
        {
            // Given
            var cookie1 = A.Fake<INancyCookie>();
            var cookie2 = A.Fake<INancyCookie>();
            var r = new Response();
            r.WithCookie(cookie1).WithCookie(cookie2);
            var nancyContext = new NancyContext { Response = r };

            A.CallTo(() => cookie1.ToString()).Returns("the first cookie");
            A.CallTo(() => cookie2.ToString()).Returns("the second cookie");

            SetupRequestProcess(nancyContext);

            // When
            await this.handler.ProcessRequest(context);

            // Then
            A.CallTo(() => this.response.AddHeader("Set-Cookie", "the first cookie")).MustHaveHappened();
            A.CallTo(() => this.response.AddHeader("Set-Cookie", "the second cookie")).MustHaveHappened();
        }

        [Fact]
        public async Task Should_dispose_the_context()
        {
            // Given
            var disposable = A.Fake<IDisposable>();
            var nancyContext = new NancyContext() { Response = new Response() };
            nancyContext.Items.Add("Disposable", disposable);
            A.CallTo(() => this.request.HttpMethod).Returns("GET");
            A.CallTo(() => this.engine.HandleRequest(
                                        A<Request>.Ignored,
                                        A<Func<NancyContext, NancyContext>>.Ignored,
                                        A<CancellationToken>.Ignored))
                                      .Returns(Task.FromResult(nancyContext));

            // When
            await this.handler.ProcessRequest(this.context);

            // Then
            A.CallTo(() => disposable.Dispose()).MustHaveHappenedOnceExactly();
        }
        [Fact]
        public async Task Should_create_request_body_when_content_length_specified()
        {
            var bodyString = "This is a sample body";
            var nancyContext = new NancyContext() { Response = new Response() };
            A.CallTo(() => this.request.HttpMethod).Returns("POST");
            A.CallTo(() => this.request.Headers).Returns(new NameValueCollection { { "Content-Length", bodyString.Length.ToString() } });
            A.CallTo(() => this.request.InputStream).Returns(new MemoryStream(Encoding.UTF8.GetBytes(bodyString)));
            A.CallTo(() => this.request.Url).Returns(new Uri("http://ihatedummydata.com/about"));
            A.CallTo(() => this.engine.HandleRequest(
                                        A<Request>.Ignored,
                                        A<Func<NancyContext, NancyContext>>.Ignored,
                                        A<CancellationToken>.Ignored))
                                      .Returns(Task.FromResult(nancyContext));
 
            await this.handler.ProcessRequest(this.context);
 
            A.CallTo(() => this.engine.HandleRequest(A<Request>
                .That
                .Matches(x => x.Body.Length == bodyString.Length), A<Func<NancyContext, NancyContext>>.Ignored, A<CancellationToken>.Ignored))
                .MustHaveHappened();
        }
 
        [Fact]
        public async Task Should_not_create_request_body_when_content_length__not_specified_and_not_chunked()
        {
            var bodyString = "This is a sample body";
            var nancyContext = new NancyContext() { Response = new Response() };
            A.CallTo(() => this.request.HttpMethod).Returns("GET");
            A.CallTo(() => this.request.InputStream).Returns(new MemoryStream(Encoding.UTF8.GetBytes(bodyString)));
            A.CallTo(() => this.request.Url).Returns(new Uri("http://ihatedummydata.com/about"));
            A.CallTo(() => this.engine.HandleRequest(
                                        A<Request>.Ignored,
                                        A<Func<NancyContext, NancyContext>>.Ignored,
                                        A<CancellationToken>.Ignored))
                                      .Returns(Task.FromResult(nancyContext));
 
            await this.handler.ProcessRequest(this.context);
 
            A.CallTo(() => this.engine.HandleRequest(A<Request>
                .That
                .Matches(x => x.Body.Length == 0), A<Func<NancyContext, NancyContext>>.Ignored, A<CancellationToken>.Ignored))
                .MustHaveHappened();
        }
 
        [Fact]
        public async Task Should_create_request_body_when_content_length_not_specified_but_transfer_encoding_is_chunked()
        {
            var bodyString = "This is a sample body";
            var nancyContext = new NancyContext() { Response = new Response() };
            A.CallTo(() => this.request.HttpMethod).Returns("POST");
            A.CallTo(() => this.request.Headers).Returns(new NameValueCollection { { "Transfer-Encoding", "chunked" } });
            A.CallTo(() => this.request.InputStream).Returns(new MemoryStream(Encoding.UTF8.GetBytes(bodyString)));
            A.CallTo(() => this.request.Url).Returns(new Uri("http://ihatedummydata.com/about"));
            A.CallTo(() => this.engine.HandleRequest(
                                        A<Request>.Ignored,
                                        A<Func<NancyContext, NancyContext>>.Ignored,
                                        A<CancellationToken>.Ignored))
                                      .Returns(Task.FromResult(nancyContext));
 
            await this.handler.ProcessRequest(this.context);
 
            A.CallTo(() => this.engine.HandleRequest(A<Request>
                .That
                .Matches(x => x.Body.Length == bodyString.Length), A<Func<NancyContext, NancyContext>>.Ignored, A<CancellationToken>.Ignored))
                .MustHaveHappened();
        }

        private void SetupRequestProcess(NancyContext nancyContext)
        {
            A.CallTo(() => this.request.AppRelativeCurrentExecutionFilePath).Returns("~/about");
            A.CallTo(() => this.request.Url).Returns(new Uri("http://ihatedummydata.com/about"));
            A.CallTo(() => this.request.HttpMethod).Returns("GET");
            A.CallTo(() => this.engine.HandleRequest(
                                        A<Request>.Ignored,
                                        A<Func<NancyContext, NancyContext>>.Ignored,
                                        A<CancellationToken>.Ignored))
                                      .Returns(Task.FromResult(nancyContext));
        }
    }
}
