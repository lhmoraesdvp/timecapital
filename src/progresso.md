# TimeCapital — Progresso do Projeto

## 📅 Data de Atualização
18/02/2026

---

# 1. Visão Geral

O TimeCapital é um SaaS de gestão de tempo e foco com arquitetura pensada desde o início para:

- Separação clara de camadas
- Deploy automatizado
- Escalabilidade futura
- MVP funcional com base sólida

Neste estágio, o foco foi:

- Estruturação da solução .NET
- Configuração do EF Core + Identity
- Alinhamento para .NET 8 (compatível com Azure App Service)
- Preparação do pipeline de deploy

---

# 2. Infraestrutura Criada

## ✅ Azure

- App Service criado:
  - Nome: `timecapital-web`
  - Plano: F1 (Free)
  - Região: Brazil South
- Publish Profile baixado
- Pronto para integração com GitHub Actions

---

## ✅ GitHub

- Repositório estruturado
- Secret para deploy via Publish Profile preparado
- Workflow de CI/CD definido (deploy automático ao dar push na `main`)

---

# 3. Arquitetura da Aplicação

## Estrutura da Solution

