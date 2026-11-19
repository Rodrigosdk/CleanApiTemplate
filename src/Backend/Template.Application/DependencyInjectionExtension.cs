using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Template.Application.Users.Login;
using Template.Application.Users.Profile;
using Template.Application.Users.Register;

[assembly: InternalsVisibleTo("Template.Application.Tests")]

namespace Template.Application;

public static class DependencyInjectionExtension
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
        services.AddScoped<ILoginUseCase, LoginUseCase>();
        services.AddScoped<IGetUserProfileUseCase, GetUserProfileUseCase>();
    }
}
