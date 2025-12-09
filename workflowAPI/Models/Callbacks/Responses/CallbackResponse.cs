namespace workflowAPI.Models.Callbacks.Responses
{
    public class CallbackResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Code { get; set; }
        public List<string> Messages { get; set; }
    }

    public class CallbackResponse : CallbackResponse<object> { }

}
