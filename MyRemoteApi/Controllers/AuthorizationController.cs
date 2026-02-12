using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Microsoft.AspNetCore;

// This maps the controller to the "connect" path
[Route("connect")] 
public class AuthorizationController : Controller
{
    //Let's do some Dependcy Injection!
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    // Inject the managers here
    public AuthorizationController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // Now you can use _userManager in your Authorize and Exchange methods
    /*
        // This handles: https://localhost:5001/connect/authorize
        [HttpGet("authorize")]
        [HttpPost("authorize")]
        public async Task<IActionResult> Authorize()
        {
            // ... logic to show login page ...
        }
    */

    [HttpGet("authorize")]
    [HttpPost("authorize")]
    public async Task<IActionResult> Authorize()
    {
        Console.WriteLine("Authorize Task invoked");
        var request = HttpContext.GetOpenIddictServerRequest() ??
        throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Check if the user is logged into the API (Identity)
        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

        // If not logged in, redirect to the Login page and preserve the OIDC parameters
        if (!result.Succeeded)
        {
        return Challenge(
            authenticationSchemes: IdentityConstants.ApplicationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                    Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
            });
        }

        // User is logged in! Now create the identity for OpenIddict
        var user = await _userManager.GetUserAsync(result.Principal);
        var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        if (user != null) //try this, catch the other end?
        {
            // Required 'sub' claim (usually the User ID)
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject,
                await _userManager.GetUserIdAsync(user),
                OpenIddictConstants.Destinations.AccessToken,
                OpenIddictConstants.Destinations.IdentityToken));

            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Email,
                    await _userManager.GetEmailAsync(user),
                    OpenIddictConstants.Destinations.AccessToken,
                    OpenIddictConstants.Destinations.IdentityToken));
        }
        else
            Console.WriteLine("user==null");

        var principal = new ClaimsPrincipal(identity);

        // Set the scopes (e.g., offline_access for refresh tokens)
        principal.SetScopes(request.GetScopes());

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }


    // // This handles: https://localhost:5001/connect/token
    // [HttpPost("token")]
    // public async Task<IActionResult> Exchange()
    // {
    //     // ... logic to swap a code for a JWT ...
    // }
    [HttpPost("token")]
    [IgnoreAntiforgeryToken] // Essential for API-to-API calls
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        Console.WriteLine("Exchange Task invoked");
        // 1. Extract the request from the HttpContext
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // 2. We are specifically looking for an 'Authorization Code' swap
        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            // 3. Retrieve the claims principal stored in the authorization code/refresh token.
            // This 'principal' was created earlier in the Authorize method.
            ClaimsPrincipal? principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

            //            var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

            // 4. Ensure the user is still allowed to log in (check for locked accounts, etc.)
            IdentityUser? user = null;
            if (principal != null) // var // on next line
                user = await _userManager.GetUserAsync(principal);
            else
            //            if (user == null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme] = "The token is no longer valid."
                    }));
            }

            // 5. Sign in the user using the OpenIddict server scheme.
            // This is where OpenIddict generates the actual JWT and sends it back.
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new NotImplementedException("The specified grant type is not implemented.");
    }
}