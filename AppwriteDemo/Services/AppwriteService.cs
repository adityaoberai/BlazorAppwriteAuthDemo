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
    private readonly IWebHostEnvironment _environment;

    public AppwriteService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<AppwriteService> logger, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _environment = environment;
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
        {
            _logger.LogError("HttpContext is not available when creating session client");
            throw new InvalidOperationException("HttpContext is not available");
        }

        var sessionCookie = httpContext.Request.Cookies["appwrite-auth-session"];
        if (string.IsNullOrEmpty(sessionCookie))
        {
            _logger.LogWarning("No session cookie found when creating session client");
            throw new UnauthorizedAccessException("No session found");
        }

        var endpoint = _configuration["Appwrite:Endpoint"];
        var projectId = _configuration["Appwrite:ProjectId"];

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(projectId))
        {
            _logger.LogError("Appwrite configuration is missing. Endpoint: {Endpoint}, ProjectId: {ProjectId}", 
                endpoint ?? "NULL", projectId ?? "NULL");
            throw new InvalidOperationException("Appwrite configuration is incomplete");
        }

        try
        {
            var client = new Client()
                .SetEndpoint(endpoint)
                .SetProject(projectId)
                .SetSession(sessionCookie);

            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Appwrite session client");
            throw;
        }
    }

    /// <summary>
    /// Creates an admin client for server-side operations
    /// </summary>
    public Client CreateAdminClient()
    {
        var endpoint = _configuration["Appwrite:Endpoint"];
        var projectId = _configuration["Appwrite:ProjectId"];
        var apiKey = _configuration["Appwrite:ApiKey"];

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogError("Appwrite admin configuration is missing. Endpoint: {Endpoint}, ProjectId: {ProjectId}, ApiKey: {HasApiKey}", 
                endpoint ?? "NULL", projectId ?? "NULL", !string.IsNullOrEmpty(apiKey));
            throw new InvalidOperationException("Appwrite admin configuration is incomplete");
        }

        try
        {
            var client = new Client()
                .SetEndpoint(endpoint)
                .SetProject(projectId)
                .SetKey(apiKey);

            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Appwrite admin client");
            throw;
        }
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
            var user = await account.Get();
            _logger.LogDebug("Successfully retrieved user: {UserId}", user.Id);
            return user;
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogDebug("No valid session found for user");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get logged in user: {ErrorMessage}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Creates a new user account and session
    /// </summary>
    public async Task<Session> SignUpWithEmailAsync(string email, string password, string name)
    {
        try
        {
            var client = CreateAdminClient();
            var account = new Account(client);

            _logger.LogInformation("Creating new user account for: {Email}", email);

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

            _logger.LogInformation("Successfully created user account and session for: {Email}", email);
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sign up user with email: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Signs in an existing user
    /// </summary>
    public async Task<Session> SignInWithEmailAsync(string email, string password)
    {
        try
        {
            var client = CreateAdminClient();
            var account = new Account(client);

            _logger.LogInformation("Attempting to sign in user: {Email}", email);

            var session = await account.CreateEmailPasswordSession(
                email: email,
                password: password
            );

            _logger.LogInformation("Successfully signed in user: {Email}", email);
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sign in user with email: {Email}", email);
            throw;
        }
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
        {
            _logger.LogError("HttpContext is not available when setting session cookie");
            throw new InvalidOperationException("HttpContext is not available");
        }

        _logger.LogDebug("Setting session cookie with secret: {SessionSecretPrefix}...", 
            sessionSecret[..Math.Min(10, sessionSecret.Length)]);
        
        var cookieOptions = new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            // Only use Secure in production and when HTTPS is available
            Secure = _environment.IsProduction() && httpContext.Request.IsHttps
        };

        httpContext.Response.Cookies.Append("appwrite-auth-session", sessionSecret, cookieOptions);
        _logger.LogDebug("Session cookie set successfully");
    }

    /// <summary>
    /// Clears the session cookie
    /// </summary>
    public void ClearSessionCookie()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            _logger.LogError("HttpContext is not available when clearing session cookie");
            return;
        }

        httpContext.Response.Cookies.Delete("appwrite-auth-session");
        _logger.LogDebug("Session cookie cleared successfully");
    }

    // ============================================
    // Database Methods for Todos
    // ============================================

    /// <summary>
    /// Creates a new todo item
    /// </summary>
    public async Task<TodoItem> CreateTodoAsync(string title)
    {
        try
        {
            var client = CreateSessionClient();
            var databases = new Databases(client);
            var user = await GetLoggedInUserAsync();

            if (user == null)
            {
                _logger.LogWarning("Attempted to create todo without authenticated user");
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var data = new Dictionary<string, object>
            {
                { "title", title },
                { "isCompleted", false }
            };

            var databaseId = _configuration["Appwrite:DatabaseId"];
            var collectionId = _configuration["Appwrite:TodosCollectionId"];

            if (string.IsNullOrEmpty(databaseId) || string.IsNullOrEmpty(collectionId))
            {
                _logger.LogError("Database configuration is missing. DatabaseId: {DatabaseId}, CollectionId: {CollectionId}",
                    databaseId ?? "NULL", collectionId ?? "NULL");
                throw new InvalidOperationException("Database configuration is incomplete");
            }

            var document = await databases.CreateDocument(
                databaseId: databaseId,
                collectionId: collectionId,
                documentId: ID.Unique(),
                data: data
            );

            _logger.LogInformation("Successfully created todo: {TodoTitle} for user: {UserId}", title, user.Id);
            return DocumentToTodo(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create todo: {TodoTitle}", title);
            throw;
        }
    }

    /// <summary>
    /// Gets all todos for the current user
    /// </summary>
    public async Task<List<TodoItem>> GetTodosAsync()
    {
        try
        {
            var client = CreateSessionClient();
            var databases = new Databases(client);
            var user = await GetLoggedInUserAsync();

            if (user == null)
            {
                _logger.LogWarning("Attempted to get todos without authenticated user");
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var queries = new List<string>
            {
                Query.OrderDesc("$createdAt")
            };

            var databaseId = _configuration["Appwrite:DatabaseId"];
            var collectionId = _configuration["Appwrite:TodosCollectionId"];

            if (string.IsNullOrEmpty(databaseId) || string.IsNullOrEmpty(collectionId))
            {
                _logger.LogError("Database configuration is missing. DatabaseId: {DatabaseId}, CollectionId: {CollectionId}",
                    databaseId ?? "NULL", collectionId ?? "NULL");
                throw new InvalidOperationException("Database configuration is incomplete");
            }

            var documents = await databases.ListDocuments(
                databaseId: databaseId,
                collectionId: collectionId,
                queries: queries
            );

            var todos = documents.Documents.Select(DocumentToTodo).ToList();
            _logger.LogDebug("Successfully retrieved {TodoCount} todos for user: {UserId}", todos.Count, user.Id);
            return todos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get todos");
            throw;
        }
    }

    /// <summary>
    /// Updates a todo item
    /// </summary>
    public async Task<TodoItem> UpdateTodoAsync(string todoId, string title, bool isCompleted)
    {
        try
        {
            var client = CreateSessionClient();
            var databases = new Databases(client);

            var data = new Dictionary<string, object>
            {
                { "title", title },
                { "isCompleted", isCompleted }
            };

            var databaseId = _configuration["Appwrite:DatabaseId"];
            var collectionId = _configuration["Appwrite:TodosCollectionId"];

            if (string.IsNullOrEmpty(databaseId) || string.IsNullOrEmpty(collectionId))
            {
                _logger.LogError("Database configuration is missing. DatabaseId: {DatabaseId}, CollectionId: {CollectionId}",
                    databaseId ?? "NULL", collectionId ?? "NULL");
                throw new InvalidOperationException("Database configuration is incomplete");
            }

            var document = await databases.UpdateDocument(
                databaseId: databaseId,
                collectionId: collectionId,
                documentId: todoId,
                data: data
            );

            _logger.LogInformation("Successfully updated todo: {TodoId}", todoId);
            return DocumentToTodo(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update todo: {TodoId}", todoId);
            throw;
        }
    }

    /// <summary>
    /// Deletes a todo item
    /// </summary>
    public async Task DeleteTodoAsync(string todoId)
    {
        try
        {
            var client = CreateSessionClient();
            var databases = new Databases(client);

            var databaseId = _configuration["Appwrite:DatabaseId"];
            var collectionId = _configuration["Appwrite:TodosCollectionId"];

            if (string.IsNullOrEmpty(databaseId) || string.IsNullOrEmpty(collectionId))
            {
                _logger.LogError("Database configuration is missing. DatabaseId: {DatabaseId}, CollectionId: {CollectionId}",
                    databaseId ?? "NULL", collectionId ?? "NULL");
                throw new InvalidOperationException("Database configuration is incomplete");
            }

            await databases.DeleteDocument(
                databaseId: databaseId,
                collectionId: collectionId,
                documentId: todoId
            );

            _logger.LogInformation("Successfully deleted todo: {TodoId}", todoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete todo: {TodoId}", todoId);
            throw;
        }
    }

    /// <summary>
    /// Converts an Appwrite document to a TodoItem
    /// </summary>
    private TodoItem DocumentToTodo(Document document)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert document to TodoItem: {DocumentId}", document.Id);
            throw;
        }
    }
}