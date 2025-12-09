namespace workflowAPI.Models.Responses
{
    public class GetInstanceInfoResponse
    {
        public Guid Id { get; set; }
        public string StateName { get; set; }
        public string ActivityName { get; set; }
        public Guid SchemeId { get; set; }
        public string SchemeCode { get; set; }
        public string PreviousState { get; set; }
        public string PreviousStateForDirect { get; set; }
        public string PreviousStateForReverse { get; set; }
        public string PreviousActivity { get; set; }
        public string PreviousActivityForDirect { get; set; }
        public string PreviousActivityForReverse { get; set; }
        public Guid? ParentProcessId { get; set; }
        public Guid? RootProcessId { get; set; }
        public bool IsDeterminingParametersChanged { get; set; }
        public InstanceStatus InstanceStatus { get; set; }
        public bool IsSubProcess { get; set; }
        public string TenantId { get; set; }
        public List<InstanceTransition> Transitions { get; set; }
        public List<InstanceHistory> History { get; set; }
        public Dictionary<string, object> ProcessParameters { get; set; }
        public List<Guid> SubProcessIds { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
        public string Message { get; set; }
    }

    public class InstanceTransition
    {
        public Guid ProcessId { get; set; }
        public string ActorIdentityId { get; set; }
        public string ExecutorIdentityId { get; set; }
        public string FromActivityName { get; set; }
        public string FromStateName { get; set; }
        public bool IsFinalised { get; set; }
        public string ToActivityName { get; set; }
        public string ToStateName { get; set; }
        public string TransitionClassifier { get; set; }
        public DateTime TransitionTime { get; set; }
        public string TriggerName { get; set; }
    }

    public class InstanceHistory
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string IdentityId { get; set; }
        public string AllowedToEmployeeNames { get; set; }
        public DateTime TransitionTime { get; set; }
        public int Order { get; set; }
        public string InitialState { get; set; }
        public string DestinationState { get; set; }
        public string Command { get; set; }
    }

    public enum InstanceStatus
    {
        Initialized = 0,
        Running = 1,
        Idled = 2,
        Finalized = 3,
        Terminated = 4,
        Error = 5
    }
}
