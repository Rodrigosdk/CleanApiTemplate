# Template `cleanapi`

Template do `dotnet new` com a arquitetura usada no PolicyDesk: Clean Architecture em camadas, ProblemDetails, EF Core, Docker, CI no GitHub Actions e testes com Testcontainers. O banco, a autenticação com JWT, o logging e a observabilidade são escolhidos na geração.

Este arquivo e a pasta `.template.config` não vão para os projetos gerados.

A fonte do template é a variante padrão (PostgreSQL, com tudo ligado) e compila e testa como um projeto normal. As variações ficam em blocos `#if (auth)`, `#if (postgres)` etc. (`<!--#if -->` em XML, `.resx` e Markdown; `//#if` em JSON; `#if` em YAML). O `Directory.Build.props` define esses símbolos só na fonte; o bloco é removido nos projetos gerados. As migrations de SQL Server e MySQL ficam em `.template.config/migrations/` e são copiadas para `DataAccess/Migrations` conforme o banco escolhido.

## Instalar

```bash
dotnet new install Rodrigosdk.CleanApi.Templates
```

Para atualizar para a versão mais nova: `dotnet new update`. Para remover: `dotnet new uninstall Rodrigosdk.CleanApi.Templates`.

## Criar um projeto

```bash
dotnet new cleanapi -n MeuProjeto
cd MeuProjeto
git init
```

### Opções

| Opção | Valores | Padrão | O que muda |
| --- | --- | --- | --- |
| `--database` | `postgres`, `sqlserver`, `mysql` | `postgres` | Provider do EF Core, migration `InitialCreate`, controle de concorrência, índice único de e-mail, tratamento de violação de unicidade, Testcontainers, docker compose e instrumentação do OpenTelemetry |
| `--auth` | `true`, `false` | `true` | Usuários (`User`, repositórios, migration), cadastro, login, perfil, Argon2, JWT e a exigência de token nos endpoints. Sem ele, o projeto sai sem entidades nem migrations |
| `--logging` | `true`, `false` | `true` | Log por requisição (`HttpLogging`) e o catálogo de logs de negócio dos casos de uso. O log de erros inesperados (500) e de cancelamento continua sempre |
| `--observability` | `true`, `false` | `true` | OpenTelemetry (traces, métricas e logs via OTLP) e o Aspire Dashboard no `compose.dev.yml` |

```bash
dotnet new cleanapi -n MeuProjeto --database sqlserver
dotnet new cleanapi -n MeuProjeto --database mysql --auth false
dotnet new cleanapi -n MeuProjeto --logging false --observability false
```

Diferenças por banco:

| | PostgreSQL | SQL Server | MySQL |
| --- | --- | --- | --- |
| Provider | `Npgsql.EntityFrameworkCore.PostgreSQL` | `Microsoft.EntityFrameworkCore.SqlServer` | `MySql.EntityFrameworkCore` (Oracle) |
| Concorrência | coluna de sistema `xmin` | `rowversion` | coluna `version` (Guid) renovada pelo `DbContext` a cada gravação |
| E-mail único entre ativos | índice filtrado | índice filtrado | coluna gerada `active_email` + índice único |
| Ferramenta no `compose.dev.yml` | pgAdmin | — | — |

O provider MySQL é o oficial da Oracle porque o Pomelo ainda não tem versão para o EF Core 10.

A palavra `Template` é substituída pelo nome informado em tudo: pastas, arquivos, namespaces, solution, Dockerfile e workflows. Variações em minúsculas (`template` → `meuprojeto`) também são trocadas, o que cobre o nome do compose, do banco e das senhas locais. Os GUIDs da solution e o `UserSecretsId` são gerados de novo em cada projeto.

Use um nome em PascalCase sem pontos (`MeuProjeto`), porque a forma minúscula vira nome de banco e de imagem Docker.

## O que vem pronto

| Camada | Conteúdo |
| --- | --- |
| `Api` | Controllers de autenticação e perfil, `ExceptionFilter`, ProblemDetails, correlation id, OpenTelemetry, Swagger com Bearer, localização pt-BR/en |
| `Application` | Use cases `RegisterUser`, `Login`, `GetUserProfile`, paginação, catálogo de logs |
| `Domain` | `Entity`, `User`, repositórios ReadOnly/WriteOnly/UpdateOnly, `IUnitOfWork`, validação de e-mail e limites de campos |
| `Infrastructure` | `DbContext`, `UserMap`, `UsersRepository`, `UnitOfWork` com concorrência otimista, Argon2, JWT, options validadas na inicialização, migration `InitialCreate` |
| `Communication` / `Exception` | Requests/responses e hierarquia de exceções com status HTTP e mensagens `.resx` |
| `tests` | Domain, Application (Moq), Infrastructure e API (Testcontainers), com builders em `CommonTestUtilities` |

## Checklist depois de gerar

1. Revisar o `LICENSE` e os badges do `README.md`.
2. Gerar a migration quando criar a primeira entidade (ou, com `--auth`, para recomeçar o histórico, apagando antes `DataAccess/Migrations/*_InitialCreate*` e o snapshot):
   `dotnet ef migrations add InitialCreate --project src/Backend/MeuProjeto.Infrastructure --output-dir DataAccess/Migrations`
3. Adicionar mensagens novas nos dois `.resx` e no `ResourcesMessageException.Designer.cs` (o `dotnet build` não regenera o Designer; o Rider/VS sim).
4. Escolher a forma de deploy; o `docker/Dockerfile` já gera a imagem de produção.

## Adicionar um novo recurso

Siga o caminho do `User`:

1. `Domain/Entities` – entidade com construtor privado, método de fábrica `Create` e validações.
2. `Domain/Repositories/<Recurso>` – interfaces ReadOnly, WriteOnly e UpdateOnly.
3. `Infrastructure/DataAccess` – `Map`, `DbSet`, repositório e registro em `DependencyInjectionExtension`.
4. `Communication` – `Request…Json` e `Response…Json`.
5. `Application/<Recurso>/<Operação>` – interface + use case, registrados em `DependencyInjectionExtension`.
6. `Api/Controllers` – controller recebendo o use case por `[FromServices]`.
7. Testes nos quatro projetos, com builders e mocks em `CommonTestUtilities`.

## Manter o template

- Ao mudar o modelo de `User`, atualize as três migrations: a da fonte (PostgreSQL) com `dotnet ef migrations add`, e as de SQL Server e MySQL gerando um projeto com `--database sqlserver`/`mysql`, rodando o mesmo comando nele e copiando os arquivos para `.template.config/migrations/<banco>/`, trocando o nome do projeto por `Template`.
- Mensagem nova no `.resx` também vai no `ResourcesMessageException.Designer.cs`. Se o Rider regenerar o Designer, ele perde os blocos `#if (auth)`; recoloque-os.
- Antes de publicar uma mudança, gere e teste as variantes (`dotnet new cleanapi -n X --database ...`), porque a fonte só exercita a combinação padrão.

## Publicar uma nova versão

O pacote é gerado pelo `pack/Rodrigosdk.CleanApi.Templates.csproj` e publicado no nuget.org pelo workflow `publish.yml`, que roda o CI, empacota, gera um projeto de teste a partir do pacote e faz o push. Nem a pasta `pack` nem esse workflow vão para os projetos gerados.

```bash
git tag v1.1.0
git push origin v1.1.0
```

A versão do pacote vem da tag. A publicação usa o Trusted Publishing do nuget.org: o workflow troca o token OIDC do GitHub por uma chave temporária (válida por 1 hora), sem API key guardada no repositório. Para isso, existem:

- no nuget.org, em **Trusted Publishing**, uma política com o dono `Rodrigosdk`, o repositório `CleanApiTemplate` e o workflow `publish.yml`;
- no repositório, o secret `NUGET_USER` com o nome de perfil do nuget.org (não o e-mail).

Em repositório privado, a política fica ativa por 7 dias até a primeira publicação; depois disso, vira permanente. Se expirar antes, reative-a no nuget.org.

Para testar o pacote localmente antes de publicar:

```bash
dotnet pack pack/Rodrigosdk.CleanApi.Templates.csproj -c Release -o artifacts
dotnet new install ./artifacts/Rodrigosdk.CleanApi.Templates.1.0.0.nupkg --force
```
