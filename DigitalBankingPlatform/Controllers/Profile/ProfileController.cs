using ApiMessage;
using Application.DTOs.Profile;
using Application.Interfaces.IProfileService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalBankingPlatform.Controllers.Profile;

[ApiController]
[Route("api/v1/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    // GET api/v1/profile
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var customerId = GetCustomerId();
            var profile    = await _profileService.GetProfileAsync(customerId);
            return profile is null
                ? NotFound(ApiResponse<ProfileDto>.Fail("Profile not found."))
                : Ok(ApiResponse<ProfileDto>.Ok(profile));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<ProfileDto>.Fail(ex.Message)); }
    }

    // PUT api/v1/profile
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        try
        {
            var customerId = GetCustomerId();
            await _profileService.UpdateProfileAsync(customerId, dto);
            return Ok(ApiResponse<bool>.Ok(true, "Profile updated."));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<bool>.Fail(ex.Message)); }
    }

    private Guid GetCustomerId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new Exception("Invalid token."));
}
