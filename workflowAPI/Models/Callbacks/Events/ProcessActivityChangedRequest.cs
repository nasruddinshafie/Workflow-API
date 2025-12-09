namespace workflowAPI.Models.Callbacks.Events
{
    /// <summary>
    /// Process Activity Changed Callback
    /// Called when workflow activity changes
    /// </summary>
    public class ProcessActivityChangedRequest : EventBaseRequest
    {
        public string previousActivityName { get; set; }
        public string currentActivityName { get; set; }
    }

 
}
