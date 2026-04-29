using Login.Domain.Common;

namespace Login.Domain.Entities;

public class User : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Document { get; private set; } = string.Empty;
    public string? Cellphone { get; private set; }
    public string? Phone { get; private set; }
    public string? Occupation { get; private set; }
    public string? Address { get; private set; }
    public string? AvatarUrl { get; private set; }
    public UserType UserTypeId { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsBlocked { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public Guid? ProfileId { get; private set; }
    public int? CountryId { get; private set; }
    public string? Region { get; private set; }
    public DateTime? UltimoLogin { get; private set; }

    // Navigation
    public Profile? Profile { get; private set; }
    public ICollection<UserRestriction> Restrictions { get; private set; } = new List<UserRestriction>();

    private User() : base(Guid.Empty) { }

    public User(
        Guid id,
        string name,
        string email,
        string document,
        string passwordHash,
        UserType userTypeId,
        Guid? profileId = null)
        : base(id)
    {
        Name = name;
        Email = email;
        Document = document;
        PasswordHash = passwordHash;
        UserTypeId = userTypeId;
        ProfileId = profileId;
        IsActive = true;
        IsBlocked = false;
    }

    public void UpdateProfile(
        string name,
        string? occupation,
        string? address,
        string? cellphone,
        string? phone,
        int? countryId,
        string? region)
    {
        Name = name;
        Occupation = occupation;
        Address = address;
        Cellphone = cellphone;
        Phone = phone;
        CountryId = countryId;
        Region = region;
        SetUpdated();
    }

    public void UpdateAvatar(string? avatarUrl)
    {
        AvatarUrl = avatarUrl;
        SetUpdated();
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        SetUpdated();
    }

    public void Block()
    {
        IsBlocked = true;
        SetUpdated();
    }

    public void Unblock()
    {
        IsBlocked = false;
        SetUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdated();
    }

    public void RegisterLogin()
    {
        UltimoLogin = DateTime.UtcNow;
        SetUpdated();
    }
}
