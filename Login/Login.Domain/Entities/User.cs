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

    // ── Plano / Trial ────────────────────────────────────────────────────────
    public PlanType PlanType { get; private set; }
    public DateTime? TrialStartedAt { get; private set; }
    public DateTime? PlanExpiresAt { get; private set; }

    // ── Segurança de token ───────────────────────────────────────────────────
    /// <summary>
    /// Tokens emitidos ANTES deste instante são considerados revogados.
    /// Null = nenhuma revogação ativa.
    /// </summary>
    public DateTime? TokenRevokedAt { get; private set; }

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

    public void UpdateDocument(string? document)
    {
        if (!string.IsNullOrWhiteSpace(document))
            Document = document;
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

    /// <summary>Revoga todos os tokens emitidos até este momento.</summary>
    public void RevokeTokens()
    {
        TokenRevokedAt = DateTime.UtcNow;
        SetUpdated();
    }

    /// <summary>Inicia o período de trial de 30 dias para novos usuários.</summary>
    public void StartTrial()
    {
        if (TrialStartedAt is null)
        {
            TrialStartedAt = DateTime.UtcNow;
            PlanType = PlanType.Trial;
            SetUpdated();
        }
    }

    /// <summary>Admin: define trial com duração customizada em dias a partir de agora.</summary>
    public void AdminSetTrial(int days)
    {
        TrialStartedAt ??= DateTime.UtcNow;          // preserva data original se já existir
        PlanType      = PlanType.Trial;
        PlanExpiresAt = DateTime.UtcNow.AddDays(days); // expira em exatamente `days` dias
        SetUpdated();
    }

    /// <summary>Admin: remove o plano e o trial do usuário.</summary>
    public void AdminClearPlan()
    {
        PlanType = PlanType.None;
        TrialStartedAt = null;
        PlanExpiresAt = null;
        SetUpdated();
    }

    /// <summary>Ativa um plano pago.</summary>
    public void SetPlan(PlanType planType, DateTime expiresAt)
    {
        PlanType = planType;
        PlanExpiresAt = expiresAt;
        SetUpdated();
    }

    /// <summary>Calcula o status atual do plano.</summary>
    public PlanStatus GetPlanStatus()
    {
        var now = DateTime.UtcNow;

        // Plano pago ativo
        if (PlanType is PlanType.Monthly or PlanType.Annual)
        {
            var paid = PlanExpiresAt is null || PlanExpiresAt > now;
            return new PlanStatus(
                HasPaidPlan: paid,
                IsTrialActive: false,
                IsTrialExpired: false,
                TrialDaysRemaining: null,
                TrialEndsAt: null,
                PlanExpiresAt: PlanExpiresAt);
        }

        // Trial em andamento ou expirado
        if (TrialStartedAt is not null)
        {
            // Admin pode ter definido PlanExpiresAt com duração customizada
            var trialEnd = PlanExpiresAt ?? TrialStartedAt.Value.AddDays(30);
            var daysLeft = (int)Math.Ceiling((trialEnd - now).TotalDays);
            var active = daysLeft > 0;
            return new PlanStatus(
                HasPaidPlan: false,
                IsTrialActive: active,
                IsTrialExpired: !active,
                TrialDaysRemaining: active ? daysLeft : 0,
                TrialEndsAt: trialEnd,
                PlanExpiresAt: null);
        }

        // Sem plano e sem trial (usuário antigo, migrado) → trata como trial ativo
        return new PlanStatus(
            HasPaidPlan: true,
            IsTrialActive: false,
            IsTrialExpired: false,
            TrialDaysRemaining: null,
            TrialEndsAt: null,
            PlanExpiresAt: null);
    }
}

public record PlanStatus(
    bool HasPaidPlan,
    bool IsTrialActive,
    bool IsTrialExpired,
    int? TrialDaysRemaining,
    DateTime? TrialEndsAt,
    DateTime? PlanExpiresAt
);
