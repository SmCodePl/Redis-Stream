

namespace Production.WorkerService.Model;  

public class Item
{
    public string ItemName { get; set; } = string.Empty;
    
    public int ItemQuantity { get; set; }
    public DateTime ProductionDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string ProductionLineNr { get; set; } = string.Empty;
    public string WorkOrderNumber { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public string Ean { get; set; } = string.Empty;

}
