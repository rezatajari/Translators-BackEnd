namespace Translators.Domain.Users;

public class RoleEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Permissions { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();

    public bool HasAccess(string apiHandlerName)
    {
        if (string.IsNullOrWhiteSpace(apiHandlerName))
        {
            return false;
        }

        if (string.Equals(Name, RoleNames.Admin, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(Permissions, "*", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var permissions = Permissions
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return permissions.Any(permission =>
            string.Equals(permission, apiHandlerName, StringComparison.OrdinalIgnoreCase));
    }
}
