# Template `cleanapi`

Template do `dotnet new` com a arquitetura usada no PolicyDesk: Clean Architecture em camadas, usuários com JWT, ProblemDetails, OpenTelemetry, EF Core + PostgreSQL, Docker, CI no GitHub Actions e testes com Testcontainers.

Este arquivo e a pasta `.template.config` não vão para os projetos gerados.


## Instalar

```bash
dotnet new install ~/RiderProjects/CleanApiTemplate
```

Depois de alterar o template, rode o mesmo comando com `--force` para atualizar a instalação. Para remover: `dotnet new uninstall ~/RiderProjects/CleanApiTemplate`.

## Criar um projeto

```bash
dotnet new cleanapi -n MeuProjeto
cd MeuProjeto
git init
```

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
2. Para recomeçar o histórico do banco, apague `DataAccess/Migrations/*_InitialCreate*` e o snapshot e gere de novo:
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

