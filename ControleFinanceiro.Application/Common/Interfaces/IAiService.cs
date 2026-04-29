namespace ControleFinanceiro.Application.Common.Interfaces;

/// <summary>
/// Abstração para chat completions via LLM (Azure OpenAI ou qualquer outro provedor).
/// </summary>
public interface IAiService
{
    /// <summary>
    /// Executa um chat completion com system prompt + mensagem do usuário e retorna o texto gerado.
    /// </summary>
    /// <param name="systemPrompt">Contexto/persona do assistente.</param>
    /// <param name="userMessage">Mensagem ou dados enviados pelo usuário.</param>
    /// <param name="maxTokens">Limite de tokens na resposta (default: 800).</param>
    /// <param name="temperature">Criatividade do modelo — 0 = determinístico, 1 = criativo (default: 0.3).</param>
    /// <param name="cancellationToken"></param>
    Task<string> ChatAsync(
        string systemPrompt,
        string userMessage,
        int   maxTokens    = 800,
        float temperature  = 0.3f,
        CancellationToken cancellationToken = default);
}
