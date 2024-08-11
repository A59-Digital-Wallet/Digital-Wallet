using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Wallet.Data.Models;
using Wallet.DTO.Request;
using Wallet.Services.Contracts;
using Wallet.Services.Implementations;
using Microsoft.EntityFrameworkCore;


namespace Digital_Wallet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly IEmailSender emailSender;
        private readonly IConfiguration _configuration;
        private readonly TwilioVerifyService _verifyService;

        public UserController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailSender emailSender, IConfiguration configuration, TwilioVerifyService verifyService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailSender = emailSender;
            _configuration = configuration;
            _verifyService = verifyService;
        }

        [HttpPost("add-user")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var user = new AppUser()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.UserName,
                PasswordHash = model.Password,
                PhoneNumber = model.PhoneNumber,

            };

            var result = await userManager.CreateAsync(user, user.PasswordHash!);
            if (result.Succeeded)
            {
                await this.userManager.AddToRoleAsync(user, "User");
                // Generate a 6-digit code
                var confirmationCode = new Random().Next(100000, 999999).ToString();
                user.EmailConfirmationCode = confirmationCode;
                user.EmailConfirmationCodeGeneratedAt = DateTime.UtcNow;

                // Update user with confirmation code
                await userManager.UpdateAsync(user);

                // Send the code via email
                await emailSender.SendEmail(
                    "Email Confirmation",
                    user.Email,
                    user.FirstName,
                    $"Hello, please confirm your email by entering the following code: {confirmationCode}"
                );

                var phoneVerificationSent = await _verifyService.SendVerificationCodeAsync(user.PhoneNumber);
                if (!phoneVerificationSent)
                {
                    return BadRequest("Failed to send phone verification code.");
                }

                if (userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return Ok("Confirmation codes sent. Please check your email and SMS.");
                }
                else
                {
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return Ok(result);
                }
            }

            return BadRequest(result);


        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest("Invalid email address.");
            }

            // Check if the code matches and is within a valid timeframe (e.g., 10 minutes)
            if (user.EmailConfirmationCode == model.Code &&
                user.EmailConfirmationCodeGeneratedAt.HasValue &&
                (DateTime.UtcNow - user.EmailConfirmationCodeGeneratedAt.Value).TotalMinutes <= 10)
            {
                user.EmailConfirmed = true;
                user.EmailConfirmationCode = null;
                user.EmailConfirmationCodeGeneratedAt = null;
                await userManager.UpdateAsync(user);

                return Ok("Email confirmed successfully.");
            }

            return BadRequest("Invalid or expired confirmation code.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await this.userManager.FindByEmailAsync(model.Email);
            if (user != null && await this.userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = GenerateJwtToken(user);
                return Ok(new { token });
            }

            return Unauthorized("Invalid email or password.");
        }
        [HttpPost("verify-phone")]
        public async Task<IActionResult> VerifyPhone(string phoneNumber, string code)
        {
            var user = await this.userManager.Users.SingleOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var verified = await _verifyService.VerifyCodeAsync(phoneNumber, code);
            if (verified)
            {
                user.PhoneNumberConfirmed = true;
                await this.userManager.UpdateAsync(user);
                return Ok("Phone number verified successfully.");
            }

            return BadRequest("Invalid verification code.");
        }

        private async Task<string> GenerateJwtToken(AppUser user)
        {
            
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings["Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var roles = await this.userManager.GetRolesAsync(user);

            var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.UserData, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email) // Added claim for email address
            };

            claims.AddRange(roleClaims);
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

       
      

        


    }
}
