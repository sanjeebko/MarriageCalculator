using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MarriageCalculator.API.Services;

public class FcmService : IFcmService
{
    private readonly ILogger<FcmService> _logger;
    private readonly bool _isFirebaseEnabled;

    public FcmService(IConfiguration configuration, ILogger<FcmService> logger)
    {
        _logger = logger;

        try
        {
            var projectId = configuration["Firebase:ProjectId"];
            
            // Check if Firebase App is already initialized
            if (FirebaseApp.DefaultInstance == null)
            {
                var serviceAccountJson = configuration["Firebase:ServiceAccountKeyJson"];
                var credentialsPath = configuration["Firebase:ServiceAccountKeyPath"];

                if (!string.IsNullOrEmpty(serviceAccountJson))
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromJson(serviceAccountJson),
                        ProjectId = projectId
                    });
                    _isFirebaseEnabled = true;
                    _logger.LogInformation("FirebaseApp initialized successfully using service account JSON credentials.");
                }
                else if (!string.IsNullOrEmpty(credentialsPath))
                {
                    // Resolve relative paths relative to application base directory
                    var resolvedPath = Path.IsPathRooted(credentialsPath) 
                        ? credentialsPath 
                        : Path.Combine(AppContext.BaseDirectory, credentialsPath);

                    if (File.Exists(resolvedPath))
                    {
                        FirebaseApp.Create(new AppOptions
                        {
                            Credential = GoogleCredential.FromFile(resolvedPath),
                            ProjectId = projectId
                        });
                        _isFirebaseEnabled = true;
                        _logger.LogInformation("FirebaseApp initialized successfully using service account key file at: {Path}", resolvedPath);
                    }
                    else
                    {
                        _isFirebaseEnabled = false;
                        _logger.LogWarning("Firebase:ServiceAccountKeyPath was specified but the file could not be found at: {Path}", resolvedPath);
                    }
                }
                else if (!string.IsNullOrEmpty(projectId))
                {
                    // Fallback to Application Default Credentials
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.GetApplicationDefault(),
                        ProjectId = projectId
                    });
                    _isFirebaseEnabled = true;
                    _logger.LogInformation("FirebaseApp initialized successfully using Application Default Credentials.");
                }
                else
                {
                    _isFirebaseEnabled = false;
                    _logger.LogWarning("Firebase configurations (ProjectId, ServiceAccountKeyPath, or ServiceAccountKeyJson) are not configured. FCM notifications will run in Mock (logging-only) mode.");
                }
            }
            else
            {
                _isFirebaseEnabled = true;
            }
        }
        catch (Exception ex)
        {
            _isFirebaseEnabled = false;
            _logger.LogWarning(ex, "Failed to initialize FirebaseAdmin. Push notifications will run in Mock (logging-only) mode.");
        }
    }

    public async Task SendNotificationAsync(string token, string title, string body, Dictionary<string, string>? data = null)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("FCM: Cannot send notification because the destination token is null or empty.");
            return;
        }

        if (!_isFirebaseEnabled)
        {
            _logger.LogInformation("FCM [Mock Mode]: Sending push notification.\n  To Token: {Token}\n  Title: {Title}\n  Body: {Body}\n  Data: {Data}",
                token, title, body, data != null ? string.Join(", ", data) : "none");
            return;
        }

        try
        {
            var message = new Message
            {
                Token = token,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation("FCM [Production Mode]: Successfully sent message. Response: {Response}", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FCM: Error sending push notification to token {Token}", token);
        }
    }

    public async Task SendDataMessageAsync(string token, Dictionary<string, string> data)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("FCM: Cannot send data message because the destination token is null or empty.");
            return;
        }

        if (!_isFirebaseEnabled)
        {
            _logger.LogInformation("FCM [Mock Mode]: Sending data-only push message.\n  To Token: {Token}\n  Data: {Data}",
                token, string.Join(", ", data));
            return;
        }

        try
        {
            // No Notification block on purpose - see IFcmService.SendDataMessageAsync doc comment.
            var message = new Message
            {
                Token = token,
                Data = data
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation("FCM [Production Mode]: Successfully sent data message. Response: {Response}", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FCM: Error sending data message to token {Token}", token);
        }
    }
}
