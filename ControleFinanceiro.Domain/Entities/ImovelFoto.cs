using ControleFinanceiro.Domain.Common;

namespace ControleFinanceiro.Domain.Entities;

public class ImovelFoto : Entity
{
    public Guid ImovelId { get; private set; }
    public string Dados { get; private set; }
    public int Ordem { get; private set; }

    private ImovelFoto() : base(Guid.NewGuid()) { Dados = ""; }

    public Imovel Imovel { get; private set; } = null!;

    public ImovelFoto(Guid imovelId, string dados, int ordem) : base(Guid.NewGuid())
    {
        ImovelId = imovelId; Dados = dados; Ordem = ordem;
    }
}
