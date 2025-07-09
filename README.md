# 🔥⚒️ Prometheus & Hephaestus

**Prometheus & Hephaestus** é um sistema robusto de **gestão de pedidos multi-tenant**, **offline-first**, inspirado em plataformas como iFood. Forjado para oferecer funcionalidades contínuas para empresas e clientes, mesmo sem conexão com a internet.

> *“Forjado na bigorna de Hefesto. Iluminado pelo fogo de Prometeu.”*

---

## 📚 Visão Geral

Este projeto é composto por **dois componentes complementares**:

| Componente | Papel | Descrição |
|------------|-------|-----------|
| **Prometheus** | Frontend PWA | A interface flamejante — PWA Angular com cache offline, IndexedDB e sincronização. |
| **Hephaestus** | Backend API | A bigorna resistente — API RESTful em .NET com PostgreSQL, gestão de pedidos multi-tenant, cardápios e análises. |

---

## ⚙️ Arquitetura

```
Usuário ⇄ Prometheus (Angular PWA)
           ⇄ IndexedDB (Banco local offline)
           ⇄ Sincronização ⇄ Hephaestus (Backend API)
                                ⇄ PostgreSQL (Banco central)
```

- **Offline-First**: Prometheus armazena dados localmente com IndexedDB, garantindo funcionamento sem internet.
- **Sincronização**: Quando a conexão volta, Prometheus sincroniza com Hephaestus, resolvendo conflitos com merge versionado.
- **Multi-Tenant**: Cada empresa possui dados isolados via TenantId, com tabelas específicas (Pedidos, Cardápios) e tabelas globais (Empresas, Clientes).
- **Escalável**: Hephaestus gerencia pedidos, auditoria e relatórios, com **Redis** para cache e **RabbitMQ** para processamento assíncrono.

---

## 🚀 Tecnologias

| Camada | Tecnologias |
|--------|--------------|
| **Frontend** | Angular PWA, Service Workers, IndexedDB, Dexie.js |
| **Backend** | .NET 8, ASP.NET Core, EF Core, Npgsql |
| **Banco de Dados** | PostgreSQL |
| **Sincronização** | Fila offline, timestamps, resolução de conflitos |
| **Mensageria** | RabbitMQ (processamento assíncrono de WhatsApp) |
| **Cache** | Redis (cache de cardápios e promoções) |
| **Segurança** | JWT, BCrypt, chaves de API criptografadas |

---

## ✨ Funcionalidades Principais

✅ **Offline-First**: Operações CRUD offline com sincronização em fila  
✅ **Multi-Tenant**: Dados isolados por empresa, usando TenantId e JWT  
✅ **PWA**: Aplicativo Progressivo com suporte offline (Service Workers)  
✅ **Gestão de Pedidos**: Pedidos via API WhatsApp, rastreamento em tempo real  
✅ **Cardápio & Promoções**: Gerenciamento dinâmico de cardápio, promoções avançadas e cupons  
✅ **Análises**: Dashboards detalhados para empresas e admins  
✅ **Segurança**: Autenticação JWT, MFA para admins, rate limiting  
✅ **Escalabilidade**: Backend desacoplado com mensageria assíncrona

---

## 🗡️ Manifesto

> *“Prometeu desafia limites, trazendo o fogo da tecnologia aos usuários.  
> Hefesto forja esse fogo em sistemas indestrutíveis, resilientes até na ausência de conectividade.”*

---

## 🏗️ Primeiros Passos

### ✅ Pré-Requisitos

- Node.js (v16+)
- Angular CLI (v16+)
- .NET SDK (8.0)
- PostgreSQL (v15+)
- Redis (opcional, para cache)
- RabbitMQ (opcional, para mensageria assíncrona)

---

### 🔥 Prometheus (Frontend PWA)

```bash
cd prometheus
npm install
ng serve
```

Para produção (PWA):
```bash
ng build --prod
# Deploy em servidor HTTPS (Service Worker exige HTTPS)
```

---

### ⚒️ Hephaestus (Backend API)

1. Configure a string de conexão no `hephaestus/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "Postgres": "Host=localhost;Database=hephaestus;Username=postgres;Password=sua_senha_segura"
     },
     "Jwt": {
       "Key": "sua-chave-super-segura-32-caracteres",
       "Issuer": "HephaestusAPI",
       "Audience": "HephaestusClient"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning",
         "Microsoft.EntityFrameworkCore": "Debug"
       }
     }
   }
   ```

2. Aplique as migrations e execute:
   ```bash
   cd hephaestus
   dotnet restore
   dotnet ef migrations add InitPostgres
   dotnet ef database update
   dotnet run
   ```

---

## 📜 Endpoints Principais

A API Hephaestus é RESTful e utiliza JWT. Endpoints de WhatsApp e avaliações podem usar chaves de API e rate limiting.

Exemplos:

- `POST /api/auth/login` — Autentica e retorna JWT
- `POST /api/auth/register` — Registra nova empresa (Admin)
- `GET /api/company` — Lista empresas (Admin)
- `GET /api/menu` — Lista itens do cardápio (Tenant)
- `POST /api/whatsapp/order` — Cria pedido via WhatsApp (API Key)

Para lista completa, consulte a **Documentação do Projeto**.

---

## 🗄️ Esquema do Banco

O banco PostgreSQL usa tabelas específicas por tenant (`Orders`, `MenuItems`) com `TenantId` e tabelas globais (`Companies`, `Customers`, `AuditLogs`).  
Principais entidades:
- **Company**
- **Order & OrderItem**
- **Customization**
- **Promotion & Coupon**
- **AuditLog**

---

## 🔍 Dicas de Debug

- Ative `Microsoft.EntityFrameworkCore: Debug` para log detalhado das queries.
- Use `\dt` no psql para verificar tabelas.
- Teste `SELECT * FROM "Companies";` para validar inserts.
- Verifique `ApiKey` para integração WhatsApp.

---

## 🛠️ Contribuindo

Contribuições são bem-vindas!

```bash
# Clone e crie sua branch:
git checkout -b feature/sua-feature

# Commit
git commit -m "Add nova feature"

# Push
git push origin feature/sua-feature

# Abra um Pull Request 🚀
```

---

## 📜 Licença

MIT © [Seu Nome ou Organização]

> 🔥 **Forjado para funcionar. Sempre. Mesmo offline.**