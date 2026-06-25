using Fcs.UI.Application.Interfaces;
using Microsoft.JSInterop;

namespace Fcs.UI.Web.Infrastructure;

public sealed class WebTokenStorage : ITokenStorage
{
    private readonly IJSRuntime _js;

    public WebTokenStorage(IJSRuntime js) => _js = js;

    public async Task<string?> GetAccessTokenAsync()
    {
        return await _js.InvokeAsync<string?>("localStorage.getItem", "access_token");
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        return await _js.InvokeAsync<string?>("localStorage.getItem", "refresh_token");
    }

    public async Task SaveTokensAsync(string accessToken, string refreshToken)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", "access_token", accessToken);
        await _js.InvokeVoidAsync("localStorage.setItem", "refresh_token", refreshToken);
    }

    public async Task ClearTokensAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", "access_token");
        await _js.InvokeVoidAsync("localStorage.removeItem", "refresh_token");
    }
}
