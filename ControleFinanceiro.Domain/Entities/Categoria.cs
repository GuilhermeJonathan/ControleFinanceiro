using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

public class Categoria : Entity
{
    public string Nome { get; private set; }
    public TipoLancamento Tipo { get; private set; }

    public ICollection<Lancamento> Lancamentos { get; private set; } = [];

    private Categoria() : base(Guid.NewGuid()) { Nome = string.Empty; }

    public Categoria(string nome, TipoLancamento tipo) : base(Guid.NewGuid())
    {
        Nome = nome;
        Tipo = tipo;
    }

    public void Update(string nome, TipoLancamento tipo)
    {
        Nome = nome;
        Tipo = tipo;
        SetUpdated();
    }
}
