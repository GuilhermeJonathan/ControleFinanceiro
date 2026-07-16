namespace ControleFinanceiro.Application.Assessoria.Commands.AceitarConvitePublico;

/// <summary>
/// Espelha os ids de UserType da Login usados no provisionamento por convite.
/// (Login.Domain.UserType: User=2, Corretor=4). Mantido aqui para a Application
/// não depender do assembly da Login.
/// </summary>
public enum UserTypeConvite
{
    Cliente = 2,
    Corretor = 4,
}
