
namespace Production.WorkerService.Model;

public class WorkOrder
{
    public string WorkOrderNumber { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ProductionLineNr { get; set; } = string.Empty;
    public int ItemQuantity { get; set; }
    public int DaysToExpire { get; set; } // 0 means no expiration
    public short WorkOrderStatus { get; set; } // 0 - Created, 1 - In Progress, 2 - Completed

}
