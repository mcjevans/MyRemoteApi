using OpenIddict.Abstractions;
using MyRemoteApi.Data; // Replace with your actual namespace

public class SeedData(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        Console.WriteLine("Scope Created");
        
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Console.WriteLine(context.Database.ToString());
        Console.WriteLine("Context GET");
        await context.Database.EnsureCreatedAsync(cancellationToken);
        Console.WriteLine(cancellationToken.ToString());

        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        // Check if the client already exists
        if (await manager.FindByClientIdAsync("my_local_app", cancellationToken) is null)
        {
            Console.WriteLine("finding");

            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "my_local_app",
                ClientSecret = "846B62D0-DEF9-4215-A99D-86E6B8D9D074", // Use a secure secret
                DisplayName = "My Website",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles
                }
                ,
                // These must match the URLs where your website is running
                RedirectUris = { new Uri("https://localhost:5001/signin-oidc") }
                ,
                PostLogoutRedirectUris = { new Uri("https://localhost:5001/signout-callback-oidc") }
                // },
                //     cancellationToken
            });
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}