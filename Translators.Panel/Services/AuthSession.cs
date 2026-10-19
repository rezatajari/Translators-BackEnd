using Translators.Application.Contracts.Users;

namespace Translators.Panel.Services;

public sealed class AuthSession
{
    public LoginUserResponseContract? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser is not null && !string.IsNullOrWhiteSpace(CurrentUser.Token);

    public void SignIn(LoginUserResponseContract user) => CurrentUser = user;

    public void SignOut() => CurrentUser = null;
}