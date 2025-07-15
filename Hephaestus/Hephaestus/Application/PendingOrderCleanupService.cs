using Hephaestus.Application.UseCases.Order;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hephaestus.Application;

/// <summary>
/// Serviço em background para exclusão automática de pedidos Pending não pagos após X minutos.
/// </summary>
public class PendingOrderCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PendingOrderCleanupService> _logger;
    private readonly int _minutes = 30; // tempo de expiração dos pedidos Pending
    private readonly int _intervalMinutes = 5; // intervalo de execução do job

    public PendingOrderCleanupService(IServiceProvider serviceProvider, ILogger<PendingOrderCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var useCase = scope.ServiceProvider.GetRequiredService<DeleteOldPendingOrdersUseCase>();
                    await useCase.ExecuteAsync(_minutes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar limpeza automática de pedidos Pending.");
            }
            await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
        }
    }
} 