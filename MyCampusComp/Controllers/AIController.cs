using Microsoft.AspNetCore.Mvc;
using MyCampusComp.Services;

namespace MyCampusComp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly OpenAIService _openAIService;

        public AIController(OpenAIService openAIService)
        {
            _openAIService = openAIService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AIRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message cannot be empty.");
            }

            var response = await _openAIService.GetResponseAsync(request.Message);

            return Ok(new
            {
                response = response
            });
        }
    }

    public class AIRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}