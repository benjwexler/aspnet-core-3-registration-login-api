
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Entities;
using WebApi.Services;


namespace WebApi.Controllers
{
  [Route("api/[controller]/[action]")]
  public class ExternalAuthController : Controller
  {
    private readonly DataContext _context;
    private IUserService _userService;
    private readonly Facebook _facebook;
    private static readonly HttpClient Client = new HttpClient();

    public ExternalAuthController(
      DataContext appDbContext,
      IUserService userService,
      IOptions<Facebook> facebook
    )
    {
      _userService = userService;
      _context = appDbContext;
      _facebook = facebook.Value;
    }

    // POST api/externalauth/facebook
    [HttpPost]
    public async Task<IActionResult> Facebook([FromBody]FacebookAuthViewModel model)
    {
      // 1.generate an app access token
      var appId = "2442240719384458";
      var appSecret = _facebook.AppSecret;
      var appAccessTokenResponse = await Client.GetStringAsync($"https://graph.facebook.com/oauth/access_token?client_id={appId}&client_secret={appSecret}&grant_type=client_credentials");
      var appAccessToken = JsonConvert.DeserializeObject<FacebookAppAccessToken>(appAccessTokenResponse);
      // 2. validate the user access token
      var userAccessTokenValidationResponse = await Client.GetStringAsync($"https://graph.facebook.com/debug_token?input_token={model.AccessToken}&access_token={appAccessToken.AccessToken}");
      var userAccessTokenValidation = JsonConvert.DeserializeObject<FacebookUserAccessTokenValidation>(userAccessTokenValidationResponse);

      if (!userAccessTokenValidation.Data.IsValid)
      {
        return BadRequest(new { message = "Sorry" });
        // return BadRequest(Errors.AddErrorToModelState("login_failure", "Invalid facebook token.", ModelState));
      }

      // 3. we've got a valid token so we can request user data from fb
      var userInfoResponse = await Client.GetStringAsync($"https://graph.facebook.com/v2.8/me?fields=id,email,first_name,last_name,name,gender,locale,birthday,picture&access_token={model.AccessToken}");
      var userInfo = JsonConvert.DeserializeObject<FacebookUserData>(userInfoResponse);
      var user = _userService.getFacebookUser(userInfo.Email);
      // 4. ready to create the local user account (if necessary) and jwt

      if (user == null)
      {
        var appUser = new User
        {
          FirstName = userInfo.FirstName,
          LastName = userInfo.LastName,
          Username = userInfo.Email,
          PasswordHash = null,
          PasswordSalt = null,
        };

        var result = _userService.Create(appUser, null, true);

        // if (!result.Succeeded) return new BadRequestObjectResult(Errors.AddErrorsToModelState(result, ModelState));

        await _context.SaveChangesAsync();
      }

      var existingUser = _userService.authenticateFacebookUser(userInfo.Email);
      if (existingUser == null)
        return BadRequest(new { message = "Couldn't authenticate" });
      return Ok(existingUser);
    }
  }
}