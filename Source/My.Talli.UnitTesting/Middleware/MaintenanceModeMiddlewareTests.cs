namespace My.Talli.UnitTesting.Middleware;

using Microsoft.AspNetCore.Http;
using My.Talli.UnitTesting.Infrastructure.Stubs;
using My.Talli.Web.Middleware;
using System.Security.Claims;

/// <summary>Tests</summary>
public class MaintenanceModeMiddlewareTests
{
    #region <Methods>

    [Fact]
    public async Task Invoke_MmOff_PassesThroughToNext()
    {
        var (middleware, service, nextCalled) = BuildMiddleware();
        service.IsEnabled = false;
        var context = BuildContext("/dashboard", isAdmin: false);

        await middleware.InvokeAsync(context, service);

        Assert.True(nextCalled.Value);
        Assert.NotEqual(302, context.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_MmOn_NonAdmin_RedirectsToMaintenance()
    {
        var (middleware, service, nextCalled) = BuildMiddleware();
        service.IsEnabled = true;
        var context = BuildContext("/dashboard", isAdmin: false);

        await middleware.InvokeAsync(context, service);

        Assert.False(nextCalled.Value);
        Assert.Equal(302, context.Response.StatusCode);
        Assert.Equal("/maintenance", context.Response.Headers.Location);
    }

    [Fact]
    public async Task Invoke_MmOn_Admin_PassesThroughToNext()
    {
        var (middleware, service, nextCalled) = BuildMiddleware();
        service.IsEnabled = true;
        var context = BuildContext("/dashboard", isAdmin: true);

        await middleware.InvokeAsync(context, service);

        Assert.True(nextCalled.Value);
        Assert.NotEqual(302, context.Response.StatusCode);
    }

    [Theory]
    [InlineData("/maintenance")]
    [InlineData("/maintenance/something")]
    [InlineData("/api/admin/maintenance/on")]
    [InlineData("/api/admin/maintenance/off")]
    [InlineData("/api/admin/maintenance/status")]
    [InlineData("/_blazor")]
    [InlineData("/_blazor/disconnect")]
    [InlineData("/_framework/blazor.web.js")]
    [InlineData("/css/site.css")]
    [InlineData("/js/theme.js")]
    [InlineData("/lib/bootstrap/dist/css/bootstrap.css")]
    [InlineData("/Error")]
    [InlineData("/Error/500")]
    public async Task Invoke_MmOn_NonAdmin_WhitelistedPath_PassesThrough(string path)
    {
        var (middleware, service, nextCalled) = BuildMiddleware();
        service.IsEnabled = true;
        var context = BuildContext(path, isAdmin: false);

        await middleware.InvokeAsync(context, service);

        Assert.True(nextCalled.Value);
        Assert.NotEqual(302, context.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_MmOn_NonAdmin_UnauthenticatedUser_StillRedirects()
    {
        var (middleware, service, nextCalled) = BuildMiddleware();
        service.IsEnabled = true;
        var context = BuildContext("/dashboard", isAdmin: false, authenticated: false);

        await middleware.InvokeAsync(context, service);

        Assert.False(nextCalled.Value);
        Assert.Equal(302, context.Response.StatusCode);
    }

    private static (MaintenanceModeMiddleware middleware, MaintenanceModeServiceStub service, NextCalled nextCalled) BuildMiddleware()
    {
        var nextCalled = new NextCalled();
        var middleware = new MaintenanceModeMiddleware(_ =>
        {
            nextCalled.Value = true;
            return Task.CompletedTask;
        });

        return (middleware, new MaintenanceModeServiceStub(), nextCalled);
    }

    private static DefaultHttpContext BuildContext(string path, bool isAdmin, bool authenticated = true)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        if (authenticated)
        {
            var claims = new List<Claim> { new(ClaimTypes.Name, "test-user") };

            if (isAdmin)
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));

            context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        return context;
    }

    private sealed class NextCalled
    {
        public bool Value;
    }

    #endregion
}
