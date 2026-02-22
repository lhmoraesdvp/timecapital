Documentação dos endpoints REST.

# 📡 API — TimeCapital

Documentação dos endpoints disponíveis no MVP atual.

---

# ▶ Sessões

## POST /sessions/start
Inicia uma sessão.

Body:
```json
{
  "projectId": "guid",
  "goalId": null
}

Retorno:

{
  "sessionId": "...",
  "startTimeUtc": "..."
}
POST /sessions/stop

Encerra sessão ativa.

Retorno:

200 OK com dados da sessão

400 se não houver sessão ativa

POST /sessions/cancel

Cancela sessão ativa sem registrar duração.

▶ Dashboard
GET /dashboard-state

Retorna todo o estado do dashboard:

projeto padrão

sessão ativa

totais

distribuição semanal

últimas sessões

últimos 7 dias

▶ Projetos
POST /projects/set-default

Define o projeto padrão do usuário.

Body:

{
  "projectId": "guid"
}

---

# 📝 **5) docs/changelog.md**
> Histórico organizado das versões.

```md
# 📝 CHANGELOG — TimeCapital

## v1.04 — Dashboard Real + Gráficos + Correções
- Distribuição semanal real com barras coloridas.
- Ranking com medalhas.
- Gráfico SVG dos últimos 7 dias com tooltip.
- Remoção total dos mocks.
- SessionService v2 completo.
- DashboardState v2 com totais reais.
- Correção do erro 500 no STOP.
- UI refinada (premium).

## v1.03
- Dashboard com base em dados reais.
- Consultas otimizadas no SessionService.
- Fix de agrupamento por DateOnly.

## v1.02
- API real conectada ao UI.
- Timer persistente inicial.

## v1.01
- Infraestrutura básica do projeto.
- Entidades, migrations, controllers base.

## v1.00
- Setup inicial do TimeCapital.