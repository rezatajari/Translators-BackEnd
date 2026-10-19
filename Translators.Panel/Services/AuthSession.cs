using Translators.GeneratedServices;

namespace Translators.Panel.Services;

public sealed class AuthSession
{
    public LoginUserResponseContract? CurrentUser { get; private set; }

    public string? Token => CurrentUser?.Token;

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);

    public void SignIn(LoginUserResponseContract user) => CurrentUser = user;

    public void SignOut() => CurrentUser = null;
}
