using ControleFinanceiro.Domain.Common;

namespace ControleFinanceiro.Domain.Entities;

public class CartaoCredito : Entity
{
    public string Nome { get; private set; }
    public int? DiaVencimento { get; private set; }  // Dia do mês (1-28), opcional
    public Guid UsuarioId { get; private set; }

    public ICollection<ParcelaCartao> Parcelas { get; private set; } = [];

    private CartaoCredito() : base(Guid.NewGuid()) { Nome = string.Empty; }

    public CartaoCredito(string nome, int? diaVencimento = null, Guid usuarioId = default) : base(Guid.NewGuid())
    {
        UsuarioId = usuarioId;
        Nome = nome;
        DiaVencimento = diaVencimento.HasValue ? Math.Clamp(diaVencimento.Value, 1, 28) : null;
    }

    public void Update(string nome, int? diaVencimento)
    {
        Nome = nome;
        DiaVencimento = diaVencimento.HasValue ? Math.Clamp(diaVencimento.Value, 1, 28) : null;
        SetUpdated();
    }
}
