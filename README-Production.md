# Blazor Appwrite Demo - Production Deployment Guide

## Overview
This is a Blazor Server application that integrates with Appwrite for authentication and todo management.

## Production Configuration

### Required Environment Variables

For production deployment, you need to configure the following Appwrite settings:

1. **APPWRITE_ENDPOINT** - Your Appwrite server endpoint (e.g., `https://fra.cloud.appwrite.io/v1`)
2. **APPWRITE_PROJECT_ID** - Your Appwrite project ID
3. **APPWRITE_API_KEY** - Your Appwrite API key with appropriate permissions
4. **APPWRITE_DATABASE_ID** - Your Appwrite database ID
5. **APPWRITE_TODOS_COLLECTION_ID** - Your todos collection ID

### Configuration Methods

#### Option 1: Environment Variables (Recommended)
Set the environment variables in your hosting platform:

```bash
APPWRITE_ENDPOINT=https://fra.cloud.appwrite.io/v1
APPWRITE_PROJECT_ID=your_project_id_here
APPWRITE_API_KEY=your_api_key_here
APPWRITE_DATABASE_ID=your_database_id_here
APPWRITE_TODOS_COLLECTION_ID=your_collection_id_here
```

#### Option 2: Update appsettings.Production.json
Replace the placeholder values in `appsettings.Production.json`:

```json
{
  "Appwrite": {
    "Endpoint": "https://fra.cloud.appwrite.io/v1",
    "ProjectId": "your_project_id_here",
    "ApiKey": "your_api_key_here",
    "DatabaseId": "your_database_id_here",
    "TodosCollectionId": "your_collection_id_here"
  }
}
```

## Common Production Issues & Solutions

### 1. Configuration Errors
**Error**: "Application configuration error. Please contact support."
**Solution**: Ensure all Appwrite configuration values are properly set and not using placeholder values.

### 2. Cookie Security Issues
**Error**: Session cookies not being set or cleared properly
**Solution**: Ensure your production environment supports HTTPS. The application automatically adjusts cookie security settings based on the environment.

### 3. CORS Issues
**Error**: Requests being blocked by CORS policy
**Solution**: Configure your Appwrite project to allow requests from your production domain.

### 4. Database/Collection Not Found
**Error**: Database or collection ID errors
**Solution**: Verify that your database and collection IDs are correct and that the collection has the proper structure.

## Required Appwrite Setup

### Database Schema
Create a collection called "todos" with the following attributes:
- `title` (String, required)
- `isCompleted` (Boolean, required, default: false)

### API Key Permissions
Your API key should have the following permissions:
- `databases.read`
- `databases.write`
- `users.read`
- `sessions.write`
- `sessions.delete`

### Platform Configuration
Add your production domain to the allowed origins in your Appwrite project settings.

## Deployment Steps

1. **Configure Appwrite**
   - Set up your database and collection
   - Configure API key permissions
   - Add your production domain to allowed origins

2. **Set Configuration**
   - Set environment variables or update configuration files
   - Ensure all placeholder values are replaced

3. **Deploy Application**
   - Build the application: `dotnet publish -c Release`
   - Deploy to your hosting platform
   - Verify configuration validation passes on startup

4. **Test Functionality**
   - Test user registration and login
   - Test todo creation, updating, and deletion
   - Check error handling and logging

## Troubleshooting

### Application Won't Start
- Check application logs for configuration validation errors
- Verify all required configuration values are set
- Ensure Appwrite server is accessible from your hosting environment

### Authentication Issues
- Verify Appwrite project ID and endpoint are correct
- Check that your domain is added to Appwrite's allowed origins
- Ensure API key has proper permissions

### Database Operations Fail
- Verify database and collection IDs are correct
- Check that collection schema matches expected structure
- Ensure API key has database read/write permissions

## Monitoring & Logging

The application includes comprehensive logging for:
- Configuration validation
- Authentication events  
- Database operations
- Error conditions

Monitor these logs to diagnose production issues.

## Security Considerations

- API keys should never be committed to source control
- Use environment variables or secure configuration management
- Ensure HTTPS is enabled in production
- Regularly rotate API keys
- Monitor authentication logs for suspicious activity