using CommunityToolkit.Maui.Animations;
using MarriageCalculator.Services.Implementations;
using MarriageCalculator.Services.Interfaces;

namespace MarriageCalculator;

public partial class App : Application
{
	IMarriageGameEngine MarriageGameEngine { get; }
    IAuthenticationService AuthenticationService { get; }
    IAuthenticationManager AuthenticationManager { get; }

    public App(IMarriageGameEngine marriageGameEngine, IAuthenticationService authenticationService, IAuthenticationManager authenticationManager)
    {
        string syncLicense = @"Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXZccHVSR2JfWUVyW0JWYEg=";
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncLicense);
        InitializeComponent();
        //string syncLicense = @"ORg4AjUWIQA/Gnt2UlhhQlVMfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hTX9Sd0diWXxXdXVTT2Ve;MzU3MzcyM0AzMjM3MmUzMDJlMzBXdk5vdG15VlV6dUFUdWpmOUZpSXFBVDFOeFh6VDBHZ1lRdGZXOFJvTzNVPQ==;Mgo+DSMBMAY9C3t2UlhhQlVMfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hTX9Sd0diWXxXdXVSQmNc;MzU3MzcyNUAzMjM3MmUzMDJlMzBoODRrc1lDbGV5Z2taNUlaWDhVTGd2dDNPaUkxZ216R1B5S0RtOWk3TnhJPQ==;MzU3MzcyNkAzMjM3MmUzMDJlMzBXdk5vdG15VlV6dUFUdWpmOUZpSXFBVDFOeFh6VDBHZ1lRdGZXOFJvTzNVPQ==";


        MainPage = new AppShell();
        MarriageGameEngine = marriageGameEngine;
        AuthenticationService = authenticationService;
        AuthenticationManager = authenticationManager;
    }
      
    protected override async void OnStart()
    {
        base.OnStart();
        
        // Initialize authentication first
        await AuthenticationService.InitializeAuthenticationAsync();
        
        // Check if user is already logged in and navigate accordingly
        await CheckAuthenticationAndNavigate();
    }
    
    protected override async void OnSleep() 
    {
        // Save settings when app goes to sleep
        await MarriageGameEngine.SettingsService.SaveSettingsAsync();
        base.OnSleep();
    }
    
    protected override async void OnResume()
    {
        base.OnResume();
        
        // Re-check authentication when app resumes
        await CheckAuthenticationAndNavigate();
    }
    
    private async Task CheckAuthenticationAndNavigate()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("App: Checking authentication status...");
            
            // Check if user has a valid authentication token
            bool isLoggedIn = await AuthenticationService.IsUserLoggedInAsync();
            
            System.Diagnostics.Debug.WriteLine($"App: User logged in status: {isLoggedIn}");
            
            // Navigate to appropriate page
            if (isLoggedIn)
            {
                // ✅ START AUTHENTICATION MANAGER FOR ALREADY LOGGED IN USER
                System.Diagnostics.Debug.WriteLine("App: User already logged in, starting AuthenticationManager");
                await AuthenticationManager.StartAsync();
                
                // User is already logged in, go to main page
                System.Diagnostics.Debug.WriteLine("App: Navigating to MainPage (user is logged in)");
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                // User is not logged in, stay on login page (already default)
                System.Diagnostics.Debug.WriteLine("App: Navigating to LoginPage (user not logged in)");
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"App: Error checking authentication: {ex.Message}");
            // If any error occurs, default to login page
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
    
    public static async Task Animate(VisualElement view)
    {
        var fadeAnimation = new FadeAnimation();
        await fadeAnimation.Animate(view);
    }
}
