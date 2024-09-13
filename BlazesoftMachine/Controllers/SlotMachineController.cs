using BlazesoftMachine.Model.Requests;
using BlazesoftMachine.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlazesoftMachine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SlotMachineController : Controller
    {
        private readonly SlotMachineService _slotMachineService;

        public SlotMachineController(SlotMachineService slotMachineService)
        {
            _slotMachineService = slotMachineService;
        }

        [HttpPost("spin")]
        public async Task<IActionResult> Spin([FromBody] SpinRequest request)
        {
            try
            {
                var result = await _slotMachineService.SpinAsync(request.PlayerId, request.BetAmount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("balance/update")]
        public async Task<IActionResult> UpdateBalance([FromBody] UpdateBalanceRequest request)
        {
            try
            {
                await _slotMachineService.UpdateBalanceAsync(request.PlayerId, request.Amount);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

    }
}
