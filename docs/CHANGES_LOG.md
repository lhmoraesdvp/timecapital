# Changelog

Todas as mudanças relevantes do projeto TimeCapital serão documentadas aqui.

---

## [1.02] - 2026-02-21

### ✅ Estabilização do Dashboard (Modo B)

- Correção do filtro por projeto no Dashboard.
- Implementado `effectiveProjectId` (selected > default > primeiro projeto).
- Correção de erro EF Core (GroupBy DateOnly não traduzível).
- Últimos 7 dias agora agrupados por Year/Month/Day (compatível com SQL Server).
- Correção de bug onde semana/hoje não atualizavam ao trocar projeto.
- Refatoração de `GetDashboardStateAsync` para fluxo determinístico.
- Uso de `AsNoTracking()` para melhorar estabilidade/performance.
- Dashboard agora 100% reconstruível via `/dashboard-state`.

### 🔒 Garantias Técnicas

- Apenas 1 sessão ativa por usuário.
- Cronômetro reconstruído via `StartTimeUtc`.
- Filtro consistente por projeto selecionado.
- Dados não dependem de memória do navegador.

---

## [1.01] - 2026-02-20

- Primeira versão funcional do Dashboard.
- Implementação Start/Stop/Cancel.
- Sessão persistente no banco.
- Estrutura inicial do DTO de Dashboard.