using System.Globalization;
using System.Text;

namespace ControleFinanceiro.Api.WhatsApp;

/// <summary>
/// Tenta inferir o nome de uma categoria a partir da descrição do lançamento,
/// usando um dicionário de palavras-chave → categoria.
/// </summary>
public static class CategoryMatcher
{
    // Dicionário: palavra-chave (minúsculo, sem acento) → nome da categoria
    private static readonly Dictionary<string, string> _keywords = new()
    {
        // ── Alimentação ───────────────────────────────────────────────────
        { "restaurante",    "Alimentação" },
        { "lanchonete",     "Alimentação" },
        { "lanche",         "Alimentação" },
        { "almoco",         "Alimentação" },
        { "almoço",         "Alimentação" },
        { "jantar",         "Alimentação" },
        { "cafe",           "Alimentação" },
        { "café",           "Alimentação" },
        { "cafeteria",      "Alimentação" },
        { "padaria",        "Alimentação" },
        { "pizza",          "Alimentação" },
        { "hamburguer",     "Alimentação" },
        { "hamburger",      "Alimentação" },
        { "burger",         "Alimentação" },
        { "sushi",          "Alimentação" },
        { "churrasco",      "Alimentação" },
        { "marmita",        "Alimentação" },
        { "delivery",       "Alimentação" },
        { "ifood",          "Alimentação" },
        { "rappi",          "Alimentação" },
        { "mercado",        "Alimentação" },
        { "supermercado",   "Alimentação" },
        { "feira",          "Alimentação" },
        { "acougue",        "Alimentação" },
        { "açougue",        "Alimentação" },
        { "hortifruti",     "Alimentação" },
        { "sorvete",        "Alimentação" },
        { "doceria",        "Alimentação" },
        { "pastelaria",     "Alimentação" },

        // ── Transporte ────────────────────────────────────────────────────
        { "gasolina",       "Transporte" },
        { "combustivel",    "Transporte" },
        { "combustível",    "Transporte" },
        { "etanol",         "Transporte" },
        { "diesel",         "Transporte" },
        { "uber",           "Transporte" },
        { "99",             "Transporte" },
        { "taxi",           "Transporte" },
        { "táxi",           "Transporte" },
        { "onibus",         "Transporte" },
        { "ônibus",         "Transporte" },
        { "metro",          "Transporte" },
        { "metrô",          "Transporte" },
        { "estacionamento", "Transporte" },
        { "pedagio",        "Transporte" },
        { "pedágio",        "Transporte" },
        { "oficina",        "Transporte" },
        { "mecanico",       "Transporte" },
        { "mecânico",       "Transporte" },
        { "pneu",           "Transporte" },
        { "revisao",        "Transporte" },
        { "revisão",        "Transporte" },
        { "ipva",           "Transporte" },
        { "seguro carro",   "Transporte" },
        { "passagem",       "Transporte" },

        // ── Saúde ─────────────────────────────────────────────────────────
        { "farmacia",       "Saúde" },
        { "farmácia",       "Saúde" },
        { "remedio",        "Saúde" },
        { "remédio",        "Saúde" },
        { "medico",         "Saúde" },
        { "médico",         "Saúde" },
        { "consulta",       "Saúde" },
        { "hospital",       "Saúde" },
        { "dentista",       "Saúde" },
        { "exame",          "Saúde" },
        { "plano saude",    "Saúde" },
        { "plano de saude", "Saúde" },
        { "academia",       "Saúde" },
        { "clinica",        "Saúde" },
        { "clínica",        "Saúde" },
        { "psicólogo",      "Saúde" },
        { "psicologo",      "Saúde" },
        { "fisioterapia",   "Saúde" },
        { "vacina",         "Saúde" },

        // ── Casa / Moradia ────────────────────────────────────────────────
        { "aluguel",        "Moradia" },
        { "condominio",     "Moradia" },
        { "condomínio",     "Moradia" },
        { "iptu",           "Moradia" },
        { "luz",            "Moradia" },
        { "energia",        "Moradia" },
        { "agua",           "Moradia" },
        { "água",           "Moradia" },
        { "gas",            "Moradia" },
        { "gás",            "Moradia" },
        { "internet",       "Moradia" },
        { "telefone",       "Moradia" },
        { "celular",        "Moradia" },
        { "reforma",        "Moradia" },
        { "moveis",         "Moradia" },
        { "móveis",         "Moradia" },
        { "eletrodomestico","Moradia" },

        // ── Lazer / Entretenimento ────────────────────────────────────────
        { "cinema",         "Lazer" },
        { "teatro",         "Lazer" },
        { "show",           "Lazer" },
        { "ingresso",       "Lazer" },
        { "netflix",        "Lazer" },
        { "spotify",        "Lazer" },
        { "disney",         "Lazer" },
        { "youtube",        "Lazer" },
        { "prime video",    "Lazer" },
        { "clube",          "Lazer" },
        { "bar",            "Lazer" },
        { "balada",         "Lazer" },
        { "viagem",         "Lazer" },
        { "hotel",          "Lazer" },
        { "airbnb",         "Lazer" },
        { "parque",         "Lazer" },
        { "passeio",        "Lazer" },
        { "jogo",           "Lazer" },
        { "steam",          "Lazer" },
        { "playstation",    "Lazer" },
        { "xbox",           "Lazer" },

        // ── Educação ──────────────────────────────────────────────────────
        { "escola",         "Educação" },
        { "faculdade",      "Educação" },
        { "curso",          "Educação" },
        { "livro",          "Educação" },
        { "mensalidade",    "Educação" },
        { "aula",           "Educação" },
        { "treinamento",    "Educação" },
        { "udemy",          "Educação" },
        { "alura",          "Educação" },

        // ── Pets ──────────────────────────────────────────────────────────
        { "veterinario",    "Pets" },
        { "veterinário",    "Pets" },
        { "racao",          "Pets" },
        { "ração",          "Pets" },
        { "petshop",        "Pets" },
        { "pet shop",       "Pets" },

        // ── Vestuário ─────────────────────────────────────────────────────
        { "roupa",          "Vestuário" },
        { "sapato",         "Vestuário" },
        { "tenis",          "Vestuário" },
        { "tênis",          "Vestuário" },
        { "calcado",        "Vestuário" },
        { "calçado",        "Vestuário" },
        { "shopping",       "Vestuário" },
        { "zara",           "Vestuário" },
        { "renner",         "Vestuário" },
        { "hering",         "Vestuário" },

        // ── Receita / Renda ───────────────────────────────────────────────
        { "salario",        "Salário" },
        { "salário",        "Salário" },
        { "freelance",      "Salário" },
        { "freelancer",     "Salário" },
        { "comissao",       "Salário" },
        { "comissão",       "Salário" },
        { "bonus",          "Salário" },
        { "bônus",          "Salário" },
        { "dividendo",      "Investimentos" },
        { "rendimento",     "Investimentos" },
        { "investimento",   "Investimentos" },
    };

    /// <summary>
    /// Tenta inferir o nome de uma categoria a partir da descrição.
    /// Retorna null se não encontrar nenhum match.
    /// </summary>
    public static string? Infer(string descricao)
    {
        var norm = Normalize(descricao);

        // Tenta match por frases primeiro (mais específico), depois palavras
        foreach (var (kw, cat) in _keywords.OrderByDescending(k => k.Key.Length))
        {
            if (norm.Contains(Normalize(kw)))
                return cat;
        }

        return null;
    }

    private static string Normalize(string text)
    {
        // Minúsculo + remove acentos
        var normalized = text.ToLowerInvariant();
        normalized = string.Concat(
            normalized.Normalize(NormalizationForm.FormD)
                      .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
        );
        return normalized;
    }
}
