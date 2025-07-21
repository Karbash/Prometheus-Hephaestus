# 🚀 Guia Completo - WhatsApp API

## 📋 **Visão Geral**

O sistema WhatsApp agora está **100% integrado** com todos os repositórios disponíveis:

### ✅ **Funcionalidades Implementadas:**

| **Funcionalidade** | **Status** | **Código** | **Descrição** |
|-------------------|------------|------------|---------------|
| **Empresas/Restaurantes** | ✅ **Ativo** | 1001 | Busca por localização |
| **Categorias** | ✅ **Ativo** | 2001 | Tipos de comida |
| **Menu Items** | ✅ **Ativo** | 2002 | Pratos específicos |
| **Promoções** | ✅ **Ativo** | 5001 | Descontos e ofertas |
| **Cupons** | ✅ **Ativo** | 5002 | Cupons de desconto |
| **Tags/Culinária** | ✅ **Ativo** | 6001 | Tipos de culinária |
| **Pedidos** | ✅ **Ativo** | 7001 | Status e histórico |
| **Horários** | ✅ **Ativo** | 3001 | Funcionamento |
| **Atendente** | ✅ **Ativo** | 9001 | Suporte humano |

---

## 🎯 **Endpoints Disponíveis**

### **1. Processar Mensagem**
```
POST /api/whatsapp/process
```
**Função:** Processa mensagens do WhatsApp e retorna respostas inteligentes.

### **2. Webhook WhatsApp**
```
POST /api/whatsapp/webhook
```
**Função:** Recebe mensagens do WhatsApp Business API.

### **3. Verificação Webhook**
```
GET /api/whatsapp/webhook
```
**Função:** Verificação do webhook pelo WhatsApp.

---

## 🔧 **Como Funciona**

### **Fluxo de Processamento:**

1. **Recebe mensagem** via `/api/whatsapp/process`
2. **Classifica intenção** usando regras simples + OpenAI
3. **Executa ações** baseadas nos códigos retornados
4. **Retorna resposta** formatada para WhatsApp

### **Códigos de Ação:**

| **Código** | **Ação** | **Dados Necessários** |
|------------|----------|----------------------|
| 1001 | Buscar restaurantes | latitude, longitude |
| 2001 | Buscar categorias | - |
| 2002 | Buscar menu items | category_id (opcional) |
| 3001 | Verificar horários | - |
| 4001 | Iniciar pedido | - |
| 5001 | Buscar promoções | - |
| 5002 | Buscar cupons | - |
| 6001 | Buscar culinária | - |
| 6002 | Buscar por tags | tag_name |
| 7001 | Status pedido | phoneNumber |
| 8001 | Cancelar pedido | phoneNumber |
| 9001 | Atendente humano | - |

---

## 📝 **Exemplos de Uso**

### **Exemplo 1: Buscar Tipos de Culinária**
```json
POST /api/whatsapp/process
{
  "message": "Quais tipos de comida vocês têm?",
  "phoneNumber": "5511999999999",
  "conversationId": "conv_123"
}
```

**Resposta:**
```json
{
  "message": "🍕 Tipos de culinária disponíveis:\n\n• 🍕 Italiana\n• 🍜 Japonesa\n• 🥘 Brasileira\n...",
  "codes": [6001],
  "waitForResponse": true,
  "data": {
    "available_tags": ["tag1", "tag2", "tag3"]
  }
}
```

### **Exemplo 2: Buscar Cupons**
```json
POST /api/whatsapp/process
{
  "message": "Têm cupons de desconto?",
  "phoneNumber": "5511999999999",
  "conversationId": "conv_123"
}
```

**Resposta:**
```json
{
  "message": "🎫 Cupons disponíveis:\n\n🎫 CUPOM10\n📝 10% de desconto\n💰 Desconto: 10%\n💳 Pedido mínimo: R$ 30,00\n⏰ Válido até: 31/12/2024",
  "codes": [5002],
  "waitForResponse": false
}
```

### **Exemplo 3: Buscar Menu Items**
```json
POST /api/whatsapp/process
{
  "message": "Quero ver o cardápio de pizzas",
  "phoneNumber": "5511999999999",
  "conversationId": "conv_123"
}
```

**Resposta:**
```json
{
  "message": "🍽️ Cardápio:\n\n🍽️ Pizza Margherita\n📝 Molho de tomate, mussarela, manjericão\n💰 R$ 35,00\n🏷️ Italiana, Vegetariana",
  "codes": [2002],
  "waitForResponse": true
}
```

---

## 🗄️ **Integração com Banco de Dados**

### **Repositórios Utilizados:**

| **Repositório** | **UseCase** | **Funcionalidade** |
|-----------------|-------------|-------------------|
| `CompanyRepository` | `IGetCompaniesWithinRadiusUseCase` | Restaurantes próximos |
| `CategoryRepository` | `IGlobalCategoryAdminUseCase` | Categorias de comida |
| `MenuItemRepository` | `IGetMenuItemsUseCase` | Itens do cardápio |
| `PromotionRepository` | `IGetAllPromotionsAdminUseCase` | Promoções |
| `CouponRepository` | `IGetCouponsUseCase` | Cupons |
| `TagRepository` | `IGlobalTagAdminUseCase` | Tipos de culinária |
| `OrderRepository` | `IGlobalOrderAdminUseCase` | Pedidos |

### **Dados Retornados:**

- **Tags:** Nome, descrição, emoji
- **Menu Items:** Nome, descrição, preço, tags
- **Cupons:** Nome, desconto, valor mínimo, validade
- **Promoções:** Nome, desconto, descrição
- **Restaurantes:** Nome, endereço, distância
- **Pedidos:** ID, status, valor, data

---

## 🧠 **Inteligência Artificial**

### **Classificação de Intenções:**

1. **Regras Simples:** Palavras-chave e padrões
2. **OpenAI Fallback:** Para casos complexos
3. **Contexto:** Mantém conversa fluida

### **Exemplo de Prompt OpenAI:**
```
Você é um assistente de WhatsApp para um sistema de delivery de comida.

Analise a mensagem e retorne:
- message: resposta direta
- codes: códigos de ações
- wait_for_response: se aguarda resposta
- conversation_context: contexto

CÓDIGOS: 1001, 2001, 2002, 3001, 4001, 5001, 5002, 6001, 6002, 7001, 8001, 9001
```

---

## 🔄 **Pipeline de Ações**

### **Execução Sequencial:**

1. **Recebe códigos** do classificador
2. **Executa ações** em paralelo
3. **Combina respostas** em uma mensagem
4. **Retorna resultado** formatado

### **Exemplo de Pipeline:**
```csharp
// Códigos: [1001, 5001]
// Executa: Buscar restaurantes + Buscar promoções
// Retorna: Lista de restaurantes + Promoções disponíveis
```

---

## 🛠️ **Configuração**

### **Dependências Registradas:**

```csharp
// Application Layer
services.AddScoped<IGlobalTagAdminUseCase, GlobalTagAdminUseCase>();
services.AddScoped<IGetAllTagsByTenantUseCase, GetAllTagsByTenantUseCase>();
services.AddScoped<IGetMenuItemsUseCase, GetMenuItemsUseCase>();
services.AddScoped<IGetCouponsUseCase, GetCouponsUseCase>();

// Infrastructure Layer
services.AddScoped<ITagRepository, TagRepository>();
services.AddScoped<IMenuItemRepository, MenuItemRepository>();
services.AddScoped<ICouponRepository, CouponRepository>();
```

---

## 📊 **Métricas e Logs**

### **Logs Gerados:**

- **Classificação:** Intenções identificadas
- **Execução:** Ações realizadas
- **Erros:** Exceções e fallbacks
- **Performance:** Tempo de resposta

### **Exemplo de Log:**
```
[INFO] WhatsApp: Classified intent as "search_restaurants" (code: 1001)
[INFO] WhatsApp: Executed action "ExecuteSearchNearbyRestaurantsAsync"
[INFO] WhatsApp: Found 5 restaurants within 2km radius
```

---

## 🚀 **Próximos Passos**

### **Melhorias Futuras:**

1. **Busca por Tags:** Restaurantes com culinária específica
2. **Filtros Avançados:** Preço, avaliação, horário
3. **Recomendações:** Baseadas no histórico
4. **Integração Pagamento:** Processar pagamentos
5. **Notificações:** Status de pedidos em tempo real

---

## 📞 **Suporte**

### **Testes:**

1. **REST Client:** `exemplos-whatsapp-process-completo.http`
2. **cURL:** Scripts automatizados
3. **Swagger:** Documentação interativa

### **Debug:**

1. **Logs:** Verificar execução
2. **Banco:** Confirmar dados
3. **OpenAI:** Testar classificação

---

## ✅ **Status Final**

### **Integração Completa:**
- ✅ **Tags** (tipos de culinária)
- ✅ **Menu Items** (pratos específicos)
- ✅ **Cupons** (descontos)
- ✅ **Promoções** (ofertas)
- ✅ **Restaurantes** (localização)
- ✅ **Pedidos** (status e histórico)
- ✅ **Categorias** (tipos de comida)

### **Sistema 100% Funcional:**
O WhatsApp agora aproveita **100% do potencial** dos repositórios disponíveis! 🎉 