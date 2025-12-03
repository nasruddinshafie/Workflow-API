namespace workflowAPI.Services
{
    public interface IWorkflowSchemeRegistry
    {
        string GetActiveScheme(string workflowType);
        string? GetSchemeVersion(string workflowType, string version);
        List<string> GetAvailableVersions(string workflowType);
        bool IsValidWorkflowType(string workflowType);
        List<string> GetAllWorkflowTypes();
    }
}
