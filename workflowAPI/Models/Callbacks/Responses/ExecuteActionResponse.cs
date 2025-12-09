namespace workflowAPI.Models.Callbacks.Responses
{
    public class ExecuteActionResponseData
    {
        public Dictionary<string, object> UpdatedParameters { get; set; }
    }

    public class ExecuteActionResponse : CallbackResponse<ExecuteActionResponseData>
    {
    }
}
