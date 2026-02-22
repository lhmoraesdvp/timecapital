# 🏗 Arquitetura do TimeCapital

O TimeCapital segue uma arquitetura limpa e organizada em camadas:
# 🏗 Arquitetura do TimeCapital

O TimeCapital segue uma arquitetura limpa e organizada em camadas:

src/
├── TimeCapital.Web/ → UI + Controllers + Views
├── TimeCapital.Application/ → Regras de negócio (Services)
├── TimeCapital.Data/ → Infraestrutura (EF Core)
└── TimeCapital.Domain/ → Entidades
---

## 📌 Camadas

### **1. Web (Apresentação)**
- Controllers REST e MVC
- Views Razor
- Dashboard (index.cshtml)
- Chamada das APIs via fetch()

### **2. Application (Serviços / Domínio da Aplicação)**
Contém a lógica central de negócios.

Serviços principais:
- `SessionService` (v2)
- Futuros serviços: Goals, Analytics, etc.

### **3. Data (Persistência)**
- `ApplicationDbContext`
- Configuração do EF Core
- Consultas otimizadas
- Mapeamento das entidades

### **4. Domain**
- Entidades:
  - `Session`
  - `Project`
  - `User`
- Regra mínima → toda a lógica fica no Application

---

## 🔄 Fluxo Geral do Dashboard

1. UI chama:

GET /dashboard-state


2. Controller chama:

_sessionService.GetDashboardStateAsync()


3. Service consulta o banco e retorna DTO unificado com:
- sessão ativa  
- totais do dia  
- totais da semana  
- distribuição por projeto  
- últimas sessões  
- últimos 7 dias  

4. Front renderiza:
- cronômetro persistente  
- gráfico de distribuição  
- gráfico dos últimos 7 dias  
- cards de totais  
- últimas sessões  

---

## 📶 Fluxo de Sessões Start/Stop/Cancel


[UI] → POST /sessions/start → SessionService.StartSessionAsync
[UI] → POST /sessions/stop → SessionService.StopSessionAsync
[UI] → POST /sessions/cancel → SessionService.CancelActiveSessionAsync


### Regras:
- Apenas uma sessão pode estar ativa.
- Sessões são persistidas com horário UTC.
- Stop calcula duração com `DateDiffSecond`.
- Dashboard sempre reconstrói o tempo.

---

## 🎯 Objetivo da Arquitetura

- Simples
- Elegante
- Extensível (futuro: metas, relatórios, mobile)
- Fácil de escalar e testar
