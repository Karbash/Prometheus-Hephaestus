using Hephaestus.Application.Base;
using Hephaestus.Application.Interfaces.WhatsApp;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Application.Interfaces.Category;
using Hephaestus.Application.Interfaces.Promotion;
using Hephaestus.Application.Interfaces.Order;
using Hephaestus.Application.Services;
using Hephaestus.Domain.DTOs.Response;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.WhatsApp;

/// <summary>
/// Pipeline de ações que executa códigos específicos usando dados dinâmicos
/// </summary>
public class ActionPipelineUseCase : BaseUseCase, IActionPipelineUseCase
{
    private readonly IGetCompaniesWithinRadiusUseCase _getCompaniesUseCase;
    private readonly IGlobalCategoryAdminUseCase _getGlobalCategoriesUseCase;
    private readonly IGetAllPromotionsAdminUseCase _getPromotionsUseCase;
    private readonly Hephaestus.Application.Interfaces.Order.IGlobalOrderAdminUseCase _getGlobalOrdersUseCase;
    private readonly IGlobalTagAdminUseCase _getGlobalTagsUseCase;
    private readonly IGlobalMenuItemAdminUseCase _getMenuItemsUseCase;
    private readonly IGlobalCouponAdminUseCase _getCouponsUseCase;
    private readonly ILogger<ActionPipelineUseCase> _logger;

    public ActionPipelineUseCase(
        IGetCompaniesWithinRadiusUseCase getCompaniesUseCase,
        IGlobalCategoryAdminUseCase getGlobalCategoriesUseCase,
        IGetAllPromotionsAdminUseCase getPromotionsUseCase,
        Hephaestus.Application.Interfaces.Order.IGlobalOrderAdminUseCase getGlobalOrdersUseCase,
        IGlobalTagAdminUseCase getGlobalTagsUseCase,
        IGlobalMenuItemAdminUseCase getMenuItemsUseCase,
        IGlobalCouponAdminUseCase getCouponsUseCase,
        ILogger<ActionPipelineUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _getCompaniesUseCase = getCompaniesUseCase;
        _getGlobalCategoriesUseCase = getGlobalCategoriesUseCase;
        _getPromotionsUseCase = getPromotionsUseCase;
        _getGlobalOrdersUseCase = getGlobalOrdersUseCase;
        _getGlobalTagsUseCase = getGlobalTagsUseCase;
        _getMenuItemsUseCase = getMenuItemsUseCase;
        _getCouponsUseCase = getCouponsUseCase;
        _logger = logger;
    }

    public async Task<WhatsAppResponse> ExecutePipelineAsync(List<int> codes, Dictionary<string, object>? data, string phoneNumber)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var response = new WhatsAppResponse();
            var messages = new List<string>();

            foreach (var code in codes)
            {
                var actionResult = await ExecuteActionAsync(code, data, phoneNumber);
                if (!string.IsNullOrEmpty(actionResult.Message))
                {
                    messages.Add(actionResult.Message);
                }
            }

            response.Message = string.Join("\n\n", messages);
            response.WaitForResponse = codes.Any(c => c == 1001 || c == 2001 || c == 4001); // Ações que precisam de resposta

            return response;
        }, "ActionPipeline");
    }

    private async Task<WhatsAppResponse> ExecuteActionAsync(int code, Dictionary<string, object>? data, string phoneNumber)
    {
        switch (code)
        {
            case 1001: // Buscar restaurantes próximos
                return await ExecuteSearchNearbyRestaurantsAsync(data);

            case 2001: // Buscar cardápio/menu
                return await ExecuteSearchMenuAsync(data);

            case 3001: // Verificar horários de funcionamento
                return await ExecuteCheckOperatingHoursAsync(data);

            case 4001: // Iniciar processo de pedido
                return await ExecuteStartOrderAsync(data);

            case 5001: // Buscar promoções/descontos
                return await ExecuteSearchPromotionsAsync(data);

            case 6001: // Buscar por tipo de culinária
                return await ExecuteSearchByCuisineAsync(data);

            case 7001: // Verificar status de pedido
                return await ExecuteCheckOrderStatusAsync(data, phoneNumber);

            case 8001: // Cancelar pedido
                return await ExecuteCancelOrderAsync(data, phoneNumber);

            case 9001: // Falar com atendente humano
                return await ExecuteHumanSupportAsync();

            case 5002: // Buscar cupons
                return await ExecuteSearchCouponsAsync(data);

            case 2002: // Buscar itens do menu por categoria
                return await ExecuteSearchMenuItemsAsync(data);

            case 6002: // Buscar restaurantes por tags/culinária
                return await ExecuteSearchRestaurantsByTagsAsync(data);

            default:
                return new WhatsAppResponse
                {
                    Message = "Ação não reconhecida. Por favor, tente novamente.",
                    WaitForResponse = true
                };
        }
    }

    private async Task<WhatsAppResponse> ExecuteSearchNearbyRestaurantsAsync(Dictionary<string, object>? data)
    {
        if (data == null || !data.ContainsKey("latitude") || !data.ContainsKey("longitude"))
        {
            return new WhatsAppResponse
            {
                Message = "Preciso da sua localização para encontrar restaurantes próximos. Por favor, compartilhe sua localização.",
                WaitForResponse = true
            };
        }

        try
        {
            var latitude = Convert.ToDouble(data["latitude"]);
            var longitude = Convert.ToDouble(data["longitude"]);
            var radius = 5.0; // 5km de raio

            var companies = await _getCompaniesUseCase.ExecuteAsync(latitude, longitude, radius);

            if (companies.Any())
            {
                var message = "*Restaurantes próximos encontrados:*\n\n";
                foreach (var company in companies.Take(5)) // Limita a 5 resultados
                {
                    message += $"*{company.Name}*\n";
                    message += $"📞 {company.PhoneNumber}\n";
                    if (company.Address != null)
                    {
                        message += $"📍 {company.Address.Street}, {company.Address.Number}\n";
                        message += $"🏙️ {company.Address.Neighborhood}, {company.Address.City}\n";
                    }
                    if (company.Slogan != null)
                    {
                        message += $"💬 {company.Slogan}\n";
                    }
                    message += "\n";
                }

                return new WhatsAppResponse
                {
                    Message = message,
                    Data = new Dictionary<string, object>
                    {
                        ["found_companies"] = companies.Select(c => c.Id).ToList()
                    },
                    WaitForResponse = true
                };
            }
            else
            {
                return new WhatsAppResponse
                {
                    Message = "Não encontrei restaurantes próximos à sua localização. Tente aumentar o raio de busca ou verificar se a localização está correta.",
                    WaitForResponse = true
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar restaurantes próximos");
            return new WhatsAppResponse
            {
                Message = "Desculpe, ocorreu um erro ao buscar restaurantes próximos. Tente novamente.",
                WaitForResponse = true
            };
        }
    }

    private async Task<WhatsAppResponse> ExecuteSearchMenuAsync(Dictionary<string, object>? data)
    {
        try
        {
            // Busca categorias globais do sistema
            var categories = await _getGlobalCategoriesUseCase.ExecuteAsync(
                isActive: true,
                pageNumber: 1,
                pageSize: 20,
                sortBy: "name",
                sortOrder: "asc"
            );

            if (categories.Items.Any())
            {
                var message = "🍽️ *Categorias disponíveis:*\n\n";
                foreach (var category in categories.Items.Take(10))
                {
                    message += $"• {category.Name}";
                    if (!string.IsNullOrEmpty(category.Description))
                    {
                        message += $" - {category.Description}";
                    }
                    message += "\n";
                }

                message += "\nDigite o nome da categoria que você quer ver o cardápio.";

                return new WhatsAppResponse
                {
                    Message = message,
                    Data = new Dictionary<string, object>
                    {
                        ["available_categories"] = categories.Items.Select(c => c.Id).ToList()
                    },
                    WaitForResponse = true
                };
            }
            else
            {
                return new WhatsAppResponse
                {
                    Message = "Desculpe, não há categorias disponíveis no momento. Tente novamente mais tarde.",
                    WaitForResponse = true
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar categorias");
            return new WhatsAppResponse
            {
                Message = "Desculpe, ocorreu um erro ao buscar o cardápio. Tente novamente.",
                WaitForResponse = true
            };
        }
    }

    private async Task<WhatsAppResponse> ExecuteCheckOperatingHoursAsync(Dictionary<string, object>? data)
    {
        try
        {
            // Busca restaurantes próximos para mostrar horários
            if (data?.ContainsKey("latitude") == true && data?.ContainsKey("longitude") == true)
            {
                var latitude = Convert.ToDouble(data["latitude"]);
                var longitude = Convert.ToDouble(data["longitude"]);
                var radius = 2.0; // 2km de raio

                var companies = await _getCompaniesUseCase.ExecuteAsync(latitude, longitude, radius);

                if (companies.Any())
                {
                    var message = "⏰ *Horários de funcionamento:*\n\n";
                    foreach (var company in companies.Take(3))
                    {
                        message += $"🏪 *{company.Name}*\n";
                        // Horários de funcionamento não estão disponíveis no CompanyResponse básico
                        message += "• Horários: Consulte diretamente o estabelecimento\n";
                        message += "\n";
                    }

                    return new WhatsAppResponse
                    {
                        Message = message,
                        WaitForResponse = false
                    };
                }
            }

            // Fallback para horários gerais
            return new WhatsAppResponse
            {
                Message = "⏰ *Horários de funcionamento:*\n\n" +
                         "🏪 Restaurantes geralmente funcionam:\n" +
                         "• Segunda a Sexta: 11h às 22h\n" +
                         "• Sábados e Domingos: 12h às 23h\n\n" +
                         "Horários podem variar por estabelecimento. Quer ver os horários de algum restaurante específico?",
                WaitForResponse = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar horários");
            return new WhatsAppResponse
            {
                Message = "Desculpe, ocorreu um erro ao verificar os horários. Tente novamente.",
                WaitForResponse = true
            };
        }
    }

    private async Task<WhatsAppResponse> ExecuteStartOrderAsync(Dictionary<string, object>? data)
    {
        return new WhatsAppResponse
        {
            Message = "🛒 *Vamos fazer seu pedido!*\n\n" +
                     "Primeiro, preciso saber qual restaurante você quer. " +
                     "Você já tem um em mente ou quer que eu mostre as opções próximas?",
            WaitForResponse = true
        };
    }

    private async Task<WhatsAppResponse> ExecuteSearchPromotionsAsync(Dictionary<string, object>? data)
    {
        try
        {
            // Busca promoções ativas do sistema
            var promotions = await _getPromotionsUseCase.ExecuteAsync(
                isActive: true,
                pageNumber: 1,
                pageSize: 10,
                sortBy: "enddate",
                sortOrder: "asc"
            );

            if (promotions.Items.Any())
            {
                var message = "*Promoções ativas:*\n\n";
                foreach (var promotion in promotions.Items.Take(5))
                {
                    message += $"*{promotion.Name}*\n";
                    if (!string.IsNullOrEmpty(promotion.Description))
                    {
                        message += $"📝 {promotion.Description}\n";
                    }
                    
                    // Formata o desconto baseado no tipo
                    if (promotion.DiscountType == Domain.Enum.DiscountType.Percentage)
                    {
                        message += $"💰 Desconto: {promotion.DiscountValue}%\n";
                    }
                    else
                    {
                        message += $"💰 Desconto: R$ {promotion.DiscountValue:F2}\n";
                    }
                    
                    if (promotion.MinOrderValue.HasValue)
                    {
                        message += $"💳 Pedido mínimo: R$ {promotion.MinOrderValue:F2}\n";
                    }
                    
                    if (promotion.EndDate != default(DateTime))
                    {
                        message += $"⏰ Válida até: {promotion.EndDate:dd/MM/yyyy}\n";
                    }
                    message += "\n";
                }

                return new WhatsAppResponse
                {
                    Message = message,
                    Data = new Dictionary<string, object>
                    {
                        ["available_promotions"] = promotions.Items.Select(p => p.Id).ToList()
                    },
                    WaitForResponse = false
                };
            }
            else
            {
                return new WhatsAppResponse
                {
                    Message = "Não há promoções ativas no momento. Mas não se preocupe, sempre temos ótimas ofertas!",
                    WaitForResponse = false
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar promoções");
            return new WhatsAppResponse
            {
                Message = "Desculpe, ocorreu um erro ao buscar promoções. Tente novamente.",
                WaitForResponse = false
            };
        }
    }

    private async Task<WhatsAppResponse> ExecuteSearchByCuisineAsync(Dictionary<string, object>? data)
    {
        try
        {
            // Busca tags globais que representam tipos de culinária
            var tags = await _getGlobalTagsUseCase.ExecuteAsync(
                name: null,
                pageNumber: 1,
                pageSize: 20,
                sortBy: "name",
                sortOrder: "asc"
            );

            if (tags.Items.Any())
            {
                var message = "🍕 *Tipos de culinária disponíveis:*\n\n";

                foreach (var tag in tags.Items)
                {
                    var emoji = GetCuisineEmoji(tag.Name);
                    message += $"• {emoji} {tag.Name}";
                    if (!string.IsNullOrEmpty(tag.Description))
                    {
                        message += $" - {tag.Description}";
                    }
                    message += "\n";
                }

                message += "\nQual tipo de comida você prefere?";

                return new WhatsAppResponse
                {
                    Message = message,
                    Data = new Dictionary<string, object>
                    {
                        ["available_tags"] = tags.Items.Select(t => t.Id).ToList()
                    },
                    WaitForResponse = true
                };
            }
            else
            {
                // Fallback para lista estática se não houver tags no banco
                var message = "🍕 *Tipos de culinária disponíveis:*\n\n";
                var cuisineTypes = new[]
                {
                    "🍕 Italiana",
                    "🍜 Japonesa",
                    "🥘 Brasileira",
                    "🌮 Mexicana",
                    "🍔 Americana",
                    "🥙 Árabe",
                    "🍛 Indiana",
                    "🥡 Chinesa",
                    "🍣 Sushi",
                    "🍖 Churrasco",
                    "🥗 Vegetariana",
                    "🌱 Vegana"
                };

                foreach (var cuisine in cuisineTypes)
                {
                    message += $"• {cuisine}\n";
                }

                message += "\nQual tipo de comida você prefere?";

                return new WhatsAppResponse
                {
                    Message = message,
                    WaitForResponse = true
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar tipos de culinária");
            return new WhatsAppResponse
            {
                Message = "Desculpe, ocorreu um erro ao buscar tipos de culinária. Tente novamente.",
                WaitForResponse = true
            };
        }
    }

    private async Task<WhatsAppResponse> ExecuteSearchCouponsAsync(Dictionary<string, object>? data)
    {
        try
        {
            // Busca cupons ativos do sistema
            var coupons = await _getCouponsUseCase.ExecuteAsync(
                isActive: true,
                pageNumber: 1,
                pageSize: 10,
                sortBy: "endDate",
                sortOrder: "asc"
            );

            if (coupons.Items.Any())
            {
                var message = "🎫 *Cupons disponíveis:*\n\n";
                foreach (var coupon in coupons.Items.Take(5))
                {
                    message += $"🎫 *{coupon.Code}*\n";
                    
                    // Formata o desconto baseado no tipo
                    if (coupon.DiscountType == Domain.Enum.DiscountType.Percentage)
                    {
                        message += $"💰 Desconto: {coupon.DiscountValue}%\n";
                    }
                    else
                    {
                        message += $"💰 Desconto: R$ {coupon.DiscountValue:F2}\n";
                    }
                    
                    if (coupon.MinOrderValue > 0)
                    {
                        message += $"💳 Pedido mínimo: R$ {coupon.MinOrderValue:F2}\n";
                    }
                    
                    if (coupon.EndDate != default(DateTime))
                    {
                        message += $"⏰ Válido até: {coupon.EndDate:dd/MM/yyyy}\n";
                    }
                    message += "\n";
                }

                return new WhatsAppResponse
                {
                    Message = message,
                    Data = new Dictionary<string, object>
                    {
                        ["available_coupons"] = coupons.Items.Select(c => c.Id).ToList()
                    },
                    WaitForResponse = false
                };
            }
            else
            {
                return new WhatsAppResponse
                {
                    Message = "Não há cupons disponíveis no momento. Mas não se preocupe, sempre temos ótimas ofertas!",
                    WaitForResponse = false
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cupons");
            return new WhatsAppResponse
            {
                Message = "Desculpe, ocorreu um erro ao buscar cupons. Tente novamente.",
                WaitForResponse = false
            };
        }
    }

    private async Task<WhatsAppResponse> ExecuteSearchMenuItemsAsync(Dictionary<string, object>? data)
    {
        try
        {
            // Extrai categoria dos dados ou usa padrão
            string? categoryId = null;
            if (data?.ContainsKey("category_id") == true)
            {
                categoryId = data["category_id"].ToString();
            }

            // Busca itens do menu
            var categoryIds = !string.IsNullOrEmpty(categoryId) ? new List<string> { categoryId } : null;
            var menuItems = await _getMenuItemsUseCase.ExecuteAsync(
                categoryIds: categoryIds,
                isAvailable: true,
                pageNumber: 1,
                pageSize: 15,
                sortBy: "name",
                sortOrder: "asc"
            );

            if (menuItems.Items.Any())
            {
                var message = "🍽️ *Cardápio:*\n\n";
                foreach (var item in menuItems.Items)
                {
                    message += $"🍽️ *{item.Name}*\n";
                    if (!string.IsNullOrEmpty(item.Description))
                    {
                        message += $"📝 {item.Description}\n";
                    }
                    message += $"💰 R$ {item.Price:F2}\n";
                    
                    if (item.Tags != null && item.Tags.Any())
                    {
                        var tagNames = item.Tags.Select(t => t.Name).ToList();
                        message += $"🏷️ {string.Join(", ", tagNames)}\n";
                    }
                    message += "\n";
                }

                return new WhatsAppResponse
                {
                    Message = message,
                    Data = new Dictionary<string, object>
                    {
                        ["available_menu_items"] = menuItems.Items.Select(i => i.Id).ToList()
                    },
                    WaitForResponse = true
                };
            }
            else
            {
                return new WhatsAppResponse
                {
                    Message = "Não há itens disponíveis no cardápio no momento. Tente novamente mais tarde.",
                    WaitForResponse = true
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar itens do menu");
            return new WhatsAppResponse
            {
                Message = "Desculpe, ocorreu um erro ao buscar o cardápio. Tente novamente.",
                WaitForResponse = true
            };
        }
    }

    private async Task<WhatsAppResponse> ExecuteSearchRestaurantsByTagsAsync(Dictionary<string, object>? data)
    {
        try
        {
            // Extrai tag dos dados
            string? tagName = null;
            if (data?.ContainsKey("tag_name") == true)
            {
                tagName = data["tag_name"].ToString();
            }

            if (string.IsNullOrEmpty(tagName))
            {
                return new WhatsAppResponse
                {
                    Message = "Por favor, especifique qual tipo de culinária você está procurando.",
                    WaitForResponse = true
                };
            }

            // Busca restaurantes que têm essa tag
            // Por enquanto, retorna uma mensagem informativa
            // TODO: Implementar busca real por restaurantes com tags específicas
            
            return new WhatsAppResponse
            {
                Message = $"🍽️ *Restaurantes com culinária {tagName}:*\n\n" +
                         $"Estamos buscando restaurantes que servem {tagName}.\n" +
                         $"Para encontrar restaurantes específicos, use a busca por localização.",
                WaitForResponse = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar restaurantes por tags");
            return new WhatsAppResponse
            {
                Message = "Desculpe, ocorreu um erro ao buscar restaurantes. Tente novamente.",
                WaitForResponse = true
            };
        }
    }

    private string GetCuisineEmoji(string cuisineName)
    {
        var lowerName = cuisineName.ToLower();
        return lowerName switch
        {
            "italiana" => "🍕",
            "japonesa" => "🍜",
            "brasileira" => "🥘",
            "mexicana" => "🌮",
            "americana" => "🍔",
            "árabe" => "🥙",
            "indiana" => "🍛",
            "chinesa" => "🥡",
            "sushi" => "🍣",
            "churrasco" => "🍖",
            "vegetariana" => "🥗",
            "vegana" => "🌱",
            "pizza" => "🍕",
            "hambúrguer" => "🍔",
            "sashimi" => "🍣",
            "temaki" => "🍣",
            "sobremesa" => "🍰",
            "bebida" => "🥤",
            "café" => "☕",
            _ => "🍽️"
        };
    }

    private async Task<WhatsAppResponse> ExecuteCheckOrderStatusAsync(Dictionary<string, object>? data, string phoneNumber)
    {
        try
        {
            // Busca pedidos do cliente pelo telefone
            var orders = await _getGlobalOrdersUseCase.ExecuteAsync(
                customerPhoneNumber: phoneNumber,
                pageNumber: 1,
                pageSize: 5,
                sortBy: "createdAt",
                sortOrder: "desc"
            );

            if (orders.Items.Any())
            {
                var message = "📋 *Seus pedidos recentes:*\n\n";
                foreach (var order in orders.Items)
                {
                    message += $"🆔 *Pedido #{order.Id.Substring(0, 8)}*\n";
                    message += $"📅 {order.CreatedAt:dd/MM/yyyy HH:mm}\n";
                    message += $"💰 R$ {order.TotalAmount:F2}\n";
                    message += $"📊 Status: {GetStatusEmoji(order.Status)} {order.Status}\n";
                    message += $"💳 Pagamento: {GetPaymentStatusEmoji(order.PaymentStatus)} {order.PaymentStatus}\n";
                    message += "\n";
                }

                return new WhatsAppResponse
                {
                    Message = message,
                    Data = new Dictionary<string, object>
                    {
                        ["recent_orders"] = orders.Items.Select(o => o.Id).ToList()
                    },
                    WaitForResponse = true
                };
            }
            else
            {
                return new WhatsAppResponse
                {
                    Message = "📋 Não encontrei pedidos para o número " + phoneNumber + ". " +
                             "Você tem certeza que este é o número correto ou quer fazer um novo pedido?",
                    WaitForResponse = true
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar status do pedido");
            return new WhatsAppResponse
            {
                Message = "Desculpe, ocorreu um erro ao verificar o status do seu pedido. Tente novamente.",
                WaitForResponse = true
            };
        }
    }

    private async Task<WhatsAppResponse> ExecuteCancelOrderAsync(Dictionary<string, object>? data, string phoneNumber)
    {
        try
        {
            // Busca pedidos pendentes do cliente
            var orders = await _getGlobalOrdersUseCase.ExecuteAsync(
                customerPhoneNumber: phoneNumber,
                status: "Pending",
                pageNumber: 1,
                pageSize: 3,
                sortBy: "createdAt",
                sortOrder: "desc"
            );

            if (orders.Items.Any())
            {
                var message = "❌ *Pedidos que podem ser cancelados:*\n\n";
                foreach (var order in orders.Items)
                {
                    message += $"🆔 *Pedido #{order.Id.Substring(0, 8)}*\n";
                    message += $"📅 {order.CreatedAt:dd/MM/yyyy HH:mm}\n";
                    message += $"💰 R$ {order.TotalAmount:F2}\n";
                    message += $"📊 Status: {order.Status}\n\n";
                }

                message += "Digite o número do pedido que deseja cancelar:";

                return new WhatsAppResponse
                {
                    Message = message,
                    Data = new Dictionary<string, object>
                    {
                        ["cancelable_orders"] = orders.Items.Select(o => o.Id).ToList()
                    },
                    WaitForResponse = true
                };
            }
            else
            {
                return new WhatsAppResponse
                {
                    Message = "❌ Não há pedidos pendentes para cancelar no número " + phoneNumber + ".",
                    WaitForResponse = false
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pedidos para cancelamento");
            return new WhatsAppResponse
            {
                Message = "Desculpe, ocorreu um erro ao buscar seus pedidos. Tente novamente.",
                WaitForResponse = true
            };
        }
    }

    private async Task<WhatsAppResponse> ExecuteHumanSupportAsync()
    {
        return new WhatsAppResponse
        {
            Message = "👨‍💼 *Atendimento Humano*\n\n" +
                     "Estou transferindo você para um atendente humano. " +
                     "Aguarde um momento, você será atendido em breve.",
            WaitForResponse = false
        };
    }

    private string GetDayName(int dayOfWeek)
    {
        return dayOfWeek switch
        {
            1 => "Segunda-feira",
            2 => "Terça-feira",
            3 => "Quarta-feira",
            4 => "Quinta-feira",
            5 => "Sexta-feira",
            6 => "Sábado",
            7 => "Domingo",
            _ => "Dia " + dayOfWeek
        };
    }

    private string GetStatusEmoji(Domain.Enum.OrderStatus status)
    {
        return status switch
        {
            Domain.Enum.OrderStatus.Pending => "⏳",
            Domain.Enum.OrderStatus.InProduction => "👨‍🍳",
            Domain.Enum.OrderStatus.Completed => "✅",
            Domain.Enum.OrderStatus.Cancelled => "❌",
            _ => "📋"
        };
    }

    private string GetPaymentStatusEmoji(Domain.Enum.PaymentStatus paymentStatus)
    {
        return paymentStatus switch
        {
            Domain.Enum.PaymentStatus.Pending => "⏳",
            Domain.Enum.PaymentStatus.Paid => "✅",
            Domain.Enum.PaymentStatus.Failed => "❌",
            Domain.Enum.PaymentStatus.Refunded => "↩️",
            _ => "💳"
        };
    }
} 