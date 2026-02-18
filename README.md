# TimeCapital

TimeCapital é um SaaS de gestão de tempo e foco (anti-procrastinação), começando com um MVP simples:
- Timer por área (Estudo / Trabalho / Saúde / Projeto pessoal)
- Metas (Goals)
- Relatórios/Dashboard (semana, área, sessões)
- Notificações básicas (futuro)

## Stack (MVP)
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- Identity (login/register)
- HTML/CSS/JS (widgets e UI)
- GitHub Actions (CI/CD)
- GitHub Pages (vitrine estática em `/docs`)
- Azure App Service (F1 Free) para backend

## Estrutura do repositório
- `/src` → código do app (.NET)
  - `TimeCapital.Domain` → entidades/regras (Area, Session, Goal)
  - `TimeCapital.Data` → EF Core + DbContext + Migrations (Identity)
  - `TimeCapital.Web` → ASP.NET Core MVC (UI/Controllers)
- `/docs` → vitrine (GitHub Pages), dashboard afetivo + widgets
- `/design` → protótipos soltos (opcional)
- `/docs` (documentação) → arquivos de guia/decisões (ver abaixo)

## Como rodar (local)
1. Pré-requisitos: .NET SDK, SQL Server LocalDB ou SQL Server
2. Restaurar e build:
   ```bash
   dotnet restore src/TimeCapital.sln
   dotnet build src/TimeCapital.sln
