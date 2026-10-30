using Atomos.API.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace Atomos.Tests;

public sealed class AuthorizationPolicyTests
{
    [Fact]
    public void UserAreaControllersAreStudentOnly()
    {
        Type[] controllers =
        [
            typeof(AboutController),
            typeof(LearningController),
            typeof(LookupController),
            typeof(ProfileController)
        ];

        foreach (var controller in controllers)
        {
            Assert.Contains(controller.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Cast<AuthorizeAttribute>(),
                attribute => attribute.Policy == "StudentOnly");
        }
    }
}
