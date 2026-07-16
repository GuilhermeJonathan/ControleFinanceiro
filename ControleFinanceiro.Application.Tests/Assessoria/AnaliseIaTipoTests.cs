using ControleFinanceiro.Application.Assessoria.Queries.GetAnaliseIa;
using ControleFinanceiro.Domain.Enums;
using FluentAssertions;

namespace ControleFinanceiro.Application.Tests.Assessoria;

public class AnaliseIaTipoTests
{
    [Fact]
    public void SugerirTipo_situacao_saudavel_retorna_Dica()
    {
        // score alto, saldo positivo, renda pouco comprometida
        var tipo = GetAnaliseIaQueryHandler.SugerirTipo(scoreGeral: 80, receitas: 10000m, despesas: 6000m, saldo: 4000m);
        tipo.Should().Be(TipoRecomendacao.Dica);
    }

    [Fact]
    public void SugerirTipo_renda_muito_comprometida_retorna_Alerta()
    {
        // 92% da renda comprometida (caso do painel) => Alerta mesmo com score razoável
        var tipo = GetAnaliseIaQueryHandler.SugerirTipo(scoreGeral: 74, receitas: 10000m, despesas: 9200m, saldo: 800m);
        tipo.Should().Be(TipoRecomendacao.Alerta);
    }

    [Fact]
    public void SugerirTipo_comprometimento_no_limiar_de_80pct_retorna_Alerta()
    {
        var tipo = GetAnaliseIaQueryHandler.SugerirTipo(scoreGeral: 74, receitas: 10000m, despesas: 8000m, saldo: 2000m);
        tipo.Should().Be(TipoRecomendacao.Alerta);
    }

    [Fact]
    public void SugerirTipo_comprometimento_logo_abaixo_de_80pct_retorna_Dica()
    {
        var tipo = GetAnaliseIaQueryHandler.SugerirTipo(scoreGeral: 74, receitas: 10000m, despesas: 7900m, saldo: 2100m);
        tipo.Should().Be(TipoRecomendacao.Dica);
    }

    [Fact]
    public void SugerirTipo_saldo_negativo_retorna_Alerta()
    {
        var tipo = GetAnaliseIaQueryHandler.SugerirTipo(scoreGeral: 60, receitas: 10000m, despesas: 11000m, saldo: -1000m);
        tipo.Should().Be(TipoRecomendacao.Alerta);
    }

    [Fact]
    public void SugerirTipo_score_baixo_retorna_Alerta()
    {
        var tipo = GetAnaliseIaQueryHandler.SugerirTipo(scoreGeral: 40, receitas: 10000m, despesas: 5000m, saldo: 5000m);
        tipo.Should().Be(TipoRecomendacao.Alerta);
    }

    [Fact]
    public void SugerirTipo_sem_receitas_nao_quebra_e_retorna_Dica()
    {
        var tipo = GetAnaliseIaQueryHandler.SugerirTipo(scoreGeral: 70, receitas: 0m, despesas: 0m, saldo: 0m);
        tipo.Should().Be(TipoRecomendacao.Dica);
    }
}
