using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Xml.Linq;
using System;

namespace Identity.UnderTheHood.Pages.Account
{
    public class LoginModel : PageModel
    {
        // Model binding for form input
        [BindProperty]
        public LoginCredential Credential { get; set; }

        csharp
        Copy code
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

                // Create authentication properties with the RememberMe option
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = Credential.RememberMe
                };

                // Sign the user in using the claims principal and authentication properties
                await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);

                // Redirect the user to the home page
                return RedirectToPage("/Index");
            }

            // If credentials are invalid, return to the login page
            return Page();
        }

        // Class to store the user's login credentials
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