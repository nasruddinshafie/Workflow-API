namespace workflowAPI.Models.Callbacks.Events
{
    public class EventBaseRequest
    {
        public Guid ProcessId { get; set; }
        public string SchemeCode { get; set; }
        public object ProcessInstance { get; set; }
    }

  
}
