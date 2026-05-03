## Testes unitários

Regras obrigatórias para toda implementação nova:
- Todo novo CommandHandler ou QueryHandler DEVE ter um arquivo de testes correspondente em `Login.Application.Tests/Users/` (ou pasta equivalente no projeto)
- Cenários mínimos por handler: happy path, not found (KeyNotFoundException), e verificação de que efeitos colaterais (Remove, Update, SaveChanges) NÃO ocorrem em caso de erro
- Padrão: xUnit + Moq + FluentAssertions, seguindo os arquivos existentes em `Login.Application.Tests/Users/`
- Rodar `dotnet test` antes de concluir qualquer tarefa que adicione handlers

## graphify

This project has a graphify knowledge graph at graphify-out/.

Rules:
- Before answering architecture or codebase questions, read graphify-out/GRAPH_REPORT.md for god nodes and community structure
- If graphify-out/wiki/index.md exists, navigate it instead of reading raw files
- For cross-module "how does X relate to Y" questions, prefer `graphify query "<question>"`, `graphify path "<A>" "<B>"`, or `graphify explain "<concept>"` over grep — these traverse the graph's EXTRACTED + INFERRED edges instead of scanning files
- After modifying code files in this session, run `graphify update C:\Repositorio\ControleFinanceiro` to keep the graph current (AST-only, no API cost)
- MANDATORY: always run the update command at the end of every task that touches code files — do not skip
