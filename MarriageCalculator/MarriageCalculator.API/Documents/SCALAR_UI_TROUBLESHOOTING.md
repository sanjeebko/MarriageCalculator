# Scalar UI Troubleshooting Guide

## Issue: Scalar UI shows blank page with blue background and dots

This guide will help you troubleshoot and fix the Scalar UI configuration.

## Step-by-Step Troubleshooting

### Step 1: Verify the Application is Running
1. **Start the API**: Run `dotnet run` in the API project directory
2. **Check the console output**: Look for any startup errors
3. **Verify the port**: Note the port number (usually 5000/5001 for HTTP or 7000/7001 for HTTPS)

### Step 2: Test Swagger JSON Generation
Before testing Scalar UI, ensure OpenAPI document is being generated correctly:

1. **Access Swagger JSON directly**: 
   - Navigate to: `https://localhost:7000/swagger/v1/swagger.json`
   - You should see a JSON document with your API definition
   - If this fails, there's an issue with Swagger generation

2. **Check for controllers**: The JSON should contain your controller endpoints like:
   ```json
   {
     "openapi": "3.0.1",
     "info": {
       "title": "Marriage Calculator API",
       "version": "v1"
     },
     "paths": {
       "/api/Players": { ... },
       "/api/GameSettings": { ... },
       "/api/Database": { ... }
     }
   }
   ```

### Step 3: Test Traditional Swagger UI
1. **Access Swagger UI**: Navigate to `https://localhost:7000/swagger`
2. **Verify endpoints appear**: You should see all your controllers and endpoints
3. **Test an endpoint**: Try the `GET /api/database/info` endpoint

### Step 4: Test Scalar UI
1. **Access Scalar UI**: Navigate to `https://localhost:7000/scalar/v1`
2. **Check browser console**: Open F12 Developer Tools and check for JavaScript errors
3. **Verify network requests**: Check if the OpenAPI document is being loaded

## Common Issues and Solutions

### Issue 1: No Controllers Appear in Swagger JSON

**Possible Causes:**
- Controllers not properly decorated with `[ApiController]` and `[Route]` attributes
- Controllers not in the correct namespace
- Build errors preventing controller discovery

**Solution:**
```csharp
// Ensure your controllers look like this:
[ApiController]
[Route("api/[controller]")]
public class YourController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<SomeDto>> GetSomething()
    {
        // Your code here
    }
}
```

### Issue 2: Scalar UI Cannot Load OpenAPI Document

**Possible Causes:**
- CORS issues
- Incorrect OpenAPI document path
- Swagger not properly configured

**Solution:**
Update Program.cs to ensure proper ordering:
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();           // This must come first
    app.UseSwaggerUI(...);      // Traditional Swagger UI
    app.MapScalarApiReference(); // Scalar UI
}
```

### Issue 3: Browser Console Errors

**Common Errors and Solutions:**

1. **Failed to fetch**: Network connectivity issue
   - Check if API is running
   - Verify port numbers
   - Check firewall settings

2. **CORS Error**: Cross-origin resource sharing issue
   ```csharp
   // Add CORS if needed
   builder.Services.AddCors(options =>
   {
       options.AddDefaultPolicy(builder =>
       {
           builder.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
       });
   });
   
   // Use CORS
   app.UseCors();
   ```

3. **404 on swagger.json**: OpenAPI document not available
   - Ensure `app.UseSwagger()` is called
   - Check if running in Development environment

## Alternative Troubleshooting Steps

### Option 1: Use Default Scalar Configuration
Try the simplest possible configuration:
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.MapScalarApiReference(); // Uses default settings
}
```

### Option 2: Explicit Document Configuration
```csharp
app.MapScalarApiReference(options =>
{
    options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
});
```

### Option 3: Enable CORS for Development
```csharp
builder.Services.AddCors();

// In app configuration:
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
```

## Testing URLs

Test these URLs in your browser (replace `7000` with your actual port):

1. **API Base**: `https://localhost:7000/`
2. **Swagger JSON**: `https://localhost:7000/swagger/v1/swagger.json`
3. **Swagger UI**: `https://localhost:7000/swagger`
4. **Scalar UI**: `https://localhost:7000/scalar/v1`
5. **Test Endpoint**: `https://localhost:7000/api/database/info`

## Expected Results

### Swagger JSON Should Show:
- All your controllers (Players, GameSettings, Database, etc.)
- All HTTP methods (GET, POST, PUT, DELETE)
- Proper response schemas

### Scalar UI Should Show:
- Modern, clean interface
- All API endpoints organized by controller
- Interactive testing capabilities
- Model schemas

## Current Configuration

Your current configuration should work with these URLs:
- **Scalar UI**: `https://localhost:7000/scalar/v1`
- **Swagger UI**: `https://localhost:7000/swagger`

If you're still seeing a blank page, try these steps in order:

1. Restart the application
2. Clear browser cache
3. Try an incognito/private browsing window
4. Check the browser's developer console for errors
5. Verify the swagger.json URL is accessible

## Next Steps

If the issue persists:
1. Share the browser console error messages
2. Confirm which URLs work and which don't
3. Share the exact port numbers and URLs you're using
4. Check if there are any antivirus or proxy settings interfering