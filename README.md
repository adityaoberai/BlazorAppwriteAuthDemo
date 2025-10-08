# Blazor Appwrite Authentication Demo

This application demonstrates how to implement Server-Side Rendering (SSR) authentication in a Blazor Server application using Appwrite, following the same patterns as the Next.js SSR authentication tutorial. It showcases both authentication and database operations with a complete todo list application.

## Features

- **Email/Password Authentication**: Sign up and sign in with email and password
- **Server-Side Session Management**: Sessions are managed server-side with secure HTTP-only cookies
- **Protected Routes**: Automatic redirection based on authentication state
- **User Account Management**: View user profile information and sign out
- **Todo List Application**: Full CRUD operations for managing todos with Appwrite Databases
- **Document-Level Security**: Todos are secured with user-specific permissions

## Prerequisites

- .NET 9.0 or later
- An Appwrite instance (cloud or self-hosted)
- Appwrite project with authentication enabled
- Appwrite CLI (optional, for deploying database schema)

## Setup

### 1. Appwrite Configuration

1. Create an Appwrite project at [Appwrite Console](https://cloud.appwrite.io/)
2. Enable Email/Password authentication in your project settings
3. Create an API key with the following scopes:
   - `sessions.write` - Allows API key to create, update, and delete sessions
4. Add `localhost:5172` to your platform settings (Web platform)

#### Database Setup

You can set up the database and collection in two ways:

**Option 1: Using Appwrite CLI (Recommended)**

```bash
# Install Appwrite CLI if you haven't already
npm install -g appwrite-cli

# Login to Appwrite
appwrite login

# Deploy the database schema
appwrite push -a
```

> Note: Ensure you update the project ID in the `appwrite.config.json` file to your project.

**Option 2: Manual Setup**

1. In the Appwrite Console, create a new database named `todos-db`
2. Create a collection named `todos` with the following:
   - **Collection ID**: `todos`
   - **Document Security**: Enabled
   - **Collection Permissions**: 
     - Add `create("users")` permission
   - **Attributes**:
     - `title` (String, size: 255, required)
     - `isCompleted` (Boolean, default: false)

The `appwrite.config.json` file in the project root contains the complete database schema configuration.

### 2. Application Configuration

Update your `appsettings.json` with your Appwrite credentials:

```json
{
  "Appwrite": {
    "Endpoint": "https://[YOUR-REGION].cloud.appwrite.io/v1",
    "ProjectId": "your-project-id",
    "ApiKey": "your-api-key",
    "DatabaseId": "todos-db",
    "TodosCollectionId": "todos"
  }
}
```

### 3. Run the Application

```bash
dotnet run
```

Navigate to `http://localhost:5172` to start using the application.

## Architecture

### AppwriteService

The `AppwriteService` class provides the core authentication and database functionality:

**Authentication Methods:**
- **CreateSessionClient()**: Creates an Appwrite client with the current user's session
- **CreateAdminClient()**: Creates an admin Appwrite client for server operations
- **GetLoggedInUserAsync()**: Retrieves the current user information
- **SignUpWithEmailAsync()**: Creates a new user account and session
- **SignInWithEmailAsync()**: Authenticates an existing user
- **SignOutAsync()**: Signs out the current user and clears the session
- **SetSessionCookie()**: Manages session cookies securely

**Database Methods:**
- **CreateTodoAsync()**: Creates a new todo item for the authenticated user
- **GetTodosAsync()**: Retrieves all todos for the current user
- **UpdateTodoAsync()**: Updates an existing todo item
- **DeleteTodoAsync()**: Deletes a todo item
- **GetTodoByIdAsync()**: Retrieves a specific todo by ID

### Authentication Flow

1. **Home Page**: Checks authentication state and redirects accordingly
2. **Sign Up/Sign In**: Forms for user authentication
3. **Account Page**: Protected page showing user information
4. **Session Management**: Uses HTTP-only cookies for secure session storage

### Security Features

- **HTTP-only cookies**: Prevent XSS attacks by making cookies inaccessible to JavaScript
- **Secure flag**: Ensures cookies are only sent over HTTPS in production
- **SameSite=Strict**: Prevents CSRF attacks
- **Server-side session validation**: All authentication checks happen on the server
- **Automatic session cleanup**: Sessions are properly cleaned up on sign out
- **Document-level security**: Users can only access their own todos
- **User-specific permissions**: Each todo document has read/write permissions for its creator only

## Pages

- `/` - Home page (redirects based on auth state)
- `/signup` - User registration
- `/signin` - User login
- `/account` - Protected user account page
- `/todos` - Todo list management (protected)

## Usage

### Authentication

1. Navigate to the application at `http://localhost:5172`
2. If not authenticated, you'll be redirected to the sign-up page
3. Create a new account or sign in with existing credentials
4. Access your account information on the protected account page

### Todo Management

1. After signing in, navigate to the Todos page
2. Add new todos using the input field at the top
3. Check/uncheck todos to mark them as complete/incomplete
4. Delete todos by clicking the delete button
5. All todos are automatically saved to your Appwrite database
6. Your todos are private and only visible to you

### Sign Out

Click the "Sign Out" button on the account or todos page when finished

## Error Handling

The application includes comprehensive error handling:
- Invalid credentials during sign in
- Network errors during API calls
- Session expiration handling
- Graceful fallbacks for authentication failures
- Todo operation error handling with user-friendly messages
- Database connection error handling

## Technology Stack

- **.NET 9.0**: Latest version of .NET for modern web development
- **Blazor Web App**: Server-side rendering framework for building interactive web applications
- **Appwrite SDK**: Official .NET SDK for Appwrite integration
- **Bootstrap 5**: For responsive UI components
- **Appwrite Services Used**:
  - Account API for authentication
  - Databases API for todo management
  - Session management for secure authentication

## Project Structure

```
AppwriteDemo/
├── Components/
│   ├── Pages/              # Razor pages
│   │   ├── Account.razor   # User account page
│   │   ├── Home.razor      # Landing page
│   │   ├── SignIn.razor    # Sign in form
│   │   ├── SignUp.razor    # Sign up form
│   │   └── Todos.razor     # Todo list page
│   ├── Layout/             # Layout components
│   └── Shared/             # Shared components
├── Models/
│   ├── AuthModels.cs       # Authentication models
│   └── TodoModels.cs       # Todo data models
├── Services/
│   └── AppwriteService.cs  # Appwrite integration service
├── Program.cs              # Application entry point
└── appsettings.json        # Configuration

appwrite.config.json        # Appwrite schema configuration
```

## Contributing

Feel free to submit issues or pull requests to improve this demo application.

## License

This project is open source and available for educational purposes.

## Resources

- [Appwrite Documentation](https://appwrite.io/docs)
- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Appwrite .NET SDK](https://github.com/appwrite/sdk-for-dotnet)
- [Next.js SSR Authentication Tutorial](https://appwrite.io/docs/products/auth/server-side-rendering)

## Development Notes

This implementation mirrors the Next.js SSR authentication tutorial patterns:
- Server-side session management
- Protected routes with automatic redirection
- Secure cookie handling
- Admin vs. session client separation

The key difference is that Blazor Server handles the server-client communication automatically, eliminating the need for separate API routes.