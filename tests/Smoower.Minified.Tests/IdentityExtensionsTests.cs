using Microsoft.AspNetCore.Identity;
using Smoower.Minified.Identity;
using Xunit;

namespace Smoower.Minified.Tests;

// Constructing a real UserManager needs a full store/options stack, so these assert
// the shorteners BIND to the right receiver + arguments + return types at compile
// time (the lambdas are never invoked). If a wrapper had the wrong signature this
// would fail to compile.
public class IdentityExtensionsTests
{
    [Fact]
    public void UserManager_Shorteners_Bind()
    {
        Func<UserManager<IdentityUser>, Task<IdentityResult>> create = m => m.create(new IdentityUser(), "Pw1!");
        Func<UserManager<IdentityUser>, Task<IdentityUser?>> byEmail = m => m.byEmail("a@b.c");
        Func<UserManager<IdentityUser>, Task<bool>> checkPw = m => m.checkPw(new IdentityUser(), "Pw1!");
        Func<UserManager<IdentityUser>, Task<IList<string>>> roles = m => m.roles(new IdentityUser());
        Func<UserManager<IdentityUser>, Task<IdentityResult>> addRole = m => m.addRole(new IdentityUser(), "admin");
        Assert.NotNull(create);
        Assert.NotNull(byEmail);
        Assert.NotNull(checkPw);
        Assert.NotNull(roles);
        Assert.NotNull(addRole);
    }

    [Fact]
    public void SignIn_And_Role_Shorteners_Bind()
    {
        Func<SignInManager<IdentityUser>, Task<SignInResult>> pwSignIn = m => m.pwSignIn("u", "Pw1!", persist: true);
        Func<RoleManager<IdentityRole>, Task<IdentityResult>> roleCreate = m => m.create(new IdentityRole("admin"));
        Func<RoleManager<IdentityRole>, Task<bool>> roleExists = m => m.exists("admin");
        Assert.NotNull(pwSignIn);
        Assert.NotNull(roleCreate);
        Assert.NotNull(roleExists);
    }
}
