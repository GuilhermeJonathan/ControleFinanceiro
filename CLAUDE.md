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
- Do NOT run `graphify update` automatically. Only refresh the graph when the user explicitly asks for it.

---

## Visão Geral da Plataforma

**Plataforma Patrimônio** — gestão patrimonial B2B para alta renda.

### Projetos na solução

| Projeto | Descrição |
|---|---|
| `ControleFinanceiro.Api` | ASP.NET Core 10, controllers REST, middleware, background services |
| `ControleFinanceiro.Application` | MediatR — commands/queries/handlers, interfaces |
| `ControleFinanceiro.Domain` | Entidades, enums, interfaces de repositório |
| `ControleFinanceiro.Infrastructure` | EF Core + PostgreSQL, repositórios, serviços externos |
| `ControleFinanceiro.Application.Tests` | xUnit + Moq + FluentAssertions |
| `Login/Login.*` | API de autenticação separada (JWT, planos, usuários) |
| `mobile-patrimonio/mobile-patrimonio` | React Native + Expo Web (frontend ativo) |
| `mobile-patrimonio/src` | Frontend legado (não usar) |

### Stack

- **Backend**: .NET 10 / C# 14, MediatR, EF Core 9, PostgreSQL (Npgsql), Azure OpenAI
- **Frontend mobile/web**: React Native + Expo + TypeScript, deploy Vercel (`patrimonio-roan.vercel.app`)
- **Auth**: JWT via Login API separada (`Login/Login.*`)
- **Monitoramento**: Sentry, rate limiting 60 req/min por IP

---

## Arquitetura

### Padrão Clean Architecture + CQRS
```
Controller → MediatR → Handler (Command/Query) → Repository → DB
```

### Middleware Pipeline (ordem em Program.cs)
1. `ExceptionHandlingMiddleware`
2. CORS
3. HTTPS redirect (dev only)
4. Rate limiter
5. Authentication (JWT)
6. `FamiliaContextMiddleware` — resolve contexto de família (VinculoFamiliar)
7. `AssessoriaContextMiddleware` — resolve header `X-Assessoria-Cliente` → seta `ICurrentUser.ClienteId` para view-as
8. Authorization
9. Controllers

### ICurrentUser
```csharp
interface ICurrentUser {
  Guid UserId;          // usuário do JWT
  Guid? ClienteId;      // preenchido pelo AssessoriaContextMiddleware (view-as)
}
```
Quando `ClienteId != null`, handlers operam sobre o patrimônio do cliente visualizado.

---

## Módulos do Backend

### Patrimônio (`/api/patrimonio`)
| Endpoint | Descrição |
|---|---|
| `GET /resumo` | Balanço consolidado (bens, dívidas, patrimônio líquido, ROI, fluxo) |
| `GET /dicas` | Análise IA + fallback estático do patrimônio |
| `GET /evolucao?meses=12` | Histórico de snapshots patrimoniais |
| `GET /insights` | Insights automáticos |
| `GET /rebalanceamento` | Sugestão de rebalanceamento por classe |
| `PUT /alocacao-alvo` | Salva percentuais alvo por classe |
| `GET /projecao-dividas` | Projeção de quitação de dívidas |
| `GET /projecao-patrimonio` | Projeção patrimonial com cenários |
| `POST /relatorio` | Gera PDF patrimonial (com marca do assessor) |
| `POST/PUT/DELETE /ativos/{id}` | CRUD de ativos patrimoniais |
| `POST/PUT/DELETE /passivos/{id}` | CRUD de dívidas/passivos |
| `GET/POST/PUT/DELETE /plano-acao` | Planos de ação (etapas de jornada) |

**Entidades principais**: `AtivoPatrimonial`, `PassivoPatrimonial`, `PatrimonioSnapshot`  
**Enums**: `TipoAtivo` (Imovel, Veiculo, Embarcacao, Aeronave, Participacao, Investimento, Outro), `MoedaPatrimonio` (BRL/USD/EUR/CHF/GBP), `PrazoDivida`

### Investimentos (`/api/investimentos`)
| Endpoint | Descrição |
|---|---|
| `GET /resumo` | Portfólio consolidado em BRL, rentabilidade, alocação |
| `POST/PUT/DELETE /` | CRUD de investimentos |

**Entidade**: `Investimento` (nome, tipo, moeda, corretora, ticker, valorAplicado, valorAtual, rentabilidadeAnualPct)

### Assessoria (`/api/assessoria`)
| Endpoint | Descrição |
|---|---|
| `POST /convite` | Gera código de convite |
| `POST /convite/email` | Gera convite e envia e-mail ao cliente |
| `POST /convite/{id}/reenviar` | Reenvio de convite |
| `GET /convite/validar/{codigo}` | Valida código de convite |
| `POST /aceitar-publico` | Aceite público (cria conta + vincula) |
| `GET /clientes` | Carteira de clientes do assessor |
| `DELETE /{vinculoId}` | Revoga vínculo |
| `GET /meu-assessor` | Consultor do cliente logado |
| `GET /saude/{mes}/{ano}` | Score de saúde financeira (via view-as) |
| `GET /parametros-saude` | Parâmetros do termômetro (por assessor) |
| `PUT /parametros-saude` | Salva parâmetros |
| `POST /recomendacoes` | Assessor cria recomendação para cliente |
| `GET /recomendacoes/cliente/{clienteId}` | Assessor lista recomendações de um cliente |
| `GET /recomendacoes` | Cliente lista suas recomendações recebidas |
| `DELETE /recomendacoes/{id}` | Assessor exclui recomendação pendente |
| `PATCH /recomendacoes/{id}/responder` | Cliente aceita/recusa recomendação |
| `GET /recomendacoes/respostas` | Assessor vê respostas dos clientes (sino) |
| `POST /recomendacoes/respostas/marcar-vistas` | Marca respostas como vistas |
| `GET /analise-ia/{mes}/{ano}` | Rascunho de recomendação gerado por IA |

**Entidade**: `RecomendacaoAssessoria` (clienteId, assessorId, tipo[1=Ajuste,2=Dica,3=Alerta], texto, status[1=Pendente,2=Aceita,3=Recusada], respostaCliente)

### Corretores (`/api/corretores`)
| Endpoint | Descrição |
|---|---|
| `POST /convite` | Assessor gera convite para corretor |
| `GET /convite/validar/{codigo}` | Valida código |
| `POST /aceitar-publico` | Corretor aceita convite (cria conta) |
| `GET /` | Lista corretores do assessor |
| `DELETE /{vinculoId}` | Revoga vínculo |
| `POST /delegacoes` | Delega gestão de clientes ao corretor |
| `DELETE /delegacoes/{id}` | Revoga delegação |

**Entidades**: `VinculoCorretor`, `DelegacaoCarteira`

### Parâmetros (`/api/parametros`)
CRUD de tipos de ativo, tipos de investimento e moedas (por assessor).  
Cada tabela tem registros `IsSystem=true` (seed) que não podem ser excluídos.

### Gestão Pessoal (FinDog integrado) — `/api/`
Lançamentos, categorias, cartões, faturas, saldos, metas, orçamento, receitas recorrentes, dívidas, assinaturas, extrato OFX, transferências, projeção, dicas IA de lançamentos.

### Consultoria (`/api/consultoria`)
Configuração de marca do assessor (nome, logo base64, cor, WhatsApp, rodapé para relatórios PDF).

### Simulações (`/api/simulacoes`)
Simulações de independência financeira (monte carlo simplificado, cenários de aporte/resgate).

---

## Tipos de Usuário (JWT `userType` claim)

| Valor | Tipo | Acesso |
|---|---|---|
| `1` | Admin | Tudo |
| `3` | Assessor | Carteira de clientes, Corretores, Parâmetros, Consultoria |
| `4` | Corretor | Acesso delegado (DelegacaoCarteira) |
| outros | Cliente | Patrimônio próprio + receber recomendações |

`isAssessor = userType === '3' || userType === '1'`  
`isCorretor = userType === '4'`

---

## Frontend (mobile-patrimonio/mobile-patrimonio)

### Rotas principais (`router.tsx`)
```
inicio           → HomeScreen (dashboard dual: assessor ou cliente)
patrimonio       → AtivosScreen
dividas          → (passivos)
investimentos    → InvestimentosScreen
gp-metas         → MetasScreen
gp-dashboard     → DashboardGPScreen
clientes         → AssessorClientesScreen
corretores       → CorretoresScreen
parametros       → ParamCrudScreen
```

### Autenticação / AppShell
- `App.tsx` → lê `isAssessor`, `isCorretor` do JWT
- `AppShell.tsx` → menu lateral filtrado por role
- `AssessoriaContext` → controla o modo view-as (assessor visualizando cliente)

### HomeScreen (dupla visão)
- **isAssessor=true**: painel do book (AUM, composição, top clientes, convites pendentes)
- **cliente**: dashboard patrimonial + banner de recomendações pendentes do assessor

### AssessorClientesScreen
- Cards com score badge (Saudável 🟢 / Atenção 🟡 / Crítica 🔴)
- Filtros: Todos / Em atenção / Saudáveis
- Botões: Painel (view-as) / Recomendar / Histórico (PDF)
- **Recomendar**: Modal full-screen com nova recomendação + histórico respondido

### HomeCorretorScreen
- Visão simplificada dos clientes delegados ao corretor logado

### AtivosScreen
- Filtros por tipo de ativo e moeda
- **✨ Dicas IA**: painel toggle com análise patrimonial (alavancagem, ROI, fluxo) + dica educativa

### InvestimentosScreen
- Agrupamento por banco ou classe
- Filtros por classe de ativo
- Alocação % por tipo e custodiante

---

## Serviços de IA

`IAiService.ChatAsync(systemPrompt, userMessage, maxTokens, temperature, ct)`  
Implementado em `AzureOpenAiService` (Azure OpenAI).

Handlers que usam IA (todos com fallback estático):
- `GetDicasQueryHandler` — dicas de lançamentos mensais
- `GetDicasPatrimonioQueryHandler` — análise do patrimônio
- `GetAnaliseDividasQueryHandler` — análise de dívidas
- `AnaliseIaAssessoriaQueryHandler` — rascunho de recomendação para o assessor

---

## Padrões de Código

### Backend
- Handlers injetam repositório + `ICurrentUser` via constructor primary
- `ICurrentUser.UserId` = dono do recurso (ou assessor); `ClienteId` = view-as
- Migrations em `ControleFinanceiro.Infrastructure/Migrations/` — nomeação `YYYYMMDDHHMMSS_Descricao`
- Sem `Alert.alert` no mobile — usar modais próprios com `useState`

### Frontend
- `makeStyles(colors)` — estilos computados dentro do componente
- Serviços em `api.ts` — todos retornam `Promise<T>`
- Header `X-Assessoria-Cliente` → enviado automaticamente via interceptor quando `_assessoriaClienteId != null`
- Modais importantes devem ser telas cheias (`animationType="slide"`, sem `transparent`)

### Variáveis de ambiente (mobile)
```
EXPO_PUBLIC_API_URL   = URL da ControleFinanceiro.Api
EXPO_PUBLIC_LOGIN_URL = URL da Login API
```

---

## Migrations relevantes
| Migration | Conteúdo |
|---|---|
| `20260715000000_AddParametros` | TiposAtivo, TiposInvestimento, Moedas por assessor |
| `20260716000000_AddCorretoreDelegacao` | VinculoCorretor, DelegacaoCarteira |
| `20260717000000_AddCorretoreDelegacao` | (revisão/ajuste da anterior) |
| `20260428200000_AddWhatsAppVinculo` | WhatsAppVinculo |

---

## URLs de Produção
- **API**: configurada em `EXPO_PUBLIC_API_URL`
- **Frontend**: `https://patrimonio-roan.vercel.app`
- **CORS liberado para**: `app.findog.com.br`, `financeiro-web-two.vercel.app`, `patrimonio-roan.vercel.app`, `patrimonio.vercel.app`
