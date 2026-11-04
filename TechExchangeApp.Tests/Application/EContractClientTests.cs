using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TechExchangeApp.Configuration;
using TechExchangeApp.Interfaces;
using TechExchangeApp.Services.EContracts;
using Xunit;

namespace TechExchangeApp.Tests.Application;

public class EContractClientTests
{
    private const string Base = "https://econtract.test/service";
    private static readonly byte[] Pdf = Encoding.ASCII.GetBytes("%PDF-1.7 test");

    private sealed record Captured(HttpMethod Method, string Url, string? Bearer, string? Body, byte[]? Raw, string? ContentType);

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();
        public List<Captured> Calls { get; } = new();

        public void Enqueue(HttpStatusCode status, string body)
            => _responses.Enqueue(_ => new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") });

        public void Enqueue(Func<HttpRequestMessage, HttpResponseMessage> response) => _responses.Enqueue(response);

        public void EnqueueLogin(string token = "tok1")
            => Enqueue(HttpStatusCode.OK, JsonSerializer.Serialize(new { code = "00", access_token = token }));

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var raw = request.Content == null ? null : await request.Content.ReadAsByteArrayAsync(cancellationToken);
            Calls.Add(new Captured(request.Method, request.RequestUri!.ToString(), request.Headers.Authorization?.Parameter,
                raw == null ? null : Encoding.UTF8.GetString(raw), raw, request.Content?.Headers.ContentType?.MediaType));
            return _responses.Dequeue()(request);
        }
    }

    private static EContractClient NewClient(StubHandler handler, string? downloadHosts = null, string? baseUrl = Base, string email = "tk@test")
    {
        var parameters = new Mock<ISystemParameterService>();
        parameters.Setup(p => p.GetAsync(ParameterKeys.EContractBaseUrl)).ReturnsAsync(baseUrl);
        parameters.Setup(p => p.GetAsync(ParameterKeys.EContractEmail)).ReturnsAsync(email);
        parameters.Setup(p => p.GetAsync(ParameterKeys.EContractPassword)).ReturnsAsync("secret");
        parameters.Setup(p => p.GetAsync(ParameterKeys.EContractDownloadHosts)).ReturnsAsync(downloadHosts);

        return new EContractClient(new HttpClient(handler), parameters.Object,
            new MemoryCache(new MemoryCacheOptions()), NullLogger<EContractClient>.Instance);
    }

    private static EContractCreateRequest SampleRequest() => new()
    {
        Contract = new EContractInfo
        {
            Name = "Hợp đồng",
            ContractNo = "HD-1",
            SignTime = new DateTime(2026, 11, 24, 0, 0, 0, DateTimeKind.Utc)
        },
        Participants =
        {
            new EContractParticipant
            {
                Name = "Bên A",
                Type = 1,
                Recipients =
                {
                    new EContractRecipientRequest
                    {
                        Name = "Người ký",
                        Email = "a@test",
                        CardId = "123",
                        SignType = { new EContractSignType { Id = EContractConstants.SignTypeUsbToken } }
                    }
                }
            }
        },
        Fields = { new EContractFieldRequest { Name = "Ký", EmailOrPhone = "a@test", Page = 1, CoordinateX = 50, CoordinateY = 100, Width = 180, Height = 66 } }
    };

    [Fact]
    public async Task Create_SendsWireFormat_AndReturnsId()
    {
        var handler = new StubHandler();
        handler.EnqueueLogin();
        handler.Enqueue(HttpStatusCode.Created, """{"id":123,"status":0}""");

        var result = await NewClient(handler).CreateContractAsync(SampleRequest(), Pdf, "hd.pdf");

        result.Id.Should().Be(123);

        var login = handler.Calls[0];
        login.Url.Should().Be(Base + "/api/v1/auth");
        using (var doc = JsonDocument.Parse(login.Body!))
        {
            doc.RootElement.GetProperty("email").GetString().Should().Be("tk@test");
            doc.RootElement.GetProperty("type").GetInt32().Should().Be(0);
        }

        var create = handler.Calls[1];
        create.Url.Should().Be(Base + "/api/v1/tp/contracts/create");
        create.Bearer.Should().Be("tok1");
        create.ContentType.Should().Be("multipart/form-data");
        create.Body.Should().Contain("name=file_contract");
        create.Body.Should().Contain("name=jsonRequest");
        create.Body.Should().Contain("\"sign_time\":\"2026-11-24T00:00:00Z\"");
        create.Body.Should().Contain("\"contract_no\":\"HD-1\"");
        create.Body.Should().Contain("\"sign_type\":[{\"id\":2}]");
        create.Body.Should().Contain("\"email_or_phone\":\"a@test\"");
        create.Body.Should().Contain("\"coordinate_y\":100");
        create.Body.Should().Contain("\"is_template\":false");
    }

    [Fact]
    public async Task Create_RejectsNonPdf_WithoutCallingApi()
    {
        var handler = new StubHandler();

        var act = () => NewClient(handler).CreateContractAsync(SampleRequest(), Encoding.ASCII.GetBytes("not a pdf"), "x.pdf");

        (await act.Should().ThrowAsync<EContractException>()).Which.Kind.Should().Be(EContractFailureKind.Rejected);
        handler.Calls.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_201WithSuccessFalse_IsRejected()
    {
        var handler = new StubHandler();
        handler.EnqueueLogin();
        handler.Enqueue(HttpStatusCode.Created, """{"success":false,"message":"Trùng số hợp đồng"}""");

        var act = () => NewClient(handler).CreateContractAsync(SampleRequest(), Pdf, "hd.pdf");

        var ex = (await act.Should().ThrowAsync<EContractException>()).Which;
        ex.Kind.Should().Be(EContractFailureKind.Rejected);
        ex.Message.Should().Contain("Trùng số hợp đồng");
    }

    [Fact]
    public async Task Create_Timeout_IsUnknown_AndNotResent()
    {
        var handler = new StubHandler();
        handler.EnqueueLogin();
        handler.Enqueue(_ => throw new TaskCanceledException());

        var act = () => NewClient(handler).CreateContractAsync(SampleRequest(), Pdf, "hd.pdf");

        (await act.Should().ThrowAsync<EContractException>()).Which.Kind.Should().Be(EContractFailureKind.Unknown);
        handler.Calls.Count(c => c.Url.EndsWith("/tp/contracts/create")).Should().Be(1);
    }

    [Fact]
    public async Task Create_ServerError_IsUnknown_AndNotResent()
    {
        var handler = new StubHandler();
        handler.EnqueueLogin();
        handler.Enqueue(HttpStatusCode.InternalServerError, "{}");

        var act = () => NewClient(handler).CreateContractAsync(SampleRequest(), Pdf, "hd.pdf");

        (await act.Should().ThrowAsync<EContractException>()).Which.Kind.Should().Be(EContractFailureKind.Unknown);
        handler.Calls.Count(c => c.Url.EndsWith("/tp/contracts/create")).Should().Be(1);
    }

    [Fact]
    public async Task Create_ResponseWithoutId_IsUnknown()
    {
        var handler = new StubHandler();
        handler.EnqueueLogin();
        handler.Enqueue(HttpStatusCode.OK, """{"message":"ok"}""");

        var act = () => NewClient(handler).CreateContractAsync(SampleRequest(), Pdf, "hd.pdf");

        (await act.Should().ThrowAsync<EContractException>()).Which.Kind.Should().Be(EContractFailureKind.Unknown);
    }

    [Fact]
    public async Task Unauthorized_LogsInAgain_AndRetriesOnce()
    {
        var handler = new StubHandler();
        handler.EnqueueLogin("old");
        handler.Enqueue(HttpStatusCode.Unauthorized, "{}");
        handler.EnqueueLogin("new");
        handler.Enqueue(HttpStatusCode.OK, """{"success":true}""");

        await NewClient(handler).IssueContractAsync(123);

        handler.Calls.Should().HaveCount(4);
        handler.Calls[3].Bearer.Should().Be("new");
        handler.Calls[3].Body.Should().Be("[123]");
    }

    [Fact]
    public async Task TokenIsCached_BetweenCalls()
    {
        var handler = new StubHandler();
        handler.EnqueueLogin();
        handler.Enqueue(HttpStatusCode.OK, "[]");
        handler.Enqueue(HttpStatusCode.OK, "[]");
        var client = NewClient(handler);

        await client.GetDocumentsAsync(1);
        await client.GetDocumentsAsync(2);

        handler.Calls.Count(c => c.Url.EndsWith("/auth")).Should().Be(1);
    }

    [Theory]
    [InlineData("""{"code":"99","access_token":null}""")]
    [InlineData("""{"code":"00","access_token":null}""")]
    [InlineData("not json")]
    public async Task Login_Failure_IsAuthentication_AndBusinessApiNotCalled(string loginBody)
    {
        var handler = new StubHandler();
        handler.Enqueue(HttpStatusCode.OK, loginBody);

        var act = () => NewClient(handler).IssueContractAsync(1);

        (await act.Should().ThrowAsync<EContractException>()).Which.Kind.Should().Be(EContractFailureKind.Authentication);
        handler.Calls.Should().HaveCount(1);
    }

    [Fact]
    public async Task Issue_WithoutSuccessTrue_IsUnknown()
    {
        var handler = new StubHandler();
        handler.EnqueueLogin();
        handler.Enqueue(HttpStatusCode.OK, """{"message":"?"}""");

        var act = () => NewClient(handler).IssueContractAsync(1);

        (await act.Should().ThrowAsync<EContractException>()).Which.Kind.Should().Be(EContractFailureKind.Unknown);
    }

    [Fact]
    public async Task Issue_SuccessFalse_IsRejected()
    {
        var handler = new StubHandler();
        handler.EnqueueLogin();
        handler.Enqueue(HttpStatusCode.OK, """{"success":false,"message":"Hết gói"}""");

        var act = () => NewClient(handler).IssueContractAsync(1);

        (await act.Should().ThrowAsync<EContractException>()).Which.Kind.Should().Be(EContractFailureKind.Rejected);
    }

    [Fact]
    public async Task Read_RetriesOnServerError()
    {
        var handler = new StubHandler();
        handler.EnqueueLogin();
        handler.Enqueue(HttpStatusCode.ServiceUnavailable, "{}");
        handler.Enqueue(HttpStatusCode.OK, """[{"id":5,"type":2,"path":"https://econtract.test/f.pdf"}]""");

        var docs = await NewClient(handler).GetDocumentsAsync(1);

        docs.Should().ContainSingle(d => d.Type == EContractConstants.DocumentTypeFinally && d.Id == 5);
    }

    [Fact]
    public async Task Search_EscapesQuery_AndReadsEntities()
    {
        var handler = new StubHandler();
        handler.EnqueueLogin();
        handler.Enqueue(HttpStatusCode.OK, """{"total_pages":1,"total_elements":1,"entities":[{"id":123,"status":30}]}""");

        var page = await NewClient(handler).SearchContractsAsync("a+b@test", null, 0, 20);

        page.Entities.Should().ContainSingle(c => c.Id == 123 && c.Status == EContractConstants.ContractSigned);
        handler.Calls[1].Url.Should().Be(Base + "/api/v1/tp/contracts?contractStatus=-1&email=a%2Bb%40test&page=0&size=20");
    }

    [Fact]
    public async Task Download_StorageHost_NeverReceivesToken()
    {
        var handler = new StubHandler();
        handler.Enqueue(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Pdf) });

        var bytes = await NewClient(handler, downloadHosts: "storage.test").DownloadPdfAsync("https://storage.test/a.pdf?sig=abc");

        bytes.Should().Equal(Pdf);
        handler.Calls.Should().ContainSingle();
        handler.Calls[0].Bearer.Should().BeNull();
    }

    [Fact]
    public async Task Download_EContractHost_UsesToken()
    {
        var handler = new StubHandler();
        handler.EnqueueLogin();
        handler.Enqueue(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Pdf) });

        await NewClient(handler).DownloadPdfAsync("https://econtract.test/service/file/1");

        handler.Calls[1].Bearer.Should().Be("tok1");
    }

    [Theory]
    [InlineData("https://evil.test/a.pdf", EContractFailureKind.Configuration)]
    [InlineData("http://econtract.test/a.pdf", EContractFailureKind.Rejected)]
    [InlineData("/relative/a.pdf", EContractFailureKind.Rejected)]
    public async Task Download_RejectsDisallowedUrl_WithoutCalling(string url, EContractFailureKind kind)
    {
        var handler = new StubHandler();

        var act = () => NewClient(handler).DownloadPdfAsync(url);

        (await act.Should().ThrowAsync<EContractException>()).Which.Kind.Should().Be(kind);
        handler.Calls.Should().BeEmpty();
    }

    [Fact]
    public async Task Download_RedirectToUnlistedHost_IsBlocked()
    {
        var handler = new StubHandler();
        handler.Enqueue(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.Found);
            response.Headers.Location = new Uri("https://evil.test/a.pdf");
            return response;
        });

        var act = () => NewClient(handler, downloadHosts: "storage.test").DownloadPdfAsync("https://storage.test/a.pdf");

        await act.Should().ThrowAsync<EContractException>();
        handler.Calls.Should().ContainSingle();
    }

    [Fact]
    public async Task Download_NonPdfBody_IsRejected()
    {
        var handler = new StubHandler();
        handler.Enqueue(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("<html>") });

        var act = () => NewClient(handler, downloadHosts: "storage.test").DownloadPdfAsync("https://storage.test/a.pdf");

        (await act.Should().ThrowAsync<EContractException>()).Which.Kind.Should().Be(EContractFailureKind.Rejected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("http://econtract.test/service")]
    public async Task BaseUrl_MustBeHttps(string baseUrl)
    {
        var handler = new StubHandler();

        var act = () => NewClient(handler, baseUrl: baseUrl).IssueContractAsync(1);

        (await act.Should().ThrowAsync<EContractException>()).Which.Kind.Should().Be(EContractFailureKind.Configuration);
        handler.Calls.Should().BeEmpty();
    }
}
