using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

public class Meta : Entity
{
    public Guid UsuarioId { get; private set; }
    public string Titulo { get; private set; }
    public string? Descricao { get; private set; }
    public decimal ValorMeta { get; private set; }
    public decimal ValorAtual { get; private set; }
    public DateTime? DataMeta { get; private set; }
    public StatusMeta Status { get; private set; }
    /// <summary>Emoji ou letra usada como capa do card (ex: "🏠", "🏍️", "💰")</summary>
    public string? Capa { get; private set; }
    /// <summary>Cor hex do card (ex: "#1a3a2a")</summary>
    public string? CorFundo { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private Meta() : base(Guid.NewGuid()) { Titulo = string.Empty; }

    public Meta(Guid usuarioId, string titulo, string? descricao, decimal valorMeta,
        DateTime? dataMeta, string? capa, string? corFundo)
        : base(Guid.NewGuid())
    {
        UsuarioId = usuarioId;
        Titulo    = titulo;
        Descricao = descricao;
        ValorMeta = valorMeta;
        ValorAtual = 0;
        DataMeta  = dataMeta;
        Status    = StatusMeta.Ativa;
        Capa      = capa;
        CorFundo  = corFundo;
        CriadoEm = DateTime.UtcNow;
    }

    public void Atualizar(string titulo, string? descricao, decimal valorMeta,
        DateTime? dataMeta, string? capa, string? corFundo)
    {
        Titulo    = titulo;
        Descricao = descricao;
        ValorMeta = valorMeta;
        DataMeta  = dataMeta;
        Capa      = capa;
        CorFundo  = corFundo;
        SetUpdated();
    }

    public void AtualizarValor(decimal novoValor)
    {
        ValorAtual = Math.Max(0, novoValor);
        if (ValorAtual >= ValorMeta) Status = StatusMeta.Concluida;
        else if (Status == StatusMeta.Concluida) Status = StatusMeta.Ativa;
        SetUpdated();
    }

    public void AlterarStatus(StatusMeta status)
    {
        Status = status;
        SetUpdated();
    }
}
