# Documentação de Requisitos — plataforma.security.api

**Projeto:** plataforma.security.api  
**Stack:** .NET Core 3.1 / C# 8.0  
**Versão:** 1.0  
**Data:** Abril 2026

---

## Sumário

1. [Visão Geral](#1-visão-geral)
2. [Arquitetura e Camadas](#2-arquitetura-e-camadas)
3. [Tipos de Usuário](#3-tipos-de-usuário)
4. [Mecanismos de Segurança](#4-mecanismos-de-segurança)
5. [Endpoints — Autenticação e Senha](#5-endpoints--autenticação-e-senha)
6. [Endpoints — Gestão de Usuários](#6-endpoints--gestão-de-usuários)
7. [Endpoints — Perfis e Módulos](#7-endpoints--perfis-e-módulos)
8. [Endpoints — Hierarquia](#8-endpoints--hierarquia)
9. [Endpoints — Agente de Carga (CargoAgent)](#9-endpoints--agente-de-carga-cargoagent)
10. [Endpoints — Freight Forwarder](#10-endpoints--freight-forwarder)
11. [Endpoints — Integração](#11-endpoints--integração)
12. [Endpoints — Termos](#12-endpoints--termos)
13. [Endpoints — Feature Flags](#13-endpoints--feature-flags)
14. [Modelos de Dados](#14-modelos-de-dados)
15. [Repositórios e Serviços](#15-repositórios-e-serviços)
16. [Regras de Negócio](#16-regras-de-negócio)
17. [Backlog de Cards Jira](#17-backlog-de-cards-jira)

---

## 1. Visão Geral

O **plataforma.security** é uma API de autenticação, autorização e gestão de usuários para uma plataforma logística. Ela centraliza:

- Autenticação via JWT com validação de ReCaptcha
- Gestão de usuários internos, externos, Freight Forwarders e Agentes de Carga
- Controle de perfis, módulos e permissões
- Hierarquias de empresas e grupos
- Integrações via token de serviço (machine-to-machine)
- Gestão de termos de uso (LGPD)
- Pré-cadastro de novos usuários

---

## 2. Arquitetura e Camadas

```
plataforma.security.api               ← Controllers, Middleware, Configurações
plataforma.security.Application       ← Services, ViewModels, DTOs, Queries
plataforma.security.Domain            ← Entidades de domínio, Agregados
plataforma.security.Domain.Core       ← Base classes, Interfaces, Enums
plataforma.security.Infra.Data        ← EF Core, Migrations, Configurações de entidades
plataforma.security.Infra.CrossCutting← LDAP, Identity, Criptografia, Storage
plataforma.security.Api.Tests         ← Testes de API (JWT)
plataforma.security.Application.Tests ← Testes de serviço (User, Mocks)
```

**Bancos de dados utilizados:**
- SQL Server (via Entity Framework Core + Dapper) — dados transacionais
- MongoDB — configurações de usuário e preferências (UserSettings, SelectedCompanies)

---

## 3. Tipos de Usuário

| ID | Tipo            | Descrição                                 |
|----|-----------------|-------------------------------------------|
| 1  | Internal        | Usuário interno da plataforma             |
| 2  | External        | Usuário externo (cliente embarcador)      |
| 3  | FreightForwarder| Usuário de empresa despachante/transitária|
| 4  | CargoAgent      | Usuário de agente de carga                |

---

## 4. Mecanismos de Segurança

- **JWT Bearer Token** — gerado no login e em integrações. Invalidado no logout, atualização ou exclusão de usuário.
- **ReCaptcha** — validado no login em ambientes de Produção/Homologação.
- **SignatureAuthFilter** — filtro customizado para endpoints machine-to-machine (ex: `user-companies`).
- **LDAP** — suporte a autenticação via diretório corporativo (Active Directory).
- **Criptografia** — senhas e tokens gerenciados pela classe `Cryptography`.
- **Token de Reset de Senha** — gerenciado por `IResetTokenManager`, com validação de hash e código de segurança.

---

## 5. Endpoints — Autenticação e Senha

### `POST /User/authenticate`

**Descrição:** Autentica o usuário e retorna o token JWT, módulos, hierarquias, restrições e preferências.  
**Auth:** Anônimo  
**ReCaptcha:** Obrigatório em Produção/Homologação

**Request Body:**
```json
{
  "email": "string",
  "password": "string",
  "captcha": "string",
  "termName": "string"
}
```

**Response 200:**
```json
{
  "accessToken": "string (JWT)",
  "avatar": "string (URL)",
  "modules": [ { "id": "guid", "name": "string", "hiddenMenu": "bool" } ],
  "hierarchies": [ { "companies": [ { "clientId": "int" } ] } ],
  "restrictions": [ { "moduleId": "guid", "companyId": "int" } ],
  "selectedCompanies": [ "int" ],
  "preferences": {}
}
```

**Response 400:** Notificações de erro (captcha inválido, credenciais inválidas)

---

### `POST /User/forgotPassword`

**Descrição:** Solicita a redefinição de senha enviando e-mail/link ao usuário.  
**Auth:** Anônimo

**Request Body:**
```json
{ "identificador": "string (email ou CPF)" }
```

**Response 200:** `{ "data": "..." }`  
**Response 400:** Notificações de erro

---

### `POST /User/forgotPassword/validateHash`

**Descrição:** Valida o hash recebido no link de redefinição de senha.  
**Auth:** Anônimo

**Request Body:**
```json
{
  "document": "string",
  "token": "string"
}
```

**Response 200:** Dados de validação  
**Response 400:** Notificações de erro

---

### `POST /User/forgotPassword/validateSecurityCode`

**Descrição:** Valida o código de segurança enviado por e-mail/SMS na redefinição de senha.  
**Auth:** Anônimo

**Request Body:**
```json
{
  "document": "string",
  "token": "string"
}
```

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `PUT /User/password`

**Descrição:** Redefine a senha do usuário via fluxo de recuperação (anônimo, com token).  
**Auth:** Anônimo

**Request Body:**
```json
{
  "document": "string",
  "password": "string (nova senha)",
  "token": "string",
  "termName": "string"
}
```

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `PUT /User/redefinePassword`

**Descrição:** Redefine a senha do usuário autenticado (troca de senha logado).  
**Auth:** Bearer Token

**Request Body:**
```json
{
  "document": "string",
  "password": "string (nova senha)",
  "token": "string"
}
```

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `POST /User/checkToken`

**Descrição:** Verifica se o token JWT do usuário logado ainda é válido.  
**Auth:** Bearer Token

**Response 200:** `true | false`

---

## 6. Endpoints — Gestão de Usuários

### `GET /User`

**Descrição:** Lista usuários com paginação e filtros.  
**Auth:** Bearer Token

**Query Params:**
| Campo           | Tipo   | Descrição                          |
|-----------------|--------|------------------------------------|
| Hierarchy_Id    | Guid   | Filtro por hierarquia              |
| Profile_Id      | Guid   | Filtro por perfil                  |
| StatusId        | int    | Filtro por status                  |
| Email           | string | Filtro por e-mail                  |
| Name            | string | Filtro por nome                    |
| Document        | string | Filtro por CPF/CNPJ                |
| UserTypeId      | int    | Filtro por tipo de usuário         |
| Region          | string | Filtro por região                  |
| RequestDateStart| string | Data início (dd/MM/yyyy)           |
| RequestDateEnd  | string | Data fim (dd/MM/yyyy)              |
| PageSize        | int    | Itens por página                   |
| CurrentPage     | int    | Página atual                       |
| ColumnName      | string | Coluna para ordenação              |
| Direction       | string | Direção (asc/desc)                 |

**Response 200:** Lista paginada de usuários

---

### `GET /User/{id}`

**Descrição:** Busca um usuário específico pelo ID.  
**Auth:** Bearer Token

**Path Params:** `id` (Guid)

**Response 200:** Dados do usuário  
**Response 403:** Proibido  
**Response 404:** Não encontrado

---

### `GET /User/usertype`

**Descrição:** Retorna todos os tipos de usuário disponíveis.  
**Auth:** Bearer Token

**Response 200:** Lista de UserTypes

---

### `GET /User/ListByCompany/{id}`

**Descrição:** Lista usuários de uma empresa específica.  
**Auth:** Anônimo

**Path Params:** `id` (string — ID da empresa)

**Response 200:** Lista de usuários

---

### `GET /User/ListAllUsers`

**Descrição:** Lista todos os usuários do sistema.  
**Auth:** Anônimo

**Response 200:** Lista de usuários

---

### `GET /User/company/{companyId}/modules`

**Descrição:** Retorna os módulos disponíveis para uma empresa.  
**Auth:** Bearer Token

**Path Params:** `companyId` (int)

**Response 200:** Lista de módulos

---

### `GET /User/selected-companies`

**Descrição:** Retorna as empresas selecionadas pelo usuário logado (filtra por módulo para externos).  
**Auth:** Bearer Token  
**Restrição:** Proibido para FreightForwarder

**Query Params:** `moduleId` (Guid, opcional)

**Response 200:** Lista de IDs de empresas (`int[]`)  
**Response 403:** Proibido para Freight Forwarder

---

### `GET /User/ok`

**Descrição:** Health check da API.  
**Auth:** Anônimo

**Response 200:** `{ "success": true }`

---

### `GET /User/user-companies`

**Descrição:** Retorna as empresas associadas a um e-mail. Endpoint protegido por SignatureAuth (machine-to-machine).  
**Auth:** SignatureAuthFilter

**Query Params:** `email` (string)

**Response 200:** Lista de empresas

---

### `POST /User/preregister`

**Descrição:** Pré-cadastra um novo usuário externo e aceita o termo de uso automaticamente.  
**Auth:** Anônimo

**Request Body:**
```json
{
  "name": "string",
  "document": "string (CPF)",
  "email": "string",
  "cellphone": "string",
  "phone": "string",
  "occupation": "string",
  "address": "string",
  "companyName": "string",
  "companyCnpj": "string",
  "countryId": "int",
  "region": "string",
  "termName": "string"
}
```

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `POST /User/external`

**Descrição:** Cria um usuário externo (multipart: dados JSON + avatar).  
**Auth:** Bearer Token

**Request:** `multipart/form-data`
- `user` (JSON): ver `MaintainUserViewModel`
- `avatar` (file, opcional): imagem do avatar

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `POST /User/internal`

**Descrição:** Cria um usuário interno (multipart: dados JSON + avatar).  
**Auth:** Bearer Token

**Request:** `multipart/form-data` — mesmo formato de `POST /User/external`

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `PUT /User/{id}`

**Descrição:** Atualiza o perfil de um usuário (multipart: dados JSON + avatar).  
**Auth:** Bearer Token

**Path Params:** `id` (Guid)

**Request:** `multipart/form-data`
- `user` (JSON — `UserProfileViewModel`): `name`, `document`, `occupation`, `address`, `cellphone`, `phone`, `avatar`, `password`, `countryId`, `region`
- `avatar` (file, opcional)

**Response 200:** Dados atualizados do usuário  
**Response 400 / 403 / 404:** Erros

---

### `PUT /User/external`

**Descrição:** Atualiza um usuário externo existente.  
**Auth:** Bearer Token  
**Efeito colateral:** Invalida o token JWT do usuário alterado.

**Request:** `multipart/form-data` — mesmo formato de `POST /User/external`

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `PUT /User/externalResend`

**Descrição:** Atualiza usuário externo e reenvia o convite por e-mail.  
**Auth:** Bearer Token

**Request:** mesmo de `PUT /User/external`

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `PUT /User/internal`

**Descrição:** Atualiza um usuário interno existente.  
**Auth:** Bearer Token  
**Efeito colateral:** Invalida o token JWT do usuário alterado.

**Request:** `multipart/form-data` — mesmo formato de `POST /User/internal`

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `PUT /User/freightforwarder`

**Descrição:** Atualiza dados de um usuário Freight Forwarder.  
**Auth:** Bearer Token  
**Efeito colateral:** Invalida o token JWT do usuário alterado.

**Request Body:** `MaintainFreightForwarderViewModel`

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `PUT /User/freightforwarderResend`

**Descrição:** Reenvia o convite para um usuário Freight Forwarder.  
**Auth:** Bearer Token

**Request Body:** `Guid` (ID do usuário)

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `PUT /User/update-selected-companies`

**Descrição:** Atualiza a lista de empresas selecionadas do usuário logado.  
**Auth:** Bearer Token  
**Restrição:** Apenas usuários do tipo Internal

**Request Body:**
```json
{ "companies": [ "int" ] }
```

**Response 200:** OK  
**Response 403:** Proibido para não-Internal

---

### `PUT /User/cookies`

**Descrição:** Salva preferências de cookies do usuário.  
**Auth:** Bearer Token

**Request Body:** `UserCookiesDto`

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `DELETE /User/{id}`

**Descrição:** Exclui um usuário e invalida seu token JWT.  
**Auth:** Bearer Token

**Path Params:** `id` (Guid)

**Response 200:** Dados do usuário excluído  
**Response 404:** Não encontrado

---

### `DELETE /User/rejectTerms`

**Descrição:** Exclui o usuário logado e limpa todos os seus dados (rejeição de termos de uso).  
**Auth:** Bearer Token

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `DELETE /User/freightforwarder/{id}`

**Descrição:** Exclui um usuário do tipo Freight Forwarder e limpa seus dados.  
**Auth:** Bearer Token

**Path Params:** `id` (Guid)

**Response 200:** `{}`  
**Response 400:** Erro se o usuário não for do tipo FreightForwarder

---

## 7. Endpoints — Perfis e Módulos

### `GET /Profile`

**Descrição:** Lista perfis com filtros.  
**Auth:** Bearer Token

**Query Params:**
| Campo   | Tipo   | Descrição              |
|---------|--------|------------------------|
| Type_Id | int    | Filtro por tipo        |
| Name    | string | Filtro por nome        |

**Response 200:** Lista de perfis

---

### `GET /Profile/UserType/{id}`

**Descrição:** Retorna perfis associados a um tipo de usuário.  
**Auth:** Bearer Token

**Path Params:** `id` (string — UserType ID)

**Response 200:** Lista de perfis com usuários associados

---

### `POST /Profile`

**Descrição:** Cria um novo perfil de acesso.  
**Auth:** Bearer Token

**Request Body:**
```json
{
  "id": "string (guid vazio para criação)",
  "name": "string",
  "userTypeId": "int",
  "permissions": [
    { "moduleId": "guid", "functionId": "guid" }
  ]
}
```

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `PUT /Profile`

**Descrição:** Atualiza um perfil de acesso existente.  
**Auth:** Bearer Token  
**Efeito colateral:** Invalida tokens JWT de todos os usuários vinculados ao perfil.

**Request Body:** Mesmo de `POST /Profile` (com `id` preenchido)

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `DELETE /Profile/{id}`

**Descrição:** Exclui um perfil de acesso.  
**Auth:** Bearer Token  
**Efeito colateral:** Invalida tokens JWT de todos os usuários vinculados ao perfil.

**Path Params:** `id` (string — Guid do perfil)

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `GET /Module`

**Descrição:** Lista todos os módulos disponíveis na plataforma.  
**Auth:** Bearer Token

**Response 200:** Lista de módulos

---

### `POST /Module`

**Descrição:** Stub — retorna OK sem ação. (Endpoint em desenvolvimento)  
**Auth:** Bearer Token

**Response 200:** OK

---

## 8. Endpoints — Hierarquia

### `GET /Hierarchy`

**Descrição:** Retorna a árvore de hierarquias (grupos e empresas). Para usuários internos, retorna tudo; para externos, filtra pela hierarquia do usuário.  
**Auth:** Bearer Token

**Response 200:** Lista de hierarquias com grupos e empresas

---

### `GET /Hierarchy/groups-companies`

**Descrição:** Retorna IDs de empresas pertencentes a grupos específicos.  
**Auth:** Bearer Token

**Query Params:** `groups` (List<Guid>)

**Response 200:** Lista de IDs de empresa (`int[]`)

---

### `GET /Hierarchy/{companyId}/group`

**Descrição:** Retorna o grupo ao qual uma empresa pertence.  
**Auth:** Bearer Token

**Path Params:** `companyId` (int)

**Response 200:** Dados do grupo  
**Response 400:** Notificações de erro

---

### `GET /Hierarchy/syncActiveTypeCompanies`

**Descrição:** Sincroniza o tipo ativo das empresas com a API da plataforma principal.  
**Auth:** Bearer Token  
**Nota:** Passa o token Bearer do usuário para a API externa.

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `POST /Hierarchy`

**Descrição:** Cria ou atualiza um grupo na hierarquia.  
**Auth:** Bearer Token

**Request Body:**
```json
{
  "groupId": "int (0 para criação)",
  "name": "string",
  "active": "bool"
}
```

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `POST /Hierarchy/{groupId}/company`

**Descrição:** Adiciona uma empresa a um grupo da hierarquia.  
**Auth:** Bearer Token  
**Nota:** Repassa o Bearer token para a API da plataforma principal.

**Path Params:** `groupId` (int)

**Request Body:**
```json
{
  "id": "int",
  "companyId": "int",
  "name": "string",
  "active": "bool",
  "register": "string (CNPJ/CPF)",
  "type": "string (PJ/PF/CUIT)",
  "ie": "string",
  "vgmDeclarant": "bool",
  "email": "string",
  "zipCode": "string",
  "neighborhood": "string",
  "city": "string",
  "state": "string",
  "address": "string",
  "number": "string",
  "complement": "string",
  "requiresCounterFoil": "bool",
  "isBlocked": "bool"
}
```

**Response 200:** OK  
**Response 400:** Notificações de erro

---

## 9. Endpoints — Agente de Carga (CargoAgent)

### `GET /CargoAgent`

**Descrição:** Lista agentes de carga com paginação e filtros.  
**Auth:** Bearer Token

**Query Params:** `CargoAgentFilter` (campos de paginação e filtro)

**Response 200:** `Pagination<CargoAgentDto>`
```json
{
  "items": [
    {
      "id": "int",
      "companyName": "string",
      "document": "string",
      "email": "string",
      "associated": "bool"
    }
  ],
  "totalItems": "int",
  "currentPage": "int",
  "pageSize": "int"
}
```

---

### `GET /CargoAgent/filter`

**Descrição:** Retorna lista simplificada de agentes de carga para uso em filtros de tela.  
**Auth:** Bearer Token

**Response 200:** `List<CargoAgentFilterDto>`

---

### `GET /CargoAgent/permissions`

**Descrição:** Retorna as permissões de um agente de carga específico.  
**Auth:** Bearer Token

**Query Params:** `id` (int, opcional)

**Response 200:** `CargoAgentPermissionsDto`

---

### `GET /CargoAgent/user/permissions`

**Descrição:** Retorna as permissões do usuário logado como agente de carga.  
**Auth:** Bearer Token

**Response 200:** `List<CargoAgentUserPermissionDto>`

---

### `POST /CargoAgent`

**Descrição:** Cria a associação entre um agente de carga e um cliente.  
**Auth:** Bearer Token

**Request Body:**
```json
{
  "cargoAgentCompanyId": "int (empresa agente)",
  "cargoAgentClientCompanyId": "int (empresa cliente)"
}
```

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `PUT /CargoAgent/permissions`

**Descrição:** Atualiza as permissões de um agente de carga.  
**Auth:** Bearer Token

**Request Body:**
```json
{
  "cargoAgentId": "int",
  "documents": "bool",
  "tracking": "bool",
  "booking": "bool",
  "bl": "bool"
}
```

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `PUT /CargoAgent/association`

**Descrição:** Atualiza o status de associação de um agente de carga (associa ou desassocia).  
**Auth:** Bearer Token

**Request Body:**
```json
{
  "cargoAgentId": "int",
  "associated": "bool",
  "unassociatedReason": "string (obrigatório se associated = false)"
}
```

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `DELETE /CargoAgent`

**Descrição:** Remove a associação de um agente de carga com um cliente.  
**Auth:** Bearer Token

**Request Body:** Mesmo de `POST /CargoAgent`

**Response 200:** OK  
**Response 400:** Notificações de erro

---

## 10. Endpoints — Freight Forwarder

### `GET /FreightForwarder`

**Descrição:** Lista freight forwarders com paginação e filtros.  
**Auth:** Bearer Token

**Query Params:** `FreightForwarderFilter`

**Response 200:** `Pagination<FreightForwarderDto>`

---

### `GET /FreightForwarder/{freightForwarderId}`

**Descrição:** Busca um freight forwarder pelo ID.  
**Auth:** Bearer Token

**Path Params:** `freightForwarderId` (Guid)

**Response 200:** `FreightForwarderFormDto`  
**Response 404:** Não encontrado

---

### `GET /FreightForwarder/find`

**Descrição:** Busca um freight forwarder pelo documento (CNPJ/CPF).  
**Auth:** Bearer Token

**Query Params:** `document` (string)

**Response 200:** `FreightForwarderFormDto` ou vazio

---

### `GET /FreightForwarder/filter`

**Descrição:** Retorna lista simplificada de freight forwarders para filtros de tela.  
**Auth:** Bearer Token

**Response 200:** `List<FreightForwarderFilterDto>`

---

### `GET /FreightForwarder/permissions`

**Descrição:** Retorna as permissões de um freight forwarder específico.  
**Auth:** Bearer Token

**Query Params:** `id` (Guid)

**Response 200:** `FreightForwarderPermissionsDto`

---

### `GET /FreightForwarder/user`

**Descrição:** Busca um usuário vinculado a um freight forwarder pelo documento.  
**Auth:** Bearer Token

**Query Params:**
- `freightForwarderId` (Guid)
- `document` (string)

**Response 200:** `FreightForwarderUserDto`

---

### `GET /FreightForwarder/user/permissions`

**Descrição:** Retorna as permissões do usuário logado como freight forwarder.  
**Auth:** Bearer Token

**Response 200:** `List<FreightForwarderUserPermissionDto>`

---

### `GET /FreightForwarder/associations/{userId}`

**Descrição:** Retorna as empresas associadas ao usuário freight forwarder.  
**Auth:** Bearer Token

**Path Params:** `userId` (Guid)

**Response 200:** `FreightForwarderAssociationCompanyDto`

---

### `GET /FreightForwarder/associationUser/{userId}/{companyId}`

**Descrição:** Retorna o ID do freight forwarder vinculado a um usuário em uma empresa específica.  
**Auth:** Bearer Token

**Path Params:**
- `userId` (Guid)
- `companyId` (int)

**Response 200:** `Guid`

---

### `POST /FreightForwarder`

**Descrição:** Cria e associa um freight forwarder a uma hierarquia.  
**Auth:** Bearer Token

**Request Body:**
```json
{
  "id": "guid (null para criação)",
  "companyName": "string",
  "tradingName": "string",
  "document": "string",
  "documentType": "string",
  "phone": "string",
  "email": "string",
  "responsibleName": "string",
  "address": {
    "zipCode": "string",
    "street": "string",
    "number": "string",
    "complement": "string",
    "neighborhood": "string",
    "city": "string",
    "state": "string",
    "country": "string"
  },
  "associatedCompanies": [ "int" ],
  "associatedUsers": [ { "id": "guid" } ],
  "associatedCreatedUsers": [ { "id": "guid" } ],
  "associatedNewUsers": [ { "name": "string", "email": "string", "document": "string" } ]
}
```

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `PUT /FreightForwarder/permissions`

**Descrição:** Atualiza permissões de um freight forwarder.  
**Auth:** Bearer Token

**Request Body:**
```json
{
  "freightForwarderId": "guid",
  "documents": "bool",
  "tracking": "bool",
  "booking": "bool",
  "bl": "bool"
}
```

**Response 200:** OK  
**Response 400:** Notificações de erro

---

### `PUT /FreightForwarder/association`

**Descrição:** Atualiza o status de associação de um freight forwarder.  
**Auth:** Bearer Token

**Request Body:**
```json
{
  "freightForwarderId": "guid",
  "associated": "bool",
  "unassociatedReason": "string"
}
```

**Response 200:** OK  
**Response 400:** Notificações de erro

---

## 11. Endpoints — Integração

### `POST /Integration/authorize`

**Descrição:** Endpoint de autenticação machine-to-machine. Recebe credenciais de integração e retorna um token JWT.  
**Auth:** Anônimo

**Request Body:**
```json
{
  "login": "string",
  "password": "string"
}
```

**Response 200:**
```json
{ "accessToken": "string (JWT)" }
```

**Response 400:** Notificações de erro

---

## 12. Endpoints — Termos

### `GET /Term/{term}/accepted`

**Descrição:** Verifica se o usuário logado já aceitou um termo específico.  
**Auth:** Bearer Token

**Path Params:** `term` (string — nome do termo)

**Response 200:** `true | false`  
**Response 400:** Notificações de erro

---

### `POST /Term/{term}/accept`

**Descrição:** Registra o aceite de um termo pelo usuário logado. Captura headers e IP do request.  
**Auth:** Bearer Token

**Path Params:** `term` (string — nome do termo)

**Response 200:** OK  
**Response 400:** Notificações de erro

---

## 13. Endpoints — Feature Flags

### `GET /Feature/searchNPS`

**Descrição:** Verifica se o usuário logado tem acesso à feature de NPS (lista de e-mails permitidos).  
**Auth:** Bearer Token

**Response 200:**
```json
{
  "email": "string",
  "hasAccess": "bool"
}
```

**Response 400:** E-mail não encontrado no token

---

## 14. Modelos de Dados

### MaintainUserViewModel (criar/editar usuários internos e externos)

| Campo               | Tipo   | Descrição                         |
|---------------------|--------|-----------------------------------|
| Id                  | string | Guid do usuário (vazio para criar)|
| UserTypeId          | int    | Tipo de usuário                   |
| Document            | string | CPF do usuário                    |
| Name                | string | Nome completo                     |
| Email               | string | E-mail                            |
| Address             | string | Endereço                          |
| Cellphone           | string | Celular                           |
| Phone               | string | Telefone                          |
| Occupation          | string | Cargo/Ocupação                    |
| ProfileId           | string | ID do perfil de acesso            |
| HierarchyId         | string | ID da hierarquia                  |
| FreightForwarderId  | string | ID do Freight Forwarder           |
| CompanyDocument     | string | CNPJ/CPF da empresa               |
| CompanyDocumentType | string | Tipo do documento da empresa      |
| CompanyName         | string | Razão social da empresa           |
| CompanyPhone        | string | Telefone da empresa               |
| ResponsibleName     | string | Nome do responsável               |
| CompanyZipCode      | string | CEP da empresa                    |
| CompanyCity         | string | Cidade                            |
| CompanyStreetAddress| string | Logradouro                        |
| CompanyNumber       | int    | Número                            |
| IsBlocked           | bool   | Se o usuário está bloqueado       |
| CountryId           | int    | ID do país                        |
| Region              | string | Região                            |
| Restrictions        | List   | Restrições de módulo por empresa  |

### RestrictionViewModel

| Campo    | Tipo | Descrição            |
|----------|------|----------------------|
| ModuleId | Guid | ID do módulo         |
| CompanyId| int  | ID da empresa cliente|

### ProfileViewModel

| Campo       | Tipo   | Descrição                      |
|-------------|--------|--------------------------------|
| Id          | string | Guid do perfil                 |
| Name        | string | Nome do perfil                 |
| UserTypeId  | int    | Tipo de usuário vinculado      |
| Permissions | List   | Lista de permissões (módulo/fn)|

---

## 15. Repositórios e Serviços

### Serviços da Camada Application

| Interface                  | Implementação              | Responsabilidade                                    |
|----------------------------|----------------------------|-----------------------------------------------------|
| IUserService               | UserService                | CRUD de usuários, login, reset de senha, avatar     |
| IProfileService            | ProfileService             | CRUD de perfis de acesso                            |
| IModuleService             | ModuleService              | Listagem de módulos e funções                       |
| IHierarchyService          | HierarchyService           | Gestão de grupos e empresas na hierarquia           |
| IFreightForwarderService   | FreightForwarderService    | CRUD e associação de Freight Forwarders             |
| ICargoAgentService         | CargoAgentService          | CRUD e associação de Agentes de Carga               |
| IIntegrationService        | IntegrationService         | Autenticação machine-to-machine                     |
| ITermService               | TermService                | Aceite e verificação de termos de uso               |
| INotificationService       | NotificationService        | Envio de notificações (e-mail/push)                 |

### Infraestrutura Cross-Cutting

| Classe/Interface   | Responsabilidade                                 |
|--------------------|--------------------------------------------------|
| ITokenManager      | Geração, validação e invalidação de JWT          |
| IResetTokenManager | Gerenciamento de tokens de reset de senha        |
| IUserAccessor      | Acesso ao usuário logado via HttpContext         |
| ICryptography      | Criptografia de senhas e dados sensíveis         |
| ILdap              | Autenticação via Active Directory                |
| IFileStorage       | Upload e gestão de arquivos (avatar)             |
| IMongo             | Acesso ao MongoDB (preferências, empresas selecionadas)|
| IReCaptcha         | Validação do Google ReCaptcha                    |

### Agregados do Domínio

| Agregado              | Entidades Principais                                      |
|-----------------------|-----------------------------------------------------------|
| UserAggregate         | User, UserLog, UserCookies, UserCountry, UserStatus, Restriction |
| ProfileAggregate      | Profile, Permission                                       |
| ModuleAggregate       | Module, Function, RoleType                                |
| HierarchyAggregate    | Hierarchy, Company                                        |
| FreightForwarderAggregate | FreightForwarder, FreightForwarderClient, FreightForwarderPermissions |
| CargoAgentAggregate   | CargoAgentClient, CargoAgentPermissions, CargoAgentClientAssociationLog |
| TermAggregate         | AcceptedTerm                                              |
| IntegrationAggregate  | Integration                                               |
| UserSettingsAggregate | UserSettings, Channels, Events (MongoDB)                  |

---

## 16. Regras de Negócio

1. **Autenticação**: ReCaptcha obrigatório em Produção e Homologação. Ambientes Localhost e Development são isentos.
2. **Token JWT**: Invalidado automaticamente ao atualizar perfil, senha, tipo ou excluir um usuário.
3. **Restrições de módulo**: Usuários externos podem ter acesso a módulos restrito por empresa. Empresas onde todos os módulos restritos estão bloqueados são removidas da hierarquia retornada no login.
4. **Empresas selecionadas**: Apenas usuários do tipo Internal podem ter empresas selecionadas via MongoDB (multi-empresa).
5. **FreightForwarder no menu Docs**: O módulo Docs é ocultado no menu (`hiddenMenu: true`) para usuários do tipo FreightForwarder.
6. **Exclusão de FreightForwarder**: Só é permitida se o usuário for efetivamente do tipo FreightForwarder; caso contrário, retorna erro.
7. **Pré-cadastro**: O aceite de termo de uso é automático no fluxo de pré-cadastro.
8. **Permissions de CargoAgent/FreightForwarder**: Controladas por flags booleanas: `Documents`, `Tracking`, `Booking`, `BL`.
9. **Background Job**: Existe um job (`RemoveUnfinishedFreightForwarderUserDataBackgroundJob`) que remove dados incompletos de usuários FreightForwarder.
10. **CPF Validation**: Existe helper de validação de CPF (`CpfValidation`).
11. **Feature NPS**: Acesso controlado por lista estática de e-mails permitidos (`FeatureAllowedEmails`).
12. **Módulos Restritos**: Existe uma lista estática (`ModulesRestricted`) de módulos sujeitos à lógica de restrição por empresa.

---

## 17. Backlog de Cards Jira

### EPIC: Autenticação e Segurança

| Card | Título | Endpoints relacionados | Prioridade |
|------|--------|------------------------|------------|
| SEC-001 | Documentar fluxo de autenticação JWT | POST /User/authenticate | Alta |
| SEC-002 | Documentar fluxo de recuperação de senha (3 etapas) | POST /forgotPassword, validateHash, validateSecurityCode, PUT /password | Alta |
| SEC-003 | Documentar autenticação de integração M2M | POST /Integration/authorize | Alta |
| SEC-004 | Revisar e documentar validação de ReCaptcha por ambiente | POST /User/authenticate | Média |
| SEC-005 | Mapear lógica de invalidação de token (logout/update) | PUT /User/{id}, DELETE /User/{id} | Média |

### EPIC: Gestão de Usuários

| Card | Título | Endpoints relacionados | Prioridade |
|------|--------|------------------------|------------|
| USR-001 | Documentar CRUD de usuário interno | POST/PUT /User/internal, DELETE /User/{id} | Alta |
| USR-002 | Documentar CRUD de usuário externo | POST/PUT /User/external, PUT /User/externalResend | Alta |
| USR-003 | Documentar pré-cadastro e aceite de termos | POST /User/preregister | Alta |
| USR-004 | Documentar gestão de avatar de usuário | PUT /User/{id}, POST /User/external | Média |
| USR-005 | Documentar atualização de senha por usuário logado | PUT /User/redefinePassword | Média |
| USR-006 | Documentar filtros e paginação de listagem de usuários | GET /User | Média |
| USR-007 | Documentar rejeição de termos e exclusão de dados | DELETE /User/rejectTerms | Alta |
| USR-008 | Documentar listagem de usuários por empresa | GET /User/ListByCompany/{id} | Baixa |
| USR-009 | Documentar endpoint machine-to-machine de empresas por e-mail | GET /User/user-companies | Média |

### EPIC: Freight Forwarder

| Card | Título | Endpoints relacionados | Prioridade |
|------|--------|------------------------|------------|
| FF-001 | Documentar criação e associação de Freight Forwarder | POST /FreightForwarder | Alta |
| FF-002 | Documentar gestão de permissões de Freight Forwarder | GET/PUT /FreightForwarder/permissions | Alta |
| FF-003 | Documentar gestão de associação de Freight Forwarder | PUT /FreightForwarder/association | Alta |
| FF-004 | Documentar busca por documento e por ID | GET /FreightForwarder/find, GET /FreightForwarder/{id} | Média |
| FF-005 | Documentar listagem de empresas associadas ao usuário FF | GET /FreightForwarder/associations/{userId} | Média |
| FF-006 | Documentar CRUD de usuário Freight Forwarder | PUT /User/freightforwarder, DELETE /User/freightforwarder/{id} | Alta |
| FF-007 | Documentar reenvio de convite para Freight Forwarder | PUT /User/freightforwarderResend | Baixa |

### EPIC: Agente de Carga

| Card | Título | Endpoints relacionados | Prioridade |
|------|--------|------------------------|------------|
| CA-001 | Documentar criação e desassociação de Agente de Carga | POST/DELETE /CargoAgent | Alta |
| CA-002 | Documentar gestão de permissões de Agente de Carga | GET/PUT /CargoAgent/permissions | Alta |
| CA-003 | Documentar gestão de associação de Agente de Carga | PUT /CargoAgent/association | Alta |
| CA-004 | Documentar permissões do usuário logado como Cargo Agent | GET /CargoAgent/user/permissions | Média |

### EPIC: Perfis e Módulos

| Card | Título | Endpoints relacionados | Prioridade |
|------|--------|------------------------|------------|
| MOD-001 | Documentar CRUD de Perfil de Acesso | POST/PUT/DELETE /Profile | Alta |
| MOD-002 | Documentar listagem de módulos e permissões | GET /Module, GET /Profile | Alta |
| MOD-003 | Documentar impacto de atualização de perfil nos tokens | PUT/DELETE /Profile | Alta |
| MOD-004 | Documentar busca de perfis por tipo de usuário | GET /Profile/UserType/{id} | Média |

### EPIC: Hierarquia

| Card | Título | Endpoints relacionados | Prioridade |
|------|--------|------------------------|------------|
| HIE-001 | Documentar gestão de grupos de hierarquia | POST /Hierarchy | Alta |
| HIE-002 | Documentar gestão de empresas em grupos | POST /Hierarchy/{groupId}/company | Alta |
| HIE-003 | Documentar sincronização de tipo ativo de empresas | GET /Hierarchy/syncActiveTypeCompanies | Média |
| HIE-004 | Documentar listagem de empresas por grupo | GET /Hierarchy/groups-companies | Média |
| HIE-005 | Documentar busca de grupo por empresa | GET /Hierarchy/{companyId}/group | Baixa |

### EPIC: Termos de Uso (LGPD)

| Card | Título | Endpoints relacionados | Prioridade |
|------|--------|------------------------|------------|
| TERM-001 | Documentar fluxo de aceite de termos | POST /Term/{term}/accept | Alta |
| TERM-002 | Documentar verificação de aceite de termos | GET /Term/{term}/accepted | Alta |
| TERM-003 | Documentar aceite automático no pré-cadastro | POST /User/preregister | Alta |

### EPIC: Features e Melhorias

| Card | Título | Endpoints relacionados | Prioridade |
|------|--------|------------------------|------------|
| FEAT-001 | Documentar feature flag NPS | GET /Feature/searchNPS | Baixa |
| FEAT-002 | Avaliar e implementar POST /Module (endpoint stub) | POST /Module | Baixa |
| FEAT-003 | Revisar endpoints anônimos de listagem de usuários | GET /User/ListAllUsers, GET /User/ListByCompany | Média |
| FEAT-004 | Revisar e documentar Background Job de limpeza de dados FF | (BackgroundJob) | Média |
