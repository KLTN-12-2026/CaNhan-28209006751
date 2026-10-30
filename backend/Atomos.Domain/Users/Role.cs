namespace Atomos.Domain.Users;

public sealed class Role
{
    public int IDRole { get; set; }
    public string TenRole { get; set; } = string.Empty;
    public ICollection<User> Users { get; set; } = [];
}
