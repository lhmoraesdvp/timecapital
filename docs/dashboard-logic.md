xplica todos os cálculos usados no dashboard.

# 📊 Lógica do Dashboard — TimeCapital

O dashboard é totalmente baseado em cálculos reais vindos do banco.

---

# 1) Sessão Ativa
Critério:

EndTimeUtc == null && CanceledAtUtc == null


Retorno:
- id
- projectId
- goalId
- startTimeUtc

Usado para reconstruir o cronômetro no frontend.

---

# 2) Total do Dia

Somatória de DateDiffSecond(StartTimeUtc, EndTimeUtc)
Onde StartTimeUtc >= início do dia UTC


---

# 3) Total da Semana
Semana inicia na segunda-feira.


weekStart = hoje - diff


Mesmo cálculo do dia, porém filtrando desde `weekStart`.

---

# 4) Distribuição por Projeto
Agrupamento real:


GroupBy(ProjectId)
Sum(DateDiffSecond)
OrderByDescending(total)
Take(6)


Frontend:
- barras coloridas por projeto (hash do ID)
- percentual relativo ao total semanal
- medalhas top 1/2/3

---

# 5) Últimos 7 dias
Agrupado por dia normalizado:


GroupBy(DateOnly.FromDateTime(StartTimeUtc))


Mesmo se o banco usar timestamps diferentes.

Frontend:
- gráfico SVG com tooltip
- responsivo