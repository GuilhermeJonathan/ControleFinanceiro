namespace Login.Domain.Entities;

public enum UserType
{
    Admin = 1,
    User = 2,
    /// <summary>Assessor financeiro — gerencia carteira de clientes via VinculoAssessoria.</summary>
    Assessor = 3
}
