namespace workflowAPI.Models.Callbacks.Events
{
    /// <summary>
    /// Process Status Changed Callback
    /// Called when workflow instance moves to new state
    /// </summary>
    public class ProcessStatusChangedRequest : EventBaseRequest
    {
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
    }
}
