using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Template.Infrastructure.DataAccess;

namespace Template.Api.IntegrationTests;

public abstract class BaseIntegrationTest : IClassFixture<TemplateApplicationFactory>, IDisposable
{
    internal readonly TemplateDbContext DbContext;

    protected TemplateApplicationFactory Factory { get; }

    private readonly HttpClient _httpClient;
    private readonly IServiceScope _scope;

    protected BaseIntegrationTest(TemplateApplicationFactory factory)
    {
        Factory = factory;
        _httpClient = factory.CreateClient();
        _scope = factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<TemplateDbContext>();
    }

    protected async Task<HttpResponseMessage> Post(string requestUri, object request) =>
        await _httpClient.PostAsJsonAsync(requestUri, request);

    protected async Task<HttpResponseMessage> Put(string requestUri, object request) =>
        await _httpClient.PutAsJsonAsync(requestUri, request);

    protected async Task<HttpResponseMessage> Get(string requestUri) =>
        await _httpClient.GetAsync(requestUri);

    protected async Task<HttpResponseMessage> Delete(string requestUri) =>
        await _httpClient.DeleteAsync(requestUri);

    public void Dispose()
    {
        _httpClient.Dispose();
        _scope.Dispose();
        DbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
