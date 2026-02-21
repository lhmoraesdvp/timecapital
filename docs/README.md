# 📊 Estado Atual — v1.02

O TimeCapital encontra-se estável na versão 1.02.

## Funcionalidades Implementadas

- Start / Stop / Cancel de sessões
- Uma sessão ativa por usuário
- Dashboard reconstruído via API
- Filtro consistente por projeto
- Totais:
  - Hoje
  - Semana
  - Últimas sessões
  - Últimos 7 dias (compatível SQL Server)

## Arquitetura Atual

Backend:
- .NET 8
- EF Core
- SQL Server

Frontend:
- Razor View
- JavaScript Vanilla
- Estado reconstruído via GET `/dashboard-state`

## Estratégia de Estado

O sistema não depende de memória do navegador.
Toda renderização é baseada na resposta da API.

Isso garante:
- Refresh seguro
- Consistência de dados
- Backend determinístico