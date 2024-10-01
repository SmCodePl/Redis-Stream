
namespace Production.WorkerService.Publisher;

public static class LineProducer
{
 
    public static async Task Produce(IConnectionMultiplexer muxer, WorkOrder wo, string streamName, string groupName, CancellationToken token)
    {
        var itm = GetTestItem(wo);

        string json = JsonSerializer.Serialize(itm);

        var message = new NameValueEntry[]
        {
            new NameValueEntry("Item", json)
        };
       
        await ProduceEventAsync(muxer, streamName,groupName, message, token);

    }
    private static async Task ProduceEventAsync(IConnectionMultiplexer muxer, string streamName, string groupName, NameValueEntry[] message, CancellationToken stoppingToken)
    {
            IDatabase db = muxer.GetDatabase();
            if (!(await db.KeyExistsAsync(streamName)) ||
                (await db.StreamGroupInfoAsync(streamName)).All(x => x.Name != groupName))
            {
                await db.StreamCreateConsumerGroupAsync(streamName, groupName, "0-0", true);
            }
            await db.StreamAddAsync(streamName, message);
    }
    private static Item GetTestItem(WorkOrder wo)
    {
        var random = new Random();

       return  new Item()
        {
            BatchNumber = Guid.NewGuid().ToString(),
            Ean = $"EAN{random.Next(100, 10000)}",
            ExpirationDate = DateTime.Now.AddDays(wo.DaysToExpire),
            ItemName = wo.ItemName,
            ItemQuantity = wo.ItemQuantity,
            ProductionDate = DateTime.Now,
            ProductionLineNr = wo.ProductionLineNr,
            WorkOrderNumber = wo.WorkOrderNumber
        };
    }
}
