using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Interfaces.WhatsApp;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para integração com WhatsApp Business API
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class WhatsAppController : ControllerBase
{
    private readonly IProcessWhatsAppMessageUseCase _processMessageUseCase;
    private readonly ILogger<WhatsAppController> _logger;

    public WhatsAppController(
        IProcessWhatsAppMessageUseCase processMessageUseCase,
        ILogger<WhatsAppController> logger)
    {
        _processMessageUseCase = processMessageUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Processa mensagens de chat de forma inteligente
    /// </summary>
    /// <remarks>
    /// Este endpoint processa mensagens de chat de forma inteligente usando IA e dados dinâmicos do sistema.
    /// Pode ser usado por qualquer aplicação que precise de processamento inteligente de mensagens.
    /// 
    /// **Exemplo de requisição:**
    /// ```json
    /// {
    ///   "phoneNumber": "5511999999999",
    ///   "message": "Preciso saber onde há restaurantes perto de onde estou",
    ///   "contextData": {
    ///     "latitude": -23.5505,
    ///     "longitude": -46.6333
    ///   },
    ///   "sessionId": "session_123"
    /// }
    /// ```
    /// 
    /// **Exemplo de resposta:**
    /// ```json
    /// {
    ///   "message": "🏪 Restaurantes próximos encontrados:\n\n📍 Restaurante Italiano\n📞 (11) 9999-9999\n📍 Rua das Flores, 123\n🏙️ Centro, São Paulo",
    ///   "waitForResponse": true,
    ///   "data": {
    ///     "found_companies": ["company_id_1", "company_id_2"]
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados da mensagem</param>
    /// <returns>Resposta processada</returns>
    [HttpPost("process")]
    [SwaggerOperation(
        Summary = "Processar mensagem",
        Description = "Processa mensagens de chat de forma inteligente usando IA e dados dinâmicos")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WhatsAppResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessMessage([FromBody] ProcessMessageRequest request)
    {
        try
        {
            _logger.LogInformation("Processando mensagem para: {PhoneNumber}", request.PhoneNumber);

            // Converte para o formato interno
            var whatsAppRequest = new WhatsAppMessageRequest
            {
                From = request.PhoneNumber,
                Text = request.Message,
                Type = "text",
                Timestamp = DateTime.UtcNow,
                ConversationId = request.SessionId,
                ContextData = request.ContextData
            };
            
            var response = await _processMessageUseCase.ExecuteAsync(whatsAppRequest);

            _logger.LogInformation("Resposta processada para {PhoneNumber}: {Message}", request.PhoneNumber, response.Message);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem para: {PhoneNumber}", request?.PhoneNumber);
            
            // Retorna resposta de erro amigável
            var errorResponse = new WhatsAppResponse
            {
                Message = "Desculpe, ocorreu um erro ao processar sua mensagem. Tente novamente em alguns instantes.",
                WaitForResponse = true
            };

            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// Recebe mensagens do WhatsApp Business API
    /// </summary>
    /// <remarks>
    /// Este endpoint recebe mensagens do WhatsApp Business API e processa automaticamente
    /// usando inteligência artificial para classificar intenções e executar ações apropriadas.
    /// 
    /// **Exemplo de mensagem de texto:**
    /// ```json
    /// {
    ///   "messageId": "wamid.123456789",
    ///   "from": "5511999999999",
    ///   "to": "5511888888888",
    ///   "type": "text",
    ///   "text": "Preciso saber onde há restaurantes perto de onde estou",
    ///   "timestamp": "2024-01-20T10:30:00Z"
    /// }
    /// ```
    /// 
    /// **Exemplo de mensagem de localização:**
    /// ```json
    /// {
    ///   "messageId": "wamid.123456790",
    ///   "from": "5511999999999",
    ///   "to": "5511888888888",
    ///   "type": "location",
    ///   "latitude": -23.5505,
    ///   "longitude": -46.6333,
    ///   "timestamp": "2024-01-20T10:31:00Z"
    /// }
    /// ```
    /// 
    /// **Exemplo de resposta:**
    /// ```json
    /// {
    ///   "message": "🏪 Restaurantes próximos encontrados:\n\n📍 Restaurante Italiano\n📞 (11) 9999-9999\n📍 Rua das Flores, 123\n⏰ 11:00 - 22:00",
    ///   "codes": [1001],
    ///   "waitForResponse": true,
    ///   "conversationContext": "restaurants_found"
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados da mensagem do WhatsApp</param>
    /// <returns>Resposta processada para enviar ao usuário</returns>
    [HttpPost("webhook")]
    [SwaggerOperation(
        Summary = "Webhook do WhatsApp",
        Description = "Recebe mensagens do WhatsApp Business API e processa automaticamente usando IA")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WhatsAppResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Webhook([FromBody] WhatsAppMessageRequest request)
    {
        try
        {
            _logger.LogInformation("Recebida mensagem do WhatsApp: {MessageId} de {From}", request.MessageId, request.From);

            var response = await _processMessageUseCase.ExecuteAsync(request);

            _logger.LogInformation("Resposta processada para {From}: {Message}", request.From, response.Message);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem do WhatsApp: {MessageId}", request?.MessageId);
            
            // Retorna resposta de erro amigável
            var errorResponse = new WhatsAppResponse
            {
                Message = "Desculpe, ocorreu um erro ao processar sua mensagem. Tente novamente em alguns instantes.",
                WaitForResponse = true
            };

            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// Endpoint de verificação para configuração do webhook do WhatsApp
    /// </summary>
    /// <param name="hubMode">Modo do hub</param>
    /// <param name="hubVerifyToken">Token de verificação</param>
    /// <param name="hubChallenge">Desafio do hub</param>
    /// <returns>Desafio do hub para verificação</returns>
    [HttpGet("webhook")]
    [SwaggerOperation(
        Summary = "Verificação do Webhook",
        Description = "Endpoint para verificação do webhook do WhatsApp Business API")]
    public IActionResult VerifyWebhook(
        [FromQuery] string hubMode,
        [FromQuery] string hubVerifyToken,
        [FromQuery] string hubChallenge)
    {
        // Verifica se o token é válido (configure no appsettings.json)
        var expectedToken = "seu_token_de_verificacao"; // TODO: Mover para configuração

        if (hubMode == "subscribe" && hubVerifyToken == expectedToken)
        {
            _logger.LogInformation("Webhook do WhatsApp verificado com sucesso");
            return Ok(hubChallenge);
        }

        _logger.LogWarning("Tentativa de verificação do webhook com token inválido");
        return BadRequest("Token de verificação inválido");
    }
} 