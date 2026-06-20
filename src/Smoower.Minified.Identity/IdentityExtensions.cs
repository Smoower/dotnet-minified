using Microsoft.AspNetCore.Identity;

namespace Smoower.Minified.Identity;

// Short forms of the most common UserManager<T> operations. The long, multi-token
// BCL method names are the bulk of any auth flow; these wrap them 1:1.
public static class UserManagerExtensions
{
    public static Task<IdentityResult> create<T>(this UserManager<T> m, T user, string password) where T : class
        => m.CreateAsync(user, password);

    public static Task<IdentityResult> create<T>(this UserManager<T> m, T user) where T : class
        => m.CreateAsync(user);

    public static Task<T?> byEmail<T>(this UserManager<T> m, string email) where T : class
        => m.FindByEmailAsync(email);

    public static Task<T?> byName<T>(this UserManager<T> m, string name) where T : class
        => m.FindByNameAsync(name);

    public static Task<T?> byId<T>(this UserManager<T> m, string id) where T : class
        => m.FindByIdAsync(id);

    public static Task<bool> checkPw<T>(this UserManager<T> m, T user, string password) where T : class
        => m.CheckPasswordAsync(user, password);

    public static Task<IdentityResult> addRole<T>(this UserManager<T> m, T user, string role) where T : class
        => m.AddToRoleAsync(user, role);

    public static Task<IList<string>> roles<T>(this UserManager<T> m, T user) where T : class
        => m.GetRolesAsync(user);

    public static Task<IdentityResult> upd<T>(this UserManager<T> m, T user) where T : class
        => m.UpdateAsync(user);

    public static Task<IdentityResult> del<T>(this UserManager<T> m, T user) where T : class
        => m.DeleteAsync(user);
}

public static class SignInManagerExtensions
{
    public static Task<SignInResult> pwSignIn<T>(this SignInManager<T> m, string user, string password, bool persist = false, bool lockoutOnFailure = true) where T : class
        => m.PasswordSignInAsync(user, password, persist, lockoutOnFailure);

    public static Task signIn<T>(this SignInManager<T> m, T user, bool persist = false) where T : class
        => m.SignInAsync(user, persist);

    public static Task signOut<T>(this SignInManager<T> m) where T : class
        => m.SignOutAsync();
}

public static class RoleManagerExtensions
{
    public static Task<IdentityResult> create<T>(this RoleManager<T> m, T role) where T : class
        => m.CreateAsync(role);

    public static Task<bool> exists<T>(this RoleManager<T> m, string role) where T : class
        => m.RoleExistsAsync(role);
}
