# 📋 **Logging Guide for Blazor Appwrite Application**

## **1. Console Logs (Current Implementation)**

Your application already has console logging. To see these logs:

```bash
# Run your application and watch the terminal
dotnet run --project "AppwriteDemo\AppwriteDemo.csproj"

# You'll see logs like:
# Setting session cookie with secret: eyJpZCI6Im...
# Starting sign out process...
# Successfully deleted Appwrite session.
```

## **2. View Logs in Different Ways**

### **Terminal Output**
- Console.WriteLine messages appear in the terminal where you run `dotnet run`
- These are immediate and useful for development

### **Browser Developer Tools**
- Press F12 in your browser
- Go to Console tab
- Look for any JavaScript errors or Blazor SignalR connection issues

### **Application Logs (ILogger)**
- Better than Console.WriteLine
- Can be configured to write to files, databases, etc.
- Supports different log levels (Debug, Info, Warning, Error)

## **3. Log Levels and What to Look For**

### **Authentication Issues**
```
Look for:
- "Error deleting Appwrite session"
- "HttpContext not available"
- "No session found" 
- "Invalid credentials"
```

### **Cookie Issues**
```
Look for:
- "Session cookie set successfully"
- "Session cookie clearing commands sent"
- Check if cookies are being set/cleared properly
```

### **Navigation Issues**
```
Look for:
- SignOut page messages
- Redirect loops
- Navigation failures
```

## **4. How to Enable More Detailed Logging**

### **In appsettings.Development.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "AppwriteDemo": "Debug",
      "Microsoft.AspNetCore.Components": "Debug"
    }
  }
}
```

### **In your services (using ILogger):**
Instead of Console.WriteLine, use:
```csharp
_logger.LogInformation("User signed in: {Email}", email);
_logger.LogWarning("Failed to delete session: {Error}", ex.Message);
_logger.LogError(ex, "Critical error during authentication");
```

## **5. Quick Debug Commands**

### **View Application Logs:**
```bash
# Run with verbose logging
dotnet run --verbosity detailed

# Run with specific log level
dotnet run --project AppwriteDemo.csproj --configuration Debug
```

### **View Build Issues:**
```bash
# Get detailed build output
dotnet build --verbosity detailed

# Check for warnings and errors
dotnet build --no-restore
```

## **6. Common Debug Scenarios**

### **Sign Out Not Working:**
1. Check terminal for "Starting sign out process..."
2. Look for "Successfully deleted Appwrite session"
3. Verify "Session cookie clearing commands sent"
4. Check browser cookies (F12 > Application > Cookies)

### **Authentication Failures:**
1. Look for "Invalid credentials" or similar
2. Check Appwrite configuration in terminal output
3. Verify cookie presence and format
4. Test Appwrite connection

### **Page Not Loading:**
1. Check for navigation errors in terminal
2. Look for component rendering errors
3. Verify route configuration
4. Check for missing dependencies

## **7. Browser Debug Tools**

### **Network Tab (F12):**
- Check for failed API requests
- Verify cookie headers
- Look for 401/403 authentication errors

### **Application Tab (F12):**
- View cookies and their values
- Check localStorage/sessionStorage
- Verify service worker status

### **Console Tab (F12):**
- Look for JavaScript errors
- Check Blazor SignalR connection
- View any client-side logging

## **8. Real-time Log Monitoring**

Your application logs appear in real-time in the terminal. When you:
- Sign in → See "Setting session cookie..."  
- Sign out → See "Starting sign out process..."
- Have errors → See error messages immediately

This makes debugging much easier than traditional web applications!