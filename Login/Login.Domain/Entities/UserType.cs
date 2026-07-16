namespace Login.Domain.Entities;

public enum UserType
{
    Admin = 1,
    User = 2,
    /// <summary>Assessor financeiro — gerencia carteira de clientes via VinculoAssessoria.</summary>
    Assessor = 3,
    /// <summary>Corretor — atende clientes delegados por um assessor via VinculoCorretor/DelegacaoCarteira.</summary>
    Corretor = 4
}
