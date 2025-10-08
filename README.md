# Blazor Appwrite Authentication Demo

This application demonstrates how to implement Server-Side Rendering (SSR) authentication in a Blazor Server application using Appwrite, following the same patterns as the Next.js SSR authentication tutorial.

## Features

- **Email/Password Authentication**: Sign up and sign in with email and password
- **Server-Side Session Management**: Sessions are managed server-side with secure HTTP-only cookies
- **Protected Routes**: Automatic redirection based on authentication state
- **User Account Management**: View user profile information and sign out

## Prerequisites

- .NET 9.0 or later
- An Appwrite instance (cloud or self-hosted)
- Appwrite project with authentication enabled

## Setup

### 1. Appwrite Configuration

1. Create an Appwrite project at [Appwrite Console](https://cloud.appwrite.io/)
2. Enable Email/Password authentication in your project settings
3. Create an API key with the following scopes:
   - `sessions.write` - Allows API key to create, update, and delete sessions
4. Add `localhost:5172` to your platform settings (Web platform)

### 2. Application Configuration

Update your `appsettings.Development.json` with your Appwrite credentials:

```json
{
  "Appwrite": {
    "Endpoint": "https://[YOUR-REGION].cloud.appwrite.io/v1",
    "ProjectId": "your-project-id",
    "ApiKey": "your-api-key"
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

The `AppwriteService` class provides the core authentication functionality:

- **CreateSessionClient()**: Creates an Appwrite client with the current user's session
- **CreateAdminClient()**: Creates an admin Appwrite client for server operations
- **GetLoggedInUserAsync()**: Retrieves the current user information
- **SignUpWithEmailAsync()**: Creates a new user account and session
- **SignInWithEmailAsync()**: Authenticates an existing user
- **SignOutAsync()**: Signs out the current user and clears the session

### Authentication Flow

1. **Home Page**: Checks authentication state and redirects accordingly
2. **Sign Up/Sign In**: Forms for user authentication
3. **Account Page**: Protected page showing user information
4. **Session Management**: Uses HTTP-only cookies for secure session storage

### Security Features

- HTTP-only cookies prevent XSS attacks
- Secure flag ensures cookies are only sent over HTTPS
- SameSite=Strict prevents CSRF attacks
- Server-side session validation
- Automatic session cleanup on sign out

## Pages

- `/` - Home page (redirects based on auth state)
- `/signup` - User registration
- `/signin` - User login
- `/account` - Protected user account page

## Usage

1. Navigate to the application
2. If not authenticated, you'll be redirected to the sign-up page
3. Create a new account or sign in with existing credentials
4. Access your account information on the protected account page
5. Sign out when finished

## Error Handling

The application includes comprehensive error handling:
- Invalid credentials during sign in
- Network errors during API calls
- Session expiration handling
- Graceful fallbacks for authentication failures

## Development Notes

This implementation mirrors the Next.js SSR authentication tutorial patterns:
- Server-side session management
- Protected routes with automatic redirection
- Secure cookie handling
- Admin vs. session client separation

The key difference is that Blazor Server handles the server-client communication automatically, eliminating the need for separate API routes.