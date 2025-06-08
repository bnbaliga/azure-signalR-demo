using Newtonsoft.Json.Linq;

namespace AzureFunctionAppSignalR_Isolated.Models
{
    public class BroadcastModel
    {
        public string? Target { get; set; }
        public string? Message{ get; set; }
    }
}
