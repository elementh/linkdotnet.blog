﻿using System;
using System.Threading.Tasks;
using LinkDotNet.Blog.Infrastructure.Persistence;
using LinkDotNet.Blog.Web;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LinkDotNet.Blog.IntegrationTests;

public sealed class SmokeTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable, IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> factory;

    public SmokeTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("PersistenceProvider", PersistenceProvider.InMemory.Key);
        });
    }

    [Fact]
    public async Task ShouldBootUpApplication()
    {
        using var client = factory.CreateClient();

        var result = await client.GetAsync("/");

        result.IsSuccessStatusCode.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldBootUpWithSqlDatabase()
    {
        await using var sqlFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("PersistenceProvider", PersistenceProvider.Sqlite.Key);
            builder.UseSetting("ConnectionString", "DataSource=file::memory:?cache=shared");
        });
        using var client = sqlFactory.CreateClient();

        var result = await client.GetAsync("/");

        result.IsSuccessStatusCode.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldAllowDotsForTagSearch()
    {
        using var client = factory.CreateClient();

        var result = await client.GetAsync("/searchByTag/.NET5");

        result.IsSuccessStatusCode.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldAllowDotsForFreeTextSearch()
    {
        using var client = factory.CreateClient();

        var result = await client.GetAsync("/search/.NET5");

        result.IsSuccessStatusCode.ShouldBeTrue();
    }

    public void Dispose() => factory?.Dispose();

    public async ValueTask DisposeAsync()
    {
        if (factory is not null)
        {
            await factory.DisposeAsync();
        }
    }
}