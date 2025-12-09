using System.Diagnostics;
using System.Text.Json;
using workflowAPI.Models.Requests;
using workflowAPI.Models.Responses;

namespace workflowAPI.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly HttpClient _httpClient;
        private readonly IWorkflowSchemeRegistry _schemeRegistry;
        private readonly ILogger<WorkflowService> _logger;


        public WorkflowService(
           HttpClient httpClient,
           IWorkflowSchemeRegistry schemeRegistry,
           ILogger<WorkflowService> logger)
        {
            _httpClient = httpClient;
            _schemeRegistry = schemeRegistry;
            _logger = logger;
        }

        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public async Task<string> CreateInstanceAsync(string workflowType, string identityId, Dictionary<string, object> parameters)
        {
            var schemeCode = _schemeRegistry.GetActiveScheme(workflowType);

            _logger.LogInformation(
                "Creating {WorkflowType} workflow using scheme {SchemeCode} for user {IdentityId}",
                workflowType,
                schemeCode,
                identityId);

            var ProcessId = Guid.NewGuid().ToString();

            // Old OptimaJet API only sends schemeCode in body
            var requestBody = new { schemeCode = schemeCode };

            var response = await _httpClient.PostAsJsonAsync(
                $"/workflowapi/createinstance/{ProcessId}",
                requestBody);

            response.EnsureSuccessStatusCode();

            // Old OptimaJet API wraps response in { success, data, error, message }
            var wrappedResult = await response.Content.ReadFromJsonAsync<OptimaJetApiResponse>();

            if (wrappedResult == null)
                throw new Exception("Failed to deserialize OptimaJet response");

            if (!wrappedResult.Success)
            {
                var errorMsg = wrappedResult.Message ?? wrappedResult.Error ?? "Unknown error from Workflow Server";
                _logger.LogError(
                    "Failed to create workflow instance: {Error}",
                    errorMsg);
                throw new Exception($"Workflow Server error: {errorMsg}");
            }

            // Parse the data object into CreateInstanceResponse
            var dataJson = JsonSerializer.Serialize(wrappedResult, _jsonOptions);

            var result = JsonSerializer.Deserialize<CreateInstanceResponse>(dataJson, _jsonOptions);


            if (!result.Success)
                throw new Exception("Failed to deserialize workflow instance data");

            _logger.LogInformation(
                "Created workflow instance {ProcessId} with scheme {SchemeCode}",
                ProcessId,
                schemeCode);

            return ProcessId;
        }

        // Internal class for OptimaJet old API response format
        private class OptimaJetApiResponse
        {
            public bool Success { get; set; }
            public object? Data { get; set; }
            public string? Error { get; set; }
            public string? Message { get; set; }
        }

        public async Task ExecuteCommandAsync(ExecuteCommandRequest request)
        {
            _logger.LogInformation(
           "Executing command {Command} on {ProcessId} by {IdentityId}",
           request.Command,
           request.ProcessId,
           request.IdentityId);

            var response = await _httpClient.PostAsJsonAsync(
                "/workflowapi/executecommand",
                request);

            response.EnsureSuccessStatusCode();

            // Handle old OptimaJet API response format
            var wrappedResult = await response.Content.ReadFromJsonAsync<OptimaJetApiResponse>();

            if (wrappedResult == null)
                throw new Exception("Failed to deserialize OptimaJet response");

            if (!wrappedResult.Success)
            {
                var errorMsg = wrappedResult.Message ?? wrappedResult.Error ?? "Unknown error from Workflow Server";
                _logger.LogError(
                    "Failed to execute command {Command} on {ProcessId}: {Error}",
                    request.Command,
                    request.ProcessId,
                    errorMsg);
                throw new Exception($"Workflow Server error: {errorMsg}");
            }

            _logger.LogInformation(
                "Command {Command} executed successfully on {ProcessId}",
                request.Command,
                request.ProcessId);
        }

        public async Task<CommandsResponse> GetAvailableCommandsAsync(string processId, string identityId)
        {
            _logger.LogInformation(
            "Getting available commands for {ProcessId} by {IdentityId}",
            processId,
            identityId);

            var response = await _httpClient.GetAsync(
                $"/workflowapi/availablecommands/{processId}?identityId={identityId}");

            response.EnsureSuccessStatusCode();

            // Handle old OptimaJet API response format
            var wrappedResult = await response.Content.ReadFromJsonAsync<OptimaJetApiResponse>();

            if (wrappedResult == null)
                throw new Exception("Failed to deserialize OptimaJet response");

            if (!wrappedResult.Success)
            {
                var errorMsg = wrappedResult.Message ?? wrappedResult.Error ?? "Unknown error from Workflow Server";
                _logger.LogError(
                    "Failed to get available commands for {ProcessId}: {Error}",
                    processId,
                    errorMsg);
                throw new Exception($"Workflow Server error: {errorMsg}");
            }

            // Parse the data object into CommandsResponse
            var dataJson = JsonSerializer.Serialize(wrappedResult.Data, _jsonOptions);
            var result = JsonSerializer.Deserialize<CommandsResponse>(dataJson, _jsonOptions);

            if (result == null)
                throw new Exception("Failed to deserialize commands data");

            _logger.LogDebug(
                "Found {Count} available commands for {ProcessId}",
                result.Commands.Count,
                processId);

            return result;
        }

        public async Task<GetInstanceInfoResponse> GetInstanceAsync(string processId)
        {
            _logger.LogInformation("Getting workflow instance {ProcessId}", processId);

            var requestBody = new { tenantId = "" };


            var response = await _httpClient.PostAsJsonAsync($"/workflowapi/getinstanceinfo/{processId}", requestBody);
            response.EnsureSuccessStatusCode();

            // Handle old OptimaJet API response format
            var wrappedResult = await response.Content.ReadFromJsonAsync<OptimaJetApiResponse>();

            if (wrappedResult == null)
                throw new Exception("Failed to deserialize OptimaJet response");

            if (!wrappedResult.Success)
            {
                var errorMsg = wrappedResult.Message ?? wrappedResult.Error ?? "Unknown error from Workflow Server";
                _logger.LogError(
                    "Failed to get instance {ProcessId}: {Error}",
                    processId,
                    errorMsg);
                throw new Exception($"Workflow Server error: {errorMsg}");
            }

            // Parse the data object into WorkflowInstanceResponse
            var dataJson = JsonSerializer.Serialize(wrappedResult.Data, _jsonOptions);
            var result = JsonSerializer.Deserialize<GetInstanceInfoResponse>(dataJson, _jsonOptions);

            if (result == null)
                throw new Exception("Failed to deserialize instance data");

            _logger.LogDebug(
                "Retrieved instance {ProcessId} in state {StateName}",
                processId,
                result.StateName);

            return result;
        }

        public async Task<List<WorkflowInstanceResponse>> GetInstancesBySchemeAsync(string schemeCode, int limit = 100)
        {
            _logger.LogInformation(
             "Getting instances for scheme {SchemeCode} (limit: {Limit})",
             schemeCode,
             limit);

            // Note: This endpoint may vary depending on Workflow Server version
            // Adjust as needed for your Workflow Server API
            var response = await _httpClient.GetAsync(
                $"/workflowapi/instances?schemeCode={schemeCode}&limit={limit}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to get instances for scheme {SchemeCode}: {StatusCode}",
                    schemeCode,
                    response.StatusCode);
                return new List<WorkflowInstanceResponse>();
            }

            var result = await response.Content.ReadFromJsonAsync<List<WorkflowInstanceResponse>>();

            return result ?? new List<WorkflowInstanceResponse>();
        }


        public async Task<TResponse> PostAsync<TRequest,TResponse>(string url, TRequest request)
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);

            var httpResponse = await _httpClient.PostAsync(url, new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

            var responseContent = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("POST {Url} failed with status {StatusCode}: {ResponseContent}", url, httpResponse.StatusCode, responseContent);
                throw new Exception($"Request to {url} failed with status {httpResponse.StatusCode}");
            }

            var response = JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions);

            if (response == null)
            {
                _logger.LogError("Failed to deserialize response from {Url}", url);
                throw new Exception($"Failed to deserialize response from {url}");
            }

            return response;

        }
    }
}
