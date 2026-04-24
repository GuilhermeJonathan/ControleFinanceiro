using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Application.Categorias.Queries.GetCategorias;

public record CategoriaDto(Guid Id, string Nome, TipoLancamento Tipo);
