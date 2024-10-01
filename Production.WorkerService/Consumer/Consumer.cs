

namespace Production.WorkerService;
public static class Consumer
{       
    public static async Task Consume(IConnectionMultiplexer muxer, string streamName, string groupName, CancellationToken stoppingToken)
    {
            IDatabase db = muxer.GetDatabase();

            await SetStreamConsumerGroup(db, streamName,  groupName, stoppingToken);
            await ReadPendingMessagesAsync(db, streamName, groupName, stoppingToken);

            await Task.Delay(1000, stoppingToken);
    }
    private static async Task SetStreamConsumerGroup(IDatabase db, string streamName, string groupName, CancellationToken token)
    {
        if(!token.IsCancellationRequested)
        {
            if (!(await db.KeyExistsAsync(streamName)) ||
                  (await db.StreamGroupInfoAsync(streamName)).All(x => x.Name != groupName))
            {
                await db.StreamCreateConsumerGroupAsync(streamName, groupName, "0-0", true);
            }
        }
    }
    private static async Task ReadPendingMessagesAsync(IDatabase db,string streamName, string groupName ,CancellationToken token)
    {
        if (!token.IsCancellationRequested)
        {
            var result = await db.StreamRangeAsync(streamName, "-", "+", 1, Order.Descending);

            if (result.Any())
            {
                string id = result.FirstOrDefault().Id.ToString();
                string? value = result.FirstOrDefault().Values.First().Value;
                
                if (!string.IsNullOrEmpty(value))
                {
                    var item = JsonSerializer.Deserialize<Item>(value!);
                    Console.WriteLine($"Consumed Item: {item.ItemName}, Ean:{item.Ean}, Batch:{item.BatchNumber} " +
                        $"Wo Nr: {item.WorkOrderNumber}, Produced Line Nr:. {item.ProductionLineNr}");
                }
                await db.StreamAcknowledgeAsync(streamName, groupName, id);

                var rs = await db.StreamReadGroupAsync(streamName, groupName, "keys-1", ">", 1);

                //await db.StreamAcknowledgeAsync(streamName, groupName, rs.FirstOrDefault().Id);

                Console.WriteLine($"Consumed Item: {id}");
            }
        }
    }

}
