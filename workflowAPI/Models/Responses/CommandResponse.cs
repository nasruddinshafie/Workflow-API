namespace workflowAPI.Models.Responses
{
    public class CommandParameter
    {
        public bool IsRequired { get; set; }
        public string ParameterName { get; set; } = string.Empty;
        public string LocalizedName { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string? DefaultValue { get; set; }
    }

    public class CommandResponse
    {
        public string CommandName { get; set; } = string.Empty;
        public string LocalizedName { get; set; } = string.Empty;
        public string? ValidForActivityName { get; set; }
        public string? ValidForStateName { get; set; }
        public string? Classifier { get; set; }
        public List<string>? Identities { get; set; }
        public string? ProcessId { get; set; }
        public bool IsForSubprocess { get; set; }
        public List<CommandParameter>? Parameters { get; set; }
    }
}
