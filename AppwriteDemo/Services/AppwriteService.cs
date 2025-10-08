using Appwrite;
using Appwrite.Models;
using Appwrite.Services;
using AppwriteDemo.Models;

namespace AppwriteDemo.Services;

public class AppwriteService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AppwriteService> _logger;

    public AppwriteService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<AppwriteService> logger)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    // ============================================
    // Authentication Methods
    // ============================================

    /// <summary>
    /// Creates a session client for authenticated requests
    /// </summary>
    public Client CreateSessionClient()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            throw new InvalidOperationException("HttpContext is not available");

        var sessionCookie = httpContext.Request.Cookies["appwrite-auth-session"];
        if (string.IsNullOrEmpty(sessionCookie))
            throw new UnauthorizedAccessException("No session found");

        var client = new Client()
            .SetEndpoint(_configuration["Appwrite:Endpoint"] ?? throw new InvalidOperationException("Appwrite endpoint not configured"))
            .SetProject(_configuration["Appwrite:ProjectId"] ?? throw new InvalidOperationException("Appwrite project ID not configured"))
            .SetSession(sessionCookie);

        return client;
    }

    /// <summary>
    /// Creates an admin client for server-side operations
    /// </summary>
    public Client CreateAdminClient()
    {
        var client = new Client()
            .SetEndpoint(_configuration["Appwrite:Endpoint"] ?? throw new InvalidOperationException("Appwrite endpoint not configured"))
            .SetProject(_configuration["Appwrite:ProjectId"] ?? throw new InvalidOperationException("Appwrite project ID not configured"))
            .SetKey(_configuration["Appwrite:ApiKey"] ?? throw new InvalidOperationException("Appwrite API key not configured"));

        return client;
    }

    /// <summary>
    /// Gets the currently logged in user
    /// </summary>
    public async Task<User?> GetLoggedInUserAsync()
    {
        try
        {
            var client = CreateSessionClient();
            var account = new Account(client);
            return await account.Get();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Creates a new user account and session
    /// </summary>
    public async Task<Session> SignUpWithEmailAsync(string email, string password, string name)
    {
        var client = CreateAdminClient();
        var account = new Account(client);

        // Create the user account
        await account.Create(
            userId: ID.Unique(),
            email: email,
            password: password,
            name: name
        );

        // Create a session for the user
        var session = await account.CreateEmailPasswordSession(
            email: email,
            password: password
        );

        return session;
    }

    /// <summary>
    /// Signs in an existing user
    /// </summary>
    public async Task<Session> SignInWithEmailAsync(string email, string password)
    {
        var client = CreateAdminClient();
        var account = new Account(client);

        var session = await account.CreateEmailPasswordSession(
            email: email,
            password: password
        );

        return session;
    }

    /// <summary>
    /// Signs out the current user
    /// </summary>
    public async Task SignOutAsync()
    {
        _logger.LogInformation("Starting sign out process");

        try
        {
            var client = CreateSessionClient();
            var account = new Account(client);
            await account.DeleteSession("current");
            _logger.LogInformation("Successfully deleted Appwrite session");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error deleting Appwrite session: {ErrorMessage}", ex.Message);
            // Continue with cookie clearing even if Appwrite session deletion fails
        }
    }

    /// <summary>
    /// Sets the session cookie
    /// </summary>
    public void SetSessionCookie(string sessionSecret)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            throw new InvalidOperationException("HttpContext is not available");

        _logger.LogDebug("Setting session cookie with secret: {SessionSecretPrefix}...", sessionSecret[..Math.Min(10, sessionSecret.Length)]);
        
        var cookieOptions = new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = false // Set to false for development HTTP
        };

        httpContext.Response.Cookies.Append("appwrite-auth-session", sessionSecret, cookieOptions);
        _logger.LogDebug("Session cookie set successfully");
    }

    // ============================================
    // Database Methods for Todos
    // ============================================

    /// <summary>
    /// Creates a new todo item
    /// </summary>
    public async Task<TodoItem> CreateTodoAsync(string title)
    {
        var client = CreateSessionClient();
        var databases = new Databases(client);
        var user = await GetLoggedInUserAsync();

        if (user == null)
            throw new UnauthorizedAccessException("User not authenticated");

        var data = new Dictionary<string, object>
        {
            { "title", title },
            { "isCompleted", false }
        };

        var databaseId = _configuration["Appwrite:DatabaseId"] ?? throw new InvalidOperationException("Database ID not configured");
        var collectionId = _configuration["Appwrite:TodosCollectionId"] ?? throw new InvalidOperationException("Todos collection ID not configured");

        var document = await databases.CreateDocument(
            databaseId: databaseId,
            collectionId: collectionId,
            documentId: ID.Unique(),
            data: data
        );

        return DocumentToTodo(document);
    }

    /// <summary>
    /// Gets all todos for the current user
    /// </summary>
    public async Task<List<TodoItem>> GetTodosAsync()
    {
        var client = CreateSessionClient();
        var databases = new Databases(client);
        var user = await GetLoggedInUserAsync();

        if (user == null)
            throw new UnauthorizedAccessException("User not authenticated");

        var queries = new List<string>
        {
            Query.OrderDesc("$createdAt")
        };

        var databaseId = _configuration["Appwrite:DatabaseId"] ?? throw new InvalidOperationException("Database ID not configured");
        var collectionId = _configuration["Appwrite:TodosCollectionId"] ?? throw new InvalidOperationException("Todos collection ID not configured");

        var documents = await databases.ListDocuments(
            databaseId: databaseId,
            collectionId: collectionId,
            queries: queries
        );

        return documents.Documents.Select(DocumentToTodo).ToList();
    }

    /// <summary>
    /// Updates a todo item
    /// </summary>
    public async Task<TodoItem> UpdateTodoAsync(string todoId, string title, bool isCompleted)
    {
        var client = CreateSessionClient();
        var databases = new Databases(client);

        var data = new Dictionary<string, object>
        {
            { "title", title },
            { "isCompleted", isCompleted }
        };

        var databaseId = _configuration["Appwrite:DatabaseId"] ?? throw new InvalidOperationException("Database ID not configured");
        var collectionId = _configuration["Appwrite:TodosCollectionId"] ?? throw new InvalidOperationException("Todos collection ID not configured");

        var document = await databases.UpdateDocument(
            databaseId: databaseId,
            collectionId: collectionId,
            documentId: todoId,
            data: data
        );

        return DocumentToTodo(document);
    }

    /// <summary>
    /// Deletes a todo item
    /// </summary>
    public async Task DeleteTodoAsync(string todoId)
    {
        var client = CreateSessionClient();
        var databases = new Databases(client);

        var databaseId = _configuration["Appwrite:DatabaseId"] ?? throw new InvalidOperationException("Database ID not configured");
        var collectionId = _configuration["Appwrite:TodosCollectionId"] ?? throw new InvalidOperationException("Todos collection ID not configured");

        await databases.DeleteDocument(
            databaseId: databaseId,
            collectionId: collectionId,
            documentId: todoId
        );
    }

    /// <summary>
    /// Converts an Appwrite document to a TodoItem
    /// </summary>
    private TodoItem DocumentToTodo(Document document)
    {
        return new TodoItem
        {
            Id = document.Id,
            Title = document.Data["title"]?.ToString() ?? "",
            IsCompleted = document.Data["isCompleted"] is bool completed ? completed : false,
            CreatedAt = document.Data["$createdAt"] != null 
                ? DateTime.Parse(document.Data["$createdAt"].ToString()!) 
                : DateTime.UtcNow
        };
    }
}