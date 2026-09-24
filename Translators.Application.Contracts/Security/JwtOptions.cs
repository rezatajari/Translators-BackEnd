namespace Translators.Application.Contracts.Security;

public class JwtOptions
{
    public string Issuer { get; set; } = "Translators";
    public string Audience { get; set; } = "Translators";
    public string SigningKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 1440;
}
