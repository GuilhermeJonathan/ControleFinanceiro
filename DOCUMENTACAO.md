# Documentação do Sistema — Patrimônio / Meu FinDog

> Plataforma de finanças pessoais (Meu FinDog) + gestão patrimonial B2B2C (Patrimônio),
> onde **assessores** e **corretores** acompanham a carteira de **clientes** de alta renda,
> mantendo a identidade (marca) de cada consultoria.

---

## 1. Visão geral

| Item | Descrição |
|---|---|
| **Produto** | SaaS de finanças. Duas frentes: **Gestão Pessoal** (uso do cliente) e **Patrimônio/Assessoria** (assessor gerencia a carteira). |
| **Apps** | `mobile` (FinDog, cliente) e `mobile-patrimonio` (app do módulo Patrimônio) — Expo/React Native, web-primary (Vercel). |
| **Backend** | **2 APIs .NET 10** (Clean Architecture + CQRS/MediatR): `Login` (contas/planos) e `ControleFinanceiro.Api` (finanças/patrimônio). |
| **Banco** | PostgreSQL (compartilhado entre as APIs). EF Core, migrations automáticas no startup. |
| **Deploy** | Backend no Render, frontend no Vercel. |
| **Câmbio** | Consolidação multimoeda (BRL/USD/EUR/CHF/GBP) via cotação editável pelo assessor (`MoedaParam.CotacaoBRL`). |

### Perfis de usuário (UserType)
- **Admin (1)** — acesso total.
- **User/Cliente (2)** — pessoa física; usa Gestão Pessoal e vê seu Patrimônio.
- **Assessor (3)** — gerencia carteira de clientes, corretores, recomendações e branding.
- **Corretor (4)** — atende clientes delegados por um assessor.

---

## 2. Autenticação e planos

- **Login/JWT** compartilhado entre as duas APIs (mesma SecretKey). O JWT carrega `userType`, `planType`, nome/email.
- **Cadastro**: auto-cadastro público (`/user/selfregister`, trial de 30 dias) ou via **convite** (ver §7).
- **Planos**: trial → pago (MercadoPago na API Login). Avisos de expiração (D-7/D-1) e reengajamento por e-mail (jobs na Login).
- **Provisionamento server-to-server**: `POST /user/provision` (protegido por *service key*) cria/autentica conta a partir de um convite validado — usado no aceite público (§7).

---

## 3. Módulo Patrimônio (alta renda)

Balanço patrimonial consolidado do cliente. Sob o header `X-Assessoria-Cliente`, o assessor/corretor vê e edita os dados do cliente (modo *view-as*, §8).

### 3.1 Ativos (bens)
- Cadastro de bens: **Imóvel, Veículo, Embarcação, Aeronave, Participação, Investimento, Outro**.
- Campos: nome, tipo, moeda, valor atual, valorização anual %, receita e despesa mensal.
- **Máscara de valor** BR (`R$ 1.500.000,00`) nos inputs; ROI anual e fluxo de caixa calculados.
- Botão **"+ Novo"** sempre visível (inclusive sem ativos) e **"✨ Dicas IA"**.

### 3.2 Dívidas (passivos)
- Cadastro de dívidas: nome, moeda, saldo devedor, prazo (curto/longo), juros % a.a., prazo em meses.

### 3.3 Resumo consolidado (`GET /patrimonio/resumo`)
- Bens − Dívidas = **Patrimônio Líquido** (em BRL), alavancagem %, composição por categoria (% e ROI), fluxo mensal, totais por moeda.
- **Captura preguiçosa**: toda consulta grava/atualiza o *snapshot* do mês (§3.6).

### 3.4 Projeção de dívidas (`GET /patrimonio/projecao-dividas`)
- Estimativa do saldo devedor ao longo do tempo (gráfico).

### 3.5 Simulação / proteção patrimonial
- Simulações de independência financeira (idade atual/alvo, aporte, retirada, taxa real, cenários de aporte/resgate). Gráfico de projeção; simulação favorita entra no relatório.

### 3.6 Evolução do patrimônio (`GET /patrimonio/evolucao`)
- **Snapshot mensal** (`PatrimonioSnapshots`) do patrimônio líquido/bens/dívidas por usuário.
- Alimentado por: (a) **captura preguiçosa** ao abrir o Patrimônio; (b) **job diário** que garante o ponto do mês mesmo sem acesso.
- Gráfico de linha "Evolução do patrimônio" (responsivo) na tela Patrimônio.

### 3.7 Alocação-alvo e rebalanceamento (`GET /patrimonio/rebalanceamento`, `PUT /patrimonio/alocacao-alvo`)
- Define **% alvo por classe de investimento**; compara com a alocação atual e mostra o **desvio** (verde = dentro de ±3%, laranja = acima, azul = abaixo).
- Card "Alocação vs. alvo" na tela Investimentos, com editor de metas.

### 3.8 Insights (`GET /patrimonio/insights`)
- Alertas **determinísticos e acionáveis** sobre o patrimônio: concentração, alavancagem, fluxo negativo, investimentos no negativo, concentração de classe, desvio da alocação-alvo.
- No *view-as*, cada insight vira **recomendação com 1 clique** (§8.3). Complementa o "Dicas IA".

### 3.9 Relatório patrimonial (PDF)
- Geração sob demanda (QuestPDF) com **marca da consultoria**: resumo, projeção, investimentos e simulação em destaque. Disponível no card do cliente (assessor).

---

## 4. Investimentos

- Carteira de investimentos por **classe** (Ações, FII, ETF, Renda Fixa, Multimercado, Cripto, Exterior, Outro), corretora, ticker, valor aplicado/atual, rentabilidade.
- Consolidação em BRL, rentabilidade, agrupamento por corretora/classe (donuts).
- **Importar CSV** (`POST /patrimonio/investimentos/importar`): cabeçalho flexível (`nome, tipo, corretora, ticker, valorAplicado, valorAtual, moeda, rentabilidade`), separador `;` ou `,`, números BR/US. Linhas inválidas viram erros reportados; as válidas importam.

---

## 5. Gestão Pessoal (cliente)

- **Lançamentos** (receitas/despesas/pix), com situação (pago/pendente/vencido), recorrência e parcelamento. Máscara de valor BR e **data dd/mm/aaaa**.
- **Categorias** (com limite mensal), **Cartões**, **Parcelados**, **Assinaturas**.
- **Metas** (valor alvo, prazo com máscara dd/mm/aaaa, contribuição mensal automática, ícone/cor).
- **Dashboard GP**: receitas/despesas/saldo do mês, comprometimento de renda, dias de reserva, despesas por categoria.
- **Saúde financeira** (`/assessoria/saude`): score 0–100 por 4 pilares (comprometimento, orçamento, reserva, tendência). Clientes **sem dados** retornam classificação **"Sem dados"** (não entram como "Em atenção").
- **Jobs diários**: auto-vencimento (marca lançamentos vencidos) e geração de recorrentes (mantém 24 meses à frente).

> No *view-as*, a Gestão Pessoal é **somente leitura** para assessor/corretor (o middleware bloqueia escrita em `/lancamentos`, `/categorias`, `/metas`).

---

## 6. Onboarding do cliente novo

- Home do cliente sem nenhum dado exibe um card **"Vamos começar! 🚀"** com passos tocáveis: cadastrar 1º ativo, registrar lançamento, definir meta, adicionar investimento.

---

## 7. Convites por e-mail (cliente e corretor)

Fluxo para trazer clientes/corretores com **cadastro rápido**:

1. Assessor informa o **e-mail** → sistema **valida se já existe conta** (bloqueia com aviso se existir) e gera um convite.
2. Envia e-mail branded com link **`/aceitar?codigo=XXXX&tipo=cliente|corretor`** (URL do config).
3. Convidado abre a página pública `/aceitar`, valida o código e faz **cadastro em 1 passo** (nome + senha; e-mail pré-preenchido). A API de Patrimônio orquestra: cria a conta na Login (via *service key*) e vincula.
   - Se o e-mail já existir, autentica com a senha e vincula.
   - Corretor é criado já como **UserType.Corretor**.
4. **Expiração**: convites valem **7 dias**; o card mostra tipo (por e-mail/código), e-mail, código e validade, com **Reenviar** (renova) e **Cancelar**.
5. Filtros na tela: **Ativos / Pendentes / Encerrados** (expirados/cancelados).

---

## 8. Assessoria

### 8.1 Carteira de clientes
- Lista com **busca por nome ou e-mail**, filtros de saúde (**Todos / Em atenção / Saudáveis / Novos**) e badge de score.
- Ações por cliente: **Painel** (view-as), **Recomendar**, **Histórico/Relatório**.

### 8.2 View-as (visualizar/editar como cliente)
- Via header `X-Assessoria-Cliente`. Banner roxo "Editando como {cliente}".
- Assessor e corretor **podem editar** Patrimônio, Ativos, Dívidas, Investimentos, Projeção. **Gestão Pessoal é somente leitura.**
- No menu lateral, o grupo **Gestão Pessoal** aparece no view-as (dados do cliente).

### 8.3 Recomendações
- Assessor envia recomendação ao cliente: tipo **Ajuste de orçamento / Dica / Alerta**, texto.
- **Rascunho com IA** (`GET /assessoria/analise-ia`): gera análise do mês; **sugere o tipo** automaticamente (Alerta se score < 50, saldo negativo ou despesas ≥ 80% da renda; senão Dica).
- Cliente recebe **e-mail branded** + banner na home e **responde** (aceita/recusa + comentário).
- Insights do patrimônio viram recomendação com 1 clique (§3.8).

### 8.4 Notificações (sino na topbar)
- **Cliente**: sino com recomendações pendentes (🚨 vermelho se há alerta); dropdown lista e leva à recomendação.
- **Assessor**: sino com **respostas dos clientes** (aceitou/recusou); badge de não lidas, zera ao abrir.

### 8.5 Dashboard do assessor (home)
- Patrimônio líquido sob gestão, **clientes ativos**, **em atenção**, **respostas novas**, ativos na carteira, convites pendentes, top clientes e composição da carteira.

---

## 9. Corretores

- Assessor convida corretores (código ou e-mail, §7), com filtros por status e reenviar/cancelar.
- **Delegação de carteira**: assessor delega clientes ao corretor (**seleção múltipla**). Corretor acessa os clientes delegados em view-as.

---

## 10. Consultoria (branding)

- Cada assessor configura sua **consultoria**: nome, **logo**, cor da marca, WhatsApp, mensagem de rodapé (`ConsultoriaConfig`).
- Usada no **relatório PDF**, no card "Seu consultor" do cliente e em **todos os e-mails** (remetente white-label + logo servida por URL pública `GET /api/consultoria/{assessorId}/logo`).

---

## 11. E-mails automáticos

Envio **centralizado na API Login** (Resend). A API de Patrimônio delega via `POST /internal/email` (service key) — um único remetente/domínio.

| E-mail | Origem | Conteúdo |
|---|---|---|
| Convite (cliente/corretor) | Patrimônio | Marca + link `/aceitar` + código |
| Nova recomendação | Patrimônio | Marca + tipo + texto + "Responder" |
| **Resumo mensal** | Job diário (Patrimônio) | Patrimônio líquido do mês + variação vs. mês anterior; 1x/mês por cliente |
| Trial D-7/D-1, Reengajamento | Jobs (Login) | Ciclo de plano |
| Recuperação de senha, plano | Login | Transacionais |

> Todos os e-mails de negócio usam a **marca da consultoria** (nome + logo + cor).

---

## 12. Jobs em background (DailyJobService — API Patrimônio)

Rodam na subida e diariamente à meia-noite:
1. **Auto-vencimento** — lançamentos "a vencer" com data passada → vencido.
2. **Gerar recorrentes** — mantém séries recorrentes com 24 meses à frente.
3. **Snapshot do patrimônio** — garante 1 foto/mês por usuário com patrimônio.
4. **Resumo mensal por e-mail** — 1x/mês por cliente (controlado por `UltimoRelatorioMensalEm`).

Na **Login**: Trial Expiration (D-7/D-1) e Reengajamento.

---

## 13. Formatação (pt-BR)

- **Valores**: `R$ 1.234.567,89` (helpers manuais em `utils/format.ts`, independentes de Intl — funcionam em qualquer runtime).
- **Datas**: `dd/mm/aaaa` (exibição e máscara de input); conversão para ISO no envio.
- Backend usa `CultureInfo("pt-BR")` onde necessário.

---

## 14. Configuração / variáveis de ambiente

| Variável | Onde | Uso |
|---|---|---|
| `JwtSettings:SecretKey` | ambas | JWT compartilhado |
| `ServiceAuth:ApiKey` | **ambas (mesmo valor)** | provisionar conta + enviar e-mail server-to-server |
| `SmtpSettings:Password` | **Login** | chave do Resend (envio real) |
| `LoginApi:BaseUrl` | Patrimônio | URL da API Login (server-to-server) |
| `Frontend:BaseUrl` | Patrimônio | link dos e-mails (`/aceitar`, `/patrimonio`) |
| `Api:BaseUrl` | Patrimônio | URL pública da própria API (logo nos e-mails) |
| `ConnectionStrings:DefaultConnection` | ambas | PostgreSQL |

---

## 15. Principais entidades (banco)

`VinculoAssessoria`, `VinculoCorretor`, `DelegacaoCarteira`, `Recomendacao`, `ConsultoriaConfig`,
`AtivoPatrimonial`, `PassivoPatrimonial`, `Investimento`, `SimulacaoPatrimonial`, `MoedaParam`,
`TipoAtivoParam`, `TipoInvestimentoParam`, `PatrimonioSnapshot`, `AlocacaoAlvo`,
`Lancamento`, `Categoria`, `CartaoCredito`, `Meta`, e (na Login) `User`.

---

## 16. Convenções técnicas

- **CQRS/MediatR**: cada funcionalidade = Command/Query + Handler; testes obrigatórios (xUnit + Moq + FluentAssertions).
- **Migrations** manuais com `[Migration]` + SQL idempotente (`IF NOT EXISTS`), aplicadas no startup.
- **Middleware**: `ExceptionHandlingMiddleware` (mapeia erros → 400/403/404 com mensagem) e `AssessoriaContextMiddleware` (resolve o view-as e as permissões).
