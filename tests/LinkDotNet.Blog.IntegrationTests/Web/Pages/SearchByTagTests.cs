﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Bunit.TestDoubles;
using LinkDotNet.Blog.Domain;
using LinkDotNet.Blog.Infrastructure.Persistence;
using LinkDotNet.Blog.TestUtilities;
using LinkDotNet.Blog.Web.Features.SearchByTag;
using LinkDotNet.Blog.Web.Features.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace LinkDotNet.Blog.IntegrationTests.Web.Pages;

public class SearchByTagTests : SqlDatabaseTestBase<BlogPost>
{
    [Fact]
    public async Task ShouldOnlyDisplayTagsGivenByParameter()
    {
        using var ctx = new TestContext();
        await AddBlogPostWithTagAsync("Tag 1");
        await AddBlogPostWithTagAsync("Tag 1");
        await AddBlogPostWithTagAsync("Tag 2");
        ctx.Services.AddScoped<IRepository<BlogPost>>(_ => Repository);
        ctx.Services.AddScoped(_ => Mock.Of<IUserRecordService>());
        var cut = ctx.RenderComponent<SearchByTag>(p => p.Add(s => s.Tag, "Tag 1"));
        cut.WaitForState(() => cut.FindAll(".blog-card").Any());

        var tags = cut.FindAll(".blog-card");

        tags.Should().HaveCount(2);
    }

    [Fact]
    public async Task ShouldHandleSpecialCharacters()
    {
        using var ctx = new TestContext();
        await AddBlogPostWithTagAsync("C#");
        ctx.Services.AddScoped<IRepository<BlogPost>>(_ => Repository);
        ctx.Services.AddScoped(_ => Mock.Of<IUserRecordService>());
        var cut = ctx.RenderComponent<SearchByTag>(p => p.Add(s => s.Tag, Uri.EscapeDataString("C#")));
        cut.WaitForState(() => cut.FindAll(".blog-card").Any());

        var tags = cut.FindAll(".blog-card");

        tags.Should().HaveCount(1);
    }

    [Fact]
    public void ShouldSetTitleToTag()
    {
        using var ctx = new TestContext();
        ctx.Services.AddScoped<IRepository<BlogPost>>(_ => Repository);
        ctx.Services.AddScoped(_ => Mock.Of<IUserRecordService>());
        ctx.ComponentFactories.AddStub<PageTitle>();

        var cut = ctx.RenderComponent<SearchByTag>(p => p.Add(s => s.Tag, "Tag"));

        var pageTitleStub = cut.FindComponent<Stub<PageTitle>>();
        var pageTitle = ctx.Render(pageTitleStub.Instance.Parameters.Get(p => p.ChildContent));
        pageTitle.Markup.Should().Be("Search for tag: Tag");
    }

    private async Task AddBlogPostWithTagAsync(string tag)
    {
        var blogPost = new BlogPostBuilder().WithTags(tag).Build();
        await DbContext.AddAsync(blogPost);
        await DbContext.SaveChangesAsync();
    }
}