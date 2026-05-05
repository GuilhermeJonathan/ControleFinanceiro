using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

public class Categoria : Entity
{
    public string Nome { get; private set; }
    public TipoLancamento Tipo { get; private set; }
    public decimal? LimiteMensal { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string? Icone { get; private set; }
    public string? Cor { get; private set; }

    public ICollection<Lancamento> Lancamentos { get; private set; } = [];

    private Categoria() : base(Guid.NewGuid()) { Nome = string.Empty; }

    public Categoria(string nome, TipoLancamento tipo, Guid usuarioId = default, string? icone = null, string? cor = null) : base(Guid.NewGuid())
    {
        UsuarioId = usuarioId;
        Nome = nome;
        Tipo = tipo;
        Icone = icone;
        Cor = cor;
    }

    public void Update(string nome, TipoLancamento tipo, string? icone = null, string? cor = null)
    {
        Nome = nome;
        Tipo = tipo;
        Icone = icone;
        Cor = cor;
        SetUpdated();
    }

    public void AtualizarLimite(decimal? limite)
    {
        LimiteMensal = limite;
        SetUpdated();
    }
}
