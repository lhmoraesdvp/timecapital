# TimeCapital

TimeCapital é um SaaS minimalista focado em consistência e anti-procrastinação.

Stack:
- ASP.NET Core 8
- EF Core
- SQL Server
- MVC + API híbrido
- JS Vanilla (sem framework)

---

# 🚀 Como rodar

```bash
dotnet restore
dotnet build
dotnet run


🧠 Conceito do MVP

Regras principais:

Apenas 1 sessão ativa por usuário

Start / Stop (sem pause)

Stop grava sessão e soma horas

Cancel não soma horas

Sessão sobrevive refresh (baseado em StartTimeUtc)

Projeto padrão selecionável

🔗 Rotas importantes
Dashboard

GET /dashboard-state

Sessões

POST /sessions/start

POST /sessions/stop

POST /sessions/cancel

Projetos (API)

POST /projects/set-default

Projetos (MVC)

GET /projects → Tela de cadastro/listagem

POST /projects → Criar projeto

📂 Estrutura relevante
TimeCapital.Web
 ├─ Controllers
 │   ├─ DashboardStateController (API)
 │   ├─ ProjectsController (API)
 │   ├─ ProjectsPageController (MVC)
 │
 ├─ Views
 │   ├─ ProjectsPage
 │   │   └─ Index.cshtml
 │   └─ Dashboard.cshtml

✅ O que já funciona

Cronômetro persistente

Reconstrução via backend

Totais por projeto

Últimas sessões

Distribuição semanal (totalsByProject)

Tela MVC de projetos com criação

🔜 Próximos passos sugeridos

Editar / excluir projetos

Definir projeto padrão na tela MVC

Meta (Goal) por projeto

Relatório semanal detalhado

Autenticação real + multi-user

Melhorar layout unificado (Layout.cshtml)

🎯 Objetivo

Manter arquitetura simples.
Evoluir funcionalidade sem quebrar regras centrais do MVP.


---

Agora seu projeto está:

✔ Documentado  
✔ Estruturado  
✔ Transferível para outro chat  
✔ Profissional  

