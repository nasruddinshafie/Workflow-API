using System.Text.Json;

namespace workflowAPI.Models.Callbacks
{
    /// <summary>
    /// Helper class for deserializing ProcessInstance from callback
    /// Workflow Server sends ProcessInstance as generic object
    /// This helper converts it to strongly-typed ProcessInstanceData
    /// </summary>
    public static class  ProcessInstanceHelper
    {
        /// <summary>
        /// Deserialize ProcessInstance object to ProcessInstanceData
        /// </summary>
        /// <param name="processInstanceObject">Raw object from callback</param>
        /// <returns>Strongly-typed ProcessInstanceData</returns>
        public static ProcessInstance? Deserialize(object? processInstanceObject)
        {
            if (processInstanceObject == null)
                return null;

            try
            {
                // If it's already a JsonElement (from System.Text.Json)
                if (processInstanceObject is JsonElement jsonElement)
                {
                    return JsonSerializer.Deserialize<ProcessInstance>(
                        jsonElement.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                // If it's a string (JSON string)
                if (processInstanceObject is string jsonString)
                {
                    return JsonSerializer.Deserialize<ProcessInstance>(
                        jsonString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                // Convert to JSON and deserialize
                var json = JsonSerializer.Serialize(processInstanceObject);
                return JsonSerializer.Deserialize<ProcessInstance>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get parameter value from ProcessInstance
        /// </summary>
        public static T? GetParameter<T>(ProcessInstance processInstance, string parameterName)
        {
            if (processInstance.ProcessParameters == null || !processInstance.ProcessParameters.ContainsKey(parameterName))
                return default;

            var value = processInstance.ProcessParameters[parameterName];

            if (value is JsonElement jsonElement)
            {
                return JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
            }

            if (value is T typedValue)
                return typedValue;

            // Try convert
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default;
            }
        }

    }
}
