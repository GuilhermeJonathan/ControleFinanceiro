namespace ControleFinanceiro.Application.Lancamentos.Queries.GetDicas;

public record DicaFinanceiraDto(
    string  Tipo,            // "critico" | "atencao" | "positivo"
    string  Titulo,
    string  Descricao,
    string? DicaEducativa,   // dica de educação financeira relacionada ao contexto
    string? AcaoLabel,       // ex: "Ver Orçamento"
    string? AcaoRota         // nome da rota no mobile, ex: "Orcamento"
);
