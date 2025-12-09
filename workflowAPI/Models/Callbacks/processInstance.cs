using static System.TimeZoneInfo;

namespace workflowAPI.Models.Callbacks
{
    public class ProcessInstance
    {
        public Guid Id { get; set; }
        public string StateName { get; set; }
        public string ActivityName { get; set; }
        public Guid? SchemeId { get; set; }
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
        public byte InstanceStatus { get; set; }
        public bool IsSubProcess { get; set; }
        public string TenantId { get; set; }

        public List<TransitionItem> Transitions { get; set; }
        public List<WorkflowServerProcessHistoryItem> History { get; set; }
        public Dictionary<string, object> ProcessParameters { get; set; }
        public List<Guid> SubProcessIds { get; set; }
    }

    public class TransitionItem
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

    public class WorkflowServerProcessHistoryItem
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string IdentityId { get; set; }
        public string AllowedToEmployeeNames { get; set; }
        public DateTime? TransitionTime { get; set; }
        public long Order { get; set; }
        public string InitialState { get; set; }
        public string DestinationState { get; set; }
        public string Command { get; set; }
    }
}
