using Microsoft.AspNetCore.Mvc;
using PlanIT.BusinessLogic.Interfaces;

namespace PlanIT.WebApi.Controllers;

[ApiController]
[Route("api/lugares")]
public class LugaresController : ControllerBase
{
    private readonly ILugarService _service;

    public LugaresController(ILugarService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var lugares = await _service.ObtenerLugares();
        return Ok(lugares);
    }
}
