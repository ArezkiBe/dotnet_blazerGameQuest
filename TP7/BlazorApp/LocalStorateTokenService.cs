using Microsoft.JSInterop;

public class LocalStorageTokenService : ITokenService
{
    private readonly IJSRuntime _js;
    public const string TokenKey = "access_token";

    public LocalStorageTokenService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _js.InvokeAsync<string>("localStorage.getItem", TokenKey);
    }

    public async Task SetTokenAsync(string? token)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
    }

    public async Task RemoveTokenAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
    }
}
