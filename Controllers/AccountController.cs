using Microsoft.AspNetCore.Mvc;

public class AccountController : Controller
{
    private const string SessionKey = "IsAdmin";

    [HttpGet]
    public IActionResult Login(string returnUrl = "/")
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password, string returnUrl = "/")
    {
        // Demo: change to your secure check in production
        if (username == "admin" && password == "Password123!")
        {
            HttpContext.Session.SetString(SessionKey, "true");
            return LocalRedirect(returnUrl);
        }

        ViewData["ReturnUrl"] = returnUrl;
        ViewBag.Error = "Invalid credentials";
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Remove(SessionKey);
        return RedirectToAction("Login");
    }

    // helper to check admin in views/controllers
    public static bool IsAdmin(HttpContext ctx)
    {
        return ctx.Session.GetString(SessionKey) == "true";
    }
}
