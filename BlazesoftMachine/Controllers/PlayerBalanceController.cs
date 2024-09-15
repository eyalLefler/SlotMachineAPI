using BlazesoftMachine.Model.Requests;
using BlazesoftMachine.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlazesoftMachine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerBalanceController : Controller
    {
        private readonly PlayerBalanceService _playerBalanceService;

        public PlayerBalanceController(PlayerBalanceService playerBalanceService) => _playerBalanceService = playerBalanceService;


        [HttpPost("update")]
        public async Task<IActionResult> UpdateBalance([FromBody] UpdateBalanceRequest request)
        {
            try
            {
                await _playerBalanceService.UpdateBalanceAsync(request.PlayerId, request.Amount);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
