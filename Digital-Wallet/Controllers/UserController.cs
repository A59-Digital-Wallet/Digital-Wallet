﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;
using Wallet.Data.Models;
using Wallet.Services.Contracts;
using Wallet.Services.Implementations;

namespace Digital_Wallet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly IEmailSender emailSender;
        

        public UserController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailSender emailSender)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailSender = emailSender;
            //this.registerConfirmationModel = registerConfirmationModel;
        }

        [HttpPost("add-user")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var user = new AppUser()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
                PasswordHash = model.Password,
                PhoneNumber = model.PhoneNumber,

            };

            var result = await userManager.CreateAsync(user, user.PasswordHash!);
            if (result.Succeeded)
            {
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

                if (userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return Ok("Confirmation code sent. Please check your email.");
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





    }
}
