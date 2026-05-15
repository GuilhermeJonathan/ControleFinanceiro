using ControleFinanceiro.Domain.Common;

namespace ControleFinanceiro.Domain.Entities;

public class Produto : Entity
{
    public Guid UsuarioId { get; private set; }
    public string Nome { get; private set; }
    public decimal? PrecoDefault { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private Produto() : base(Guid.NewGuid()) { Nome = string.Empty; }

    public Produto(Guid usuarioId, string nome, decimal? precoDefault)
        : base(Guid.NewGuid())
    {
        UsuarioId = usuarioId;
        Nome = nome;
        PrecoDefault = precoDefault;
        Ativo = true;
        CriadoEm = DateTime.UtcNow;
    }

    public void Atualizar(string nome, decimal? precoDefault)
    {
        Nome = nome;
        PrecoDefault = precoDefault;
        SetUpdated();
    }

    public void Desativar()
    {
        Ativo = false;
        SetUpdated();
    }
}
