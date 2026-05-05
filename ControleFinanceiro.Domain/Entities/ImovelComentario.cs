using ControleFinanceiro.Domain.Common;

namespace ControleFinanceiro.Domain.Entities;

public class ImovelComentario : Entity
{
    public Guid ImovelId { get; private set; }
    public string Texto { get; private set; }
    public DateTime CriadoEm { get; private set; }

    public Imovel Imovel { get; private set; } = null!;

    private ImovelComentario() : base(Guid.NewGuid()) { Texto = ""; }

    public ImovelComentario(Guid imovelId, string texto) : base(Guid.NewGuid())
    {
        ImovelId = imovelId;
        Texto = texto;
        CriadoEm = DateTime.UtcNow;
    }
}
