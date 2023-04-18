# Custom Authentication and Authorization in ASP.NET Core

This tutorial demonstrates how to implement custom authentication and authorization in an ASP.NET Core application using cookie authentication and custom authorization policies.

## Setting up the project

1. Create a new ASP.NET Core Razor Pages project using the command line or your preferred IDE.
2. Add the required namespaces to your `Program.cs` file:

```csharp
using Identity.UnderTheHood.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
```
## Configuring the services
1. In the `Program.cs` file, configure the `authentication` service with a custom cookie authentication scheme:
```csharp
builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.Cookie.Name = "MyCookieAuth";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(2);
    });
```
2. Configure the `authorization` service with custom policies:
```csharp
builder.Services.AddAuthorization(options => {
    options.AddPolicy("AdminOnly", policy => policy
           .RequireClaim("Admin"));

    options.AddPolicy("MustBelongToHRDepartment", policy => policy
           .RequireClaim("Department", "HR"));

    options.AddPolicy("HRManagerOnly", policy => policy
           .RequireClaim("Department", "HR")
           .RequireClaim("Manager")
           .Requirements.Add(new HRManagerProbationRequirement(3)));
});
```
3. Register the custom `authorization` handler:
```csharp
builder.Services.AddSingleton<IAuthorizationHandler, HRManagerProbationRequirementHandler>();
```
4. Add Razor Pages to the services:
```csharp
builder.Services.AddRazorPages();
```
5. Configure the middleware pipeline in the `Program.cs` file:
```csharp
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
```
## Creating the login page
1. Create a new folder called `Account` inside the Pages folder.
2. Add a new Razor Page called Login in the `Account` folder.
3. Replace the content of the `Login.cshtml.cs` file with the following code:
```csharp
// Add required namespaces
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Identity.UnderTheHood.Pages.Account
{
    public class LoginModel : PageModel
    {
        // Model binding for form input
        [BindProperty]
        public LoginCredential Credential { get; set; }

        // Empty method for handling GET request
        public void OnGet()
        {

        }

        // Method for handling POST request (async)
        public async Task<IActionResult> OnPostAsync()
        {
            // If the ModelState is invalid, return to the login page
            if (!ModelState.IsValid) return Page();

            // Sample validation for admin user
            if (Credential.UserName == "admin" && Credential.Password == "password")
            {
                // Create a list of claims for the user
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, "admin"),
                    new Claim(ClaimTypes.Email, "admin@mywebsite.com"),
                    new Claim("Department", "Hr"),
                    new Claim("Admin", "True"),
                    new Claim("Manager", "True"),
                    new Claim("EmplaymentDate", "2021-05-01")
                };
                // Create a new ClaimsIdentity with the user claims and the authentication scheme
                var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

                // Set the authentication properties
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = Credential.RememberMe
                };

                // Sign in the user with the custom cookie authentication
                await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal, authProperties);

                // Redirect to the home page
                return RedirectToPage("/Index");
            }

            // If the user login is not valid, return to the login page
            return Page();
        }

        // LoginCredential class for the form input
        public class LoginCredential
        {
            [Required]
            [Display(Name = "User Name")]
            public string UserName { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me")]
            public bool RememberMe { get; set; }
        }
    }
}
```
4. Replace the content of the Login.cshtml file with the following code:
```html
@page
@model Identity.UnderTheHood.Pages.Account.LoginModel
@{
}
<div class="container border" style="padding:20px">
    <form method="post">
        <div class="text-danger" asp-validation-summary="ModelOnly"></div>
        <div class="form-group row">
            <div class="col-2">
                <label asp-for="Credential.UserName"></label>
            </div>
            <div class="col-5">
                <input type="text" asp-for="Credential.UserName" class="form-control" />
            </div>
            <span class="text-danger" asp-validation-for="Credential.UserName"></span>
        </div>

        <div class="form-group row">
            <div class="col-2">
                <label asp-for="Credential.Password"></label>
            </div>
            <div class="col-5">
                <input type="password" asp-for="Credential.Password" class="form-control" />
            </div>
            <span class="text-danger" asp-validation-for="Credential.Password"></span>
        </div> 
        <div class="row form-check mb-2">
            <div class="col-2">
                <input type="checkbox" asp-for="Credential.RememberMe" class="form-check-input" />
                <label class="form-check-label" asp-for="Credential.RememberMe"></label>
            </div>
        </div>
        <div class="form-group row">
            <div class="col-2">
               <input type="submit" class="btn btn-primary" value="Login">
            </div>
            <div class="col-5">
            </div>
        </div>
    </form>
</div>
```
## Implementing custom authorization
1. Create a new folder called `Authorization` inside the project folder.
2. Add a new class called HRManagerProbationRequirement in the `Authorization` folder with the following code:
```csharp
using Microsoft.AspNetCore.Authorization;

namespace Identity.UnderTheHood.Authorization
{
    public class HRManagerProbationRequirement : IAuthorizationRequirement
    {
        public HRManagerProbationRequirement(int probationMonth)
        {
            ProbationMonth = probationMonth;
        }

        public int ProbationMonth { get; }
    }
}
```
3. Add a new class called HRManagerProbationRequirementHandler in
the `Authorization` folder with the following code:

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Identity.UnderTheHood.Authorization
{
    public class HRManagerProbationRequirementHandler : AuthorizationHandler<HRManagerProbationRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HRManagerProbationRequirement requirement)
        {
            if (!context.User.HasClaim(x => x.Type == "EmplaymentDate"))
                return Task.CompletedTask;

            var empDate = DateTime.Parse(context.User.FindFirst(x => x.Type == "EmploymentDate").Value);
            var period = DateTime.Now - empDate;
            if (period.Days > 30 * requirement.ProbationMonth)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
```
That's it! You have now implemented custom authentication and authorization in your ASP.NET Core application using cookie authentication and custom authorization policies. Run your application, and you should be able to test the login functionality and the custom authorization policies.

Remember to apply the authorization policies to your pages or actions, using the `[Authorize(Policy = "PolicyName")]` attribute.

With this tutorial, you should have a better understanding of how to create a custom authentication and authorization system in an ASP.NET Core application. You can now expand upon these concepts to create more complex authentication and authorization scenarios tailored to your application's needs.
