namespace AppwriteDemo.Services;

public class ConfigurationValidationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationValidationService> _logger;

    public ConfigurationValidationService(IConfiguration configuration, ILogger<ConfigurationValidationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public bool ValidateAppwriteConfiguration()
    {
        var requiredSettings = new Dictionary<string, string?>
        {
            ["Appwrite:Endpoint"] = _configuration["Appwrite:Endpoint"],
            ["Appwrite:ProjectId"] = _configuration["Appwrite:ProjectId"],
            ["Appwrite:ApiKey"] = _configuration["Appwrite:ApiKey"],
            ["Appwrite:DatabaseId"] = _configuration["Appwrite:DatabaseId"],
            ["Appwrite:TodosCollectionId"] = _configuration["Appwrite:TodosCollectionId"]
        };

        var missingSettings = new List<string>();
        var placeholderSettings = new List<string>();

        foreach (var setting in requiredSettings)
        {
            if (string.IsNullOrEmpty(setting.Value))
            {
                missingSettings.Add(setting.Key);
            }
            else if (IsPlaceholderValue(setting.Value))
            {
                placeholderSettings.Add(setting.Key);
            }
        }

        if (missingSettings.Any())
        {
            _logger.LogError("Missing required Appwrite configuration settings: {MissingSettings}", 
                string.Join(", ", missingSettings));
        }

        if (placeholderSettings.Any())
        {
            _logger.LogError("Appwrite configuration contains placeholder values: {PlaceholderSettings}", 
                string.Join(", ", placeholderSettings));
        }

        var isValid = !missingSettings.Any() && !placeholderSettings.Any();

        if (isValid)
        {
            _logger.LogInformation("Appwrite configuration validation passed");
        }
        else
        {
            _logger.LogError("Appwrite configuration validation failed. Missing: {Missing}, Placeholders: {Placeholders}",
                string.Join(", ", missingSettings), string.Join(", ", placeholderSettings));
        }

        return isValid;
    }

    private static bool IsPlaceholderValue(string value)
    {
        var placeholderPatterns = new[]
        {
            "YOUR_PROJECT_ID_HERE",
            "YOUR_API_KEY_HERE", 
            "YOUR_DATABASE_ID_HERE",
            "YOUR_COLLECTION_ID_HERE",
            "<REGION>",
            "REPLACE_WITH_YOUR"
        };

        return placeholderPatterns.Any(pattern => value.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }
}