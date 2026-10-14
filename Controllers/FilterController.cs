using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ProfanityFilter.Common;
using ProfanityFilter.DTOs.Request;
using ProfanityFilter.DTOs.Response;
using ProfanityFilter.Services;


namespace ProfanityFilter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilterController : ControllerBase
    {
        private readonly IProfanityFilterService _filterService;
        public FilterController(IProfanityFilterService filterService)
        {

            _filterService = filterService;

        }

        [HttpPost("Create/profanity")]
        public async Task<ActionResult<CreateWordResponse>> CreateProfanityWord([FromBody] CreateWordRequest? request)
        {

            var (result, timeMs) = await _filterService.CreateProfaniyWord(request.Word, request.LanguageId);
            
            if (!result.IsSuccess)
            {

                return BadRequest(new
                {
                    Error = $"You made a mistake, the mistake is: {result.Error}",
                    Time = $"{timeMs:F4} ms"
                });
            }

            return Ok(new
            {
                Data = result.Value,
                TimeEjecution = $"{timeMs:F4} ms"
            });
        }


        [HttpGet("Words")]

        public async Task<ActionResult> HasProfanityWord([FromQuery] string? userText)
        {
            var (result, timeMs) = await _filterService.HasProfanity(userText);

            if (!result.IsSuccess)
            {
                return BadRequest(new { 
                    Error = $"You committed a foul: {result.Error}",
                    Time = $"{timeMs:F4} ms"
                });
            }

            return Ok(new { 
                Data = result.Value,
                TimeEjecution = $"{timeMs:F4} ms"
            });
        }

        [HttpPost("Create/exception")]
        public async Task<ActionResult<CreateWordResponse>> CreateExceptionWord([FromBody] CreateWordRequest? request)
        {

            var (result, timeMs) = await _filterService.CreateExceptionWord(request.Word, request.LanguageId);

            if (!result.IsSuccess)
            {

                return BadRequest(new
                {
                    Error = $"You made a mistake, the mistake is: {result.Error}",
                    Time = $"{timeMs:F4} ms"
                });
            }

            return Ok(new
            {
                Data = result.Value,
                TimeEjecution = $"{timeMs:F4} ms"
            });
        }
    }
}
