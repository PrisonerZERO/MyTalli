using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Diagnostics;
using My.Talli.Domain.Exceptions;
using System.Diagnostics;

namespace My.Talli.Web.ViewModels.Pages;

public class ErrorViewModel : ComponentBase
{
    #region <Variables>

    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    [Inject]
    private IHostEnvironment Environment { get; set; } = default!;

    #endregion

    #region <Properties>

    public string ErrorDescription { get; private set; } = string.Empty;

    public string ErrorIcon { get; private set; } = string.Empty;

    public string ErrorTitle { get; private set; } = string.Empty;

    public string? RequestId { get; private set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId)
                                 && Environment.IsDevelopment()
                                 && HttpContext?.Features.Get<IExceptionHandlerFeature>() is not null;

    [Parameter]
    public int? StatusCode { get; set; }

    #endregion

    #region <Events>

    protected override void OnInitialized()
    {
        ResolveStatusCode();
        SetErrorContent();
        CaptureRequestId();
    }

    #endregion

    #region <Methods>

    private void CaptureRequestId()
    {
        RequestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
    }

    private void ResolveStatusCode()
    {
        if (StatusCode.HasValue) return;

        var exceptionFeature = HttpContext?.Features.Get<IExceptionHandlerFeature>();
        if (exceptionFeature?.Error is TalliException talliException)
        {
            StatusCode = talliException.HttpStatusCode;
            return;
        }

        StatusCode = HttpContext?.Response.StatusCode ?? 500;
    }

    private void SetErrorContent()
    {
        (ErrorTitle, ErrorDescription, ErrorIcon) = StatusCode switch
        {
            400 => (
                "Bad Request",
                "The request couldn't be understood. Please check and try again.",
                "warning-triangle"
            ),
            401 => (
                "Sign In Required",
                "You need to sign in to access this page.",
                "lock"
            ),
            403 => (
                "Access Denied",
                "You don't have permission to view this page.",
                "shield"
            ),
            404 => (
                "Page Not Found",
                "The page you're looking for doesn't exist or has been moved.",
                "search"
            ),
            408 => (
                "Request Timeout",
                "The server took too long to respond. Please try again.",
                "clock"
            ),
            500 => (
                "Something Went Wrong",
                "We hit an unexpected error. Our team has been notified.",
                "alert-circle"
            ),
            502 => (
                "Bad Gateway",
                "We're having trouble connecting to our servers. Please try again shortly.",
                "cloud-off"
            ),
            503 => (
                "Under Maintenance",
                "MyTalli is temporarily unavailable. We'll be back shortly.",
                "tool"
            ),
            _ => (
                "Unexpected Error",
                "Something unexpected happened. Please try again or head back to the homepage.",
                "alert-circle"
            )
        };
    }

    #endregion
}
