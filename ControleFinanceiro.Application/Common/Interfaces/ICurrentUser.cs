namespace ControleFinanceiro.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid UserId { get; }
    Guid RealUserId { get; }
    string? RealUserName { get; }
    /// <summary>Perfil Assessor (userType=3). Admin (userType=1) também é considerado assessor.</summary>
    bool IsAssessor { get; }
    /// <summary>Assinatura do plano Assessor ativa (claim planType=4) — libera carteira ilimitada.</summary>
    bool TemPlanoAssessor { get; }
    /// <summary>Perfil Corretor (userType=4) — subordinado de um assessor.</summary>
    bool IsCorretor { get; }
}
