using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

/// <summary>
/// Uma etapa do Plano de Ação. Texto livre definido pelo assessor (não é fixo),
/// posicionada na linha do tempo (Prazo textual) com um alvo e um status manual.
/// Owned type de <see cref="PlanoAcao"/>.
/// </summary>
public class EtapaPlano
{
    public int Ordem { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    /// <summary>Prazo textual e livre (ex.: "2027", "2026 – 2028", "jun/26").</summary>
    public string? Prazo { get; private set; }
    /// <summary>Alvo textual da etapa (ex.: "holding ativa", "20% no exterior", "R$ 30M").</summary>
    public string? Alvo { get; private set; }
    public StatusEtapa Status { get; private set; }

    private EtapaPlano() { }

    public EtapaPlano(int ordem, string titulo, string? descricao, string? prazo, string? alvo, StatusEtapa status)
    {
        Ordem = ordem;
        Titulo = titulo;
        Descricao = descricao;
        Prazo = prazo;
        Alvo = alvo;
        Status = status;
    }
}

/// <summary>
/// Plano de Ação do cliente: um objetivo e uma jornada de etapas na linha do tempo,
/// montada pelo assessor (view-as) e acompanhada pelo cliente (somente leitura).
/// Um plano por usuário efetivo. Bounded context do Patrimônio.
/// </summary>
public class PlanoAcao
{
    private readonly List<EtapaPlano> _etapas = new();

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UsuarioId { get; private set; }
    public string Objetivo { get; private set; } = string.Empty;
    /// <summary>Prazo-alvo textual do objetivo (ex.: "2028"). Opcional.</summary>
    public string? Prazo { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; private set; }

    public IReadOnlyCollection<EtapaPlano> Etapas => _etapas.AsReadOnly();

    private PlanoAcao() { }

    public PlanoAcao(Guid usuarioId, string objetivo, string? prazo, IEnumerable<EtapaPlano>? etapas = null)
    {
        UsuarioId = usuarioId;
        AplicarCampos(objetivo, prazo, etapas);
    }

    public void Atualizar(string objetivo, string? prazo, IEnumerable<EtapaPlano>? etapas = null)
    {
        AplicarCampos(objetivo, prazo, etapas);
        AtualizadoEm = DateTime.UtcNow;
    }

    private void AplicarCampos(string objetivo, string? prazo, IEnumerable<EtapaPlano>? etapas)
    {
        Objetivo = objetivo;
        Prazo = prazo;
        _etapas.Clear();
        if (etapas != null) _etapas.AddRange(etapas);
    }
}
