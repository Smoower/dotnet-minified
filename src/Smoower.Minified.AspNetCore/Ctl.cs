using Microsoft.AspNetCore.Mvc;

namespace Smoower.Minified.AspNetCore;

// The smoower controller base. Previously `Ctl` was a using-alias for ControllerBase;
// it is now a real base class so it can expose short result helpers for the returns
// the query terminators (ok1/okl/created/...) don't cover - the hand-written guard
// and catch returns. These are INSTANCE methods, not extensions: an extension needs
// `this.`, and that prefix costs as many Claude tokens as the short name saves
// (NoContent() -> this.nc() nets zero). They are `protected` so MVC never mistakes
// them for actions. Status/behaviour are identical to the ControllerBase originals.
public abstract class Ctl : ControllerBase
{
    protected NotFoundResult nf() => NotFound();
    protected NoContentResult nc() => NoContent();
    protected UnauthorizedResult un() => Unauthorized();
    protected ForbidResult forb() => Forbid();
    protected BadRequestResult bad() => BadRequest();
    protected BadRequestObjectResult bad(object error) => BadRequest(error);
    protected UnprocessableEntityObjectResult unp(string error) => UnprocessableEntity(new { error });
}
