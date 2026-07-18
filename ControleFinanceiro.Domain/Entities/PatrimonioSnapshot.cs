namespace ControleFinanceiro.Domain.Entities;

/// <summary>
/// Foto mensal do patrimônio de um usuário — usada para o gráfico de evolução.
/// Um registro por usuário por mês (atualizado durante o mês corrente).
/// </summary>
public class PatrimonioSnapshot
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UsuarioId { get; private set; }
    public int Ano { get; private set; }
    public int Mes { get; private set; }
    public decimal PatrimonioLiquidoBRL { get; private set; }
    public decimal TotalBensBRL { get; private set; }
    public decimal TotalDividasBRL { get; private set; }
    public DateTime AtualizadoEm { get; private set; } = DateTime.UtcNow;

    private PatrimonioSnapshot() { }

    public static PatrimonioSnapshot Criar(Guid usuarioId, int ano, int mes,
        decimal patrimonioLiquidoBRL, decimal totalBensBRL, decimal totalDividasBRL) =>
        new()
        {
            UsuarioId = usuarioId,
            Ano = ano,
            Mes = mes,
            PatrimonioLiquidoBRL = patrimonioLiquidoBRL,
            TotalBensBRL = totalBensBRL,
            TotalDividasBRL = totalDividasBRL,
        };

    public void Atualizar(decimal patrimonioLiquidoBRL, decimal totalBensBRL, decimal totalDividasBRL)
    {
        PatrimonioLiquidoBRL = patrimonioLiquidoBRL;
        TotalBensBRL = totalBensBRL;
        TotalDividasBRL = totalDividasBRL;
        AtualizadoEm = DateTime.UtcNow;
    }
}
