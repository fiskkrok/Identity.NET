using Identity.UnderTheHood.Authorization;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add authentication services and configure options
builder.Services.AddAuthentication("MyCookieAuth")
.AddCookie("MyCookieAuth", options =>
{
    options.Cookie.Name = "MyCookieAuth"; // Set cookie name
    options.LoginPath = "/Account/Login"; // Set the path to the login page
    options.AccessDeniedPath = "Account/AccessDenied"; // Set the path to the access denied page
    options.ExpireTimeSpan = TimeSpan.FromMinutes(2); // Set the cookie expiration time span
});

// Add authorization services and define custom policies
builder.Services.AddAuthorization(options =>
{
    // Add policy for admin-only access
    options.AddPolicy("AdminOnly", policy => policy
    .RequireClaim("Admin"));

    // Add policy for HR department access
    options.AddPolicy("MustBelongToHRDepartment", policy => policy
           .RequireClaim("Department", "HR"));

    // Add policy for HR manager access with a probation requirement
    options.AddPolicy("HRManagerOnly", policy => policy
           .RequireClaim("Department", "HR")
           .RequireClaim("Manager")
           .Requirements.Add(new HRManagerProbationRequirement(3)));
});

// Register the authorization handler for the custom HR manager probation requirement
builder.Services.AddSingleton<IAuthorizationHandler, HRManagerProbationRequirementHandler>();

// Add Razor pages services
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error"); // Use the error handler in non-development environments
                                       // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts(); // Apply HSTS (HTTP Strict Transport Security) policy
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
app.UseStaticFiles(); // Enable serving static files

app.UseRouting(); // Enable routing

app.UseAuthentication(); // Use authentication middleware

app.UseAuthorization(); // Use authorization middleware

app.MapRazorPages(); // Map Razor pages in the application

app.Run(); // Run the application
