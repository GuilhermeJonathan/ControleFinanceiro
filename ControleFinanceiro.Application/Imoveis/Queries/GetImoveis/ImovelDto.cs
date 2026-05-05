namespace ControleFinanceiro.Application.Imoveis.Queries.GetImoveis;

public record ImovelFotoDto(Guid Id, string Dados, int Ordem);

public record ImovelComentarioDto(Guid Id, string Texto, DateTime CriadoEm);

public record ImovelDto(
    Guid Id,
    string Descricao,
    decimal Valor,
    List<string> Pros,
    List<string> Contras,
    int Nota,
    DateTime DataVisita,
    string? NomeCorretor,
    string? TelefoneCorretor,
    string? Imobiliaria,
    string? Tipo,
    List<ImovelFotoDto> Fotos,
    List<ImovelComentarioDto> Comentarios
);
