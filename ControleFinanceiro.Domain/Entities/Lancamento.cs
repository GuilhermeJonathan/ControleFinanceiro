using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

public class Lancamento : Entity
{
    public string Descricao { get; private set; }
    public DateTime Data { get; private set; }
    public decimal Valor { get; private set; }
    public TipoLancamento Tipo { get; private set; }
    public SituacaoLancamento Situacao { get; private set; }
    public int Mes { get; private set; }
    public int Ano { get; private set; }
    public Guid? CategoriaId { get; private set; }
    public Guid? CartaoId { get; private set; }
    public int? ParcelaAtual { get; private set; }
    public int? TotalParcelas { get; private set; }
    public Guid? GrupoParcelas { get; private set; }
    public Guid? ReceitaRecorrenteId { get; private set; }
    public bool IsRecorrente { get; private set; }
    public Guid? ContaBancariaId { get; private set; }
    public DateTime? DataPagamento { get; private set; }
    public Guid? TransferenciaId { get; private set; }
    public Guid UsuarioId { get; private set; }
    /// <summary>ID de quem realmente criou o lançamento (pode ser membro da família)</summary>
    public Guid? CriadoPorId { get; private set; }
    /// <summary>Nome/email de quem criou — desnormalizado para evitar lookup no Login API</summary>
    public string? CriadoPorNome { get; private set; }

    public Categoria? Categoria { get; private set; }
    public CartaoCredito? Cartao { get; private set; }
    public ReceitaRecorrente? ReceitaRecorrente { get; private set; }
    public SaldoConta? ContaBancaria { get; private set; }

    private Lancamento() : base(Guid.NewGuid()) { Descricao = string.Empty; }

    public Lancamento(string descricao, DateTime data, decimal valor, TipoLancamento tipo,
        SituacaoLancamento situacao, int mes, int ano, Guid? categoriaId = null,
        Guid? cartaoId = null, int? parcelaAtual = null, int? totalParcelas = null,
        Guid? grupoParcelas = null, Guid? receitaRecorrenteId = null,
        bool isRecorrente = false, Guid usuarioId = default,
        Guid? criadoPorId = null, string? criadoPorNome = null,
        Guid? transferenciaId = null)
        : base(Guid.NewGuid())
    {
        UsuarioId = usuarioId;
        CriadoPorId = criadoPorId;
        CriadoPorNome = criadoPorNome;
        Descricao = descricao;
        Data = data;
        Valor = valor;
        Tipo = tipo;
        Situacao = situacao;
        Mes = mes;
        Ano = ano;
        CategoriaId = categoriaId;
        CartaoId = cartaoId;
        ParcelaAtual = parcelaAtual;
        TotalParcelas = totalParcelas;
        GrupoParcelas = grupoParcelas;
        ReceitaRecorrenteId = receitaRecorrenteId;
        IsRecorrente = isRecorrente;
        TransferenciaId = transferenciaId;
    }

    public void Update(string descricao, DateTime data, decimal valor, TipoLancamento tipo,
        SituacaoLancamento situacao, Guid? categoriaId, Guid? cartaoId = null)
    {
        Descricao = descricao;
        Data = data;
        Valor = valor;
        Tipo = tipo;
        Situacao = situacao;
        CategoriaId = categoriaId;
        CartaoId = cartaoId;
        SetUpdated();
    }

    /// <summary>
    /// Atualiza apenas os campos "template" de um recorrente futuro:
    /// descrição, valor, tipo, categoria e cartão — preserva mês/ano/situação próprios.
    /// </summary>
    public void UpdateRecorrente(string descricao, decimal valor, TipoLancamento tipo,
        Guid? categoriaId, Guid? cartaoId, int dia)
    {
        Descricao   = descricao;
        Valor       = valor;
        Tipo        = tipo;
        CategoriaId = categoriaId;
        CartaoId    = cartaoId;
        var diaMax  = DateTime.DaysInMonth(Data.Year, Data.Month);
        Data        = new DateTime(Data.Year, Data.Month, Math.Min(dia, diaMax));
        SetUpdated();
    }

    public void AtualizarSituacao(SituacaoLancamento situacao)
    {
        Situacao = situacao;
        SetUpdated();
    }

    public void SetContaBancaria(Guid? contaBancariaId)
    {
        ContaBancariaId = contaBancariaId;
        SetUpdated();
    }

    public void SetTransferenciaId(Guid id)
    {
        TransferenciaId = id;
        SetUpdated();
    }

    public void SetDataPagamento(DateTime? data)
    {
        DataPagamento = data;
        SetUpdated();
    }

    public void AtualizarDeReceita(string descricao, decimal valor, int dia)
    {
        Descricao = descricao;
        Valor = valor;
        var novoDia = Math.Min(dia, DateTime.DaysInMonth(Data.Year, Data.Month));
        Data = new DateTime(Data.Year, Data.Month, novoDia);
        SetUpdated();
    }
}
