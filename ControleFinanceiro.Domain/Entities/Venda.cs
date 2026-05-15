using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

public class Venda : Entity
{
    public Guid UsuarioId { get; private set; }
    public Guid? ProdutoId { get; private set; }
    public string Descricao { get; private set; }
    public decimal Valor { get; private set; }
    public DateTime Data { get; private set; }
    public StatusVenda Status { get; private set; }
    public OrigemVenda Origem { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public string CriadoPorNome { get; private set; }

    private Venda() : base(Guid.NewGuid()) { Descricao = string.Empty; CriadoPorNome = string.Empty; }

    public Venda(Guid usuarioId, Guid? produtoId, string descricao, decimal valor,
        DateTime data, OrigemVenda origem, string criadoPorNome)
        : base(Guid.NewGuid())
    {
        UsuarioId = usuarioId;
        ProdutoId = produtoId;
        Descricao = descricao;
        Valor = valor;
        Data = data;
        Status = StatusVenda.Pendente;
        Origem = origem;
        CriadoEm = DateTime.UtcNow;
        CriadoPorNome = criadoPorNome;
    }

    public void Atualizar(string descricao, decimal valor, DateTime data, Guid? produtoId)
    {
        Descricao = descricao;
        Valor = valor;
        Data = data;
        ProdutoId = produtoId;
        SetUpdated();
    }

    public void AtualizarStatus(StatusVenda status)
    {
        Status = status;
        SetUpdated();
    }
}
