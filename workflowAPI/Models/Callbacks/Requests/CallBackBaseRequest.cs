namespace workflowAPI.Models.Callbacks.Requests
{
    public class CallBackBaseRequest
    {
        public object ProcessInstance { get; set; }
        public string Name { get; set; }
        public object Parameter { get; set; }
        
    }
}
