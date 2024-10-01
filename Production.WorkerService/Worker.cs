using Production.WorkerService.Publisher;
using System.Collections.Concurrent;

namespace Production.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly string streamName;
        private readonly string groupName;

        private ConcurrentDictionary<string, WorkOrder> _workOrders = new ConcurrentDictionary<string, WorkOrder>();
        public Worker(ILogger<Worker> logger, IConfiguration config, IConnectionMultiplexer connectionMultiplexer)
        {
            _logger = logger;
            _config = config;
            _connectionMultiplexer = connectionMultiplexer;
            streamName = _config["StreamName"]!;
            groupName = _config["GroupName"]!;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                _workOrders.TryGetValue("Pr-XC001", out WorkOrder? workOrder);
                
                if (workOrder is null )
                     CreateProductionWo();
                else
                    await ProcessWorkOrder(workOrder,stoppingToken);

                await Consumer.Consume(_connectionMultiplexer,streamName,groupName, stoppingToken);
              
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void CreateProductionWo()
        {
            _logger.LogInformation("No work orders to process. Creating one");
            var random = new Random();
           
            WorkOrder wo = new WorkOrder()
            {
                WorkOrderNumber = $"WO{random.Next(10,10000)}",
                ItemName = $"PreForm-{random.Next(3,100)}",
                ProductionLineNr = "Pr-XC001",
                ItemQuantity = 100,
                DaysToExpire = 30,
                WorkOrderStatus = 0
            };
            
            _workOrders.TryAdd(wo.ProductionLineNr, wo);
        }

        private async Task ProcessWorkOrder(WorkOrder workOrder,CancellationToken token)
        {
            if (workOrder.WorkOrderStatus == 0)
            {
                _logger.LogInformation($"Work work Status 0 Activating: {workOrder.WorkOrderNumber}");
                workOrder.WorkOrderStatus = 1;
                _workOrders.AddOrUpdate(workOrder.WorkOrderNumber, workOrder, (key, oldValue) => workOrder);
            }
            else if (workOrder.WorkOrderStatus == 1)
            {
                _logger.LogInformation($"Processing work order {workOrder.WorkOrderNumber}");
                await LineProducer.Produce(_connectionMultiplexer,workOrder, streamName, groupName, token);
            }
            else
            {
                _logger.LogInformation($"Work order {workOrder.WorkOrderNumber} already completed");
            }
        }

    }
}
