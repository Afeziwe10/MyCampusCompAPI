using Microsoft.AspNetCore.Mvc;
using MyCampusComp.Services;

namespace MyCampusComp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly OpenRouterService _openRouterService;

        public AIController(OpenRouterService openRouterService)
        {
            _openRouterService = openRouterService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask(
            [FromBody] AIRequest request)
        {
            // Validate message
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new
                {
                    error = "Message cannot be empty."
                });
            }

            try
            {
                var response =
                    await _openRouterService.GetResponseAsync(
                        request.Message
                    );

                return Ok(new
                {
                    response
                });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new
                {
                    error =
                        "The AI provider could not be reached.",

                    details = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "An unexpected error occurred.",
                    details = ex.Message
                });
            }
        }
    }

    public class AIRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}