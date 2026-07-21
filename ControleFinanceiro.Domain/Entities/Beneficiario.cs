namespace ControleFinanceiro.Domain.Entities;

/// <summary>Papel do beneficiário na estrutura/trust.</summary>
public enum PapelBeneficiario
{
    Conjuge = 1,
    Filho = 2,
    Neto = 3,
    Outro = 99,
}

/// <summary>Beneficiário da família (do cliente) — não precisa ter conta no app.</summary>
public class Beneficiario
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UsuarioId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public PapelBeneficiario Papel { get; private set; }
    /// <summary>Percentual de distribuição (0–100).</summary>
    public decimal PercentualDistribuicao { get; private set; }
    /// <summary>Termos/condição de liberação. Ex: "aos 25 anos, 20% do principal".</summary>
    public string? CondicaoLiberacao { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; private set; }

    private Beneficiario() { }

    public Beneficiario(Guid usuarioId, string nome, PapelBeneficiario papel,
        decimal percentualDistribuicao, string? condicaoLiberacao = null)
    {
        UsuarioId = usuarioId;
        Nome = nome;
        Papel = papel;
        PercentualDistribuicao = percentualDistribuicao;
        CondicaoLiberacao = condicaoLiberacao;
    }

    public void Atualizar(string nome, PapelBeneficiario papel, decimal percentualDistribuicao, string? condicaoLiberacao)
    {
        Nome = nome;
        Papel = papel;
        PercentualDistribuicao = percentualDistribuicao;
        CondicaoLiberacao = condicaoLiberacao;
        AtualizadoEm = DateTime.UtcNow;
    }
}
