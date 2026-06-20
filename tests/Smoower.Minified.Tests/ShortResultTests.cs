using Microsoft.AspNetCore.Mvc;
using Smoower.Minified.AspNetCore;
using Xunit;

namespace Smoower.Minified.Tests;

public class ShortResultTests
{
    // Ctl's helpers are protected (so MVC never routes them as actions); expose them
    // through a concrete controller to assert the status/shape.
    private sealed class C : Ctl
    {
        public IActionResult CallNf() => nf();
        public IActionResult CallNc() => nc();
        public IActionResult CallUn() => un();
        public IActionResult CallForb() => forb();
        public IActionResult CallBad() => bad();
        public IActionResult CallBadObj() => bad(new { x = 1 });
        public IActionResult CallUnp() => unp("nope");
    }

    [Fact] public void Nf_Is404() => Assert.IsType<NotFoundResult>(new C().CallNf());
    [Fact] public void Nc_Is204() => Assert.IsType<NoContentResult>(new C().CallNc());
    [Fact] public void Un_Is401() => Assert.IsType<UnauthorizedResult>(new C().CallUn());
    [Fact] public void Forb_IsForbid() => Assert.IsType<ForbidResult>(new C().CallForb());
    [Fact] public void Bad_Is400() => Assert.IsType<BadRequestResult>(new C().CallBad());
    [Fact] public void BadObj_Is400WithBody() => Assert.IsType<BadRequestObjectResult>(new C().CallBadObj());

    [Fact]
    public void Unp_Is422WithErrorBody()
    {
        var r = Assert.IsType<UnprocessableEntityObjectResult>(new C().CallUnp());
        Assert.Equal(422, r.StatusCode);
        Assert.Equal("nope", r.Value!.GetType().GetProperty("error")!.GetValue(r.Value));
    }
}
