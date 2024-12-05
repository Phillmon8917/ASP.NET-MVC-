using BestStore.Models;
using BestStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Globalization;

namespace BestStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IConfiguration configuration;
        private readonly IEmailSender emailSender;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration, IEmailSender emailSender)  //IConfiguration Is to read the sendername and email in app settings
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.emailSender = emailSender;
        }
        public IActionResult Register()
        {
            //Checking if the user is already registered
            if(signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            //Checking if the user is authenticated or not again
            if(signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(registerDto);
            }

            //Create a user

            var user = new ApplicationUser()
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                UserName = registerDto.Email,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                Address = registerDto.Address,
                CreatedAt = DateTime.Now,
            };

            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                //Successful registation of the user
                await userManager.AddToRoleAsync(user, "client");

                //sign in the new user
                await signInManager.SignInAsync(user, false);

                return RedirectToAction("Index", "Home");
            }

            // registation failed and show registration errors
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(registerDto);
        }

        //Logout function
        public async Task<IActionResult> Logout()
        {
            if (signInManager.IsSignedIn(User))
            {
                await signInManager.SignOutAsync();
            }

            return RedirectToAction("Index", "Home");
        }

        //Directing to the Login view
        public IActionResult Login()
        {
            //checking if the user is authenticated
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        //Login Fucntion
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            //check if the user is authenticated 
            if(signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            //Check if the submitted data is valid or not
            if(!ModelState.IsValid)
            {
                return View(loginDto);
            }

            //authenticating the user
            var result = await signInManager.PasswordSignInAsync(loginDto.Email , loginDto.Password,
                loginDto.RememberMe, false); /* The false at the end is to allow user to enter 
                                               as many incorrect passwords without blocking account */

            //If user authenticated succesfully
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid login attempt. Please review your password before retrying";
            }

            return View(loginDto);
        }

        [Authorize] //This makes the page to be available to users loged in
        public async Task<IActionResult> Profile()
        {
            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var profileDto = new ProfileDto()
            {
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Email = appUser.Email ?? "",
                PhoneNumber = appUser.PhoneNumber,
                Address = appUser.Address,
            };

            return View(profileDto);
        }

        [Authorize] //This makes the page to be available to users loged in
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileDto profileDto)
        {
            //Check if the submitted data is valid or not
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Please fill all the required fields with valid values";
                return View(profileDto);
            }

            // Get the current user
            var appUser = await userManager.GetUserAsync (User);
            if ( appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            //Update the user Profile
            appUser.FirstName = profileDto.FirstName;
            appUser.LastName = profileDto.LastName;
            appUser.UserName = profileDto.Email;
            appUser.Email = profileDto.Email;
            appUser.PhoneNumber = profileDto.PhoneNumber;
            appUser.Address = profileDto.Address;

            var results = await userManager.UpdateAsync (appUser);

            if (results.Succeeded)
            {
                ViewBag.SuccessMessage = "Profile updated succesfully";
            }
            else
            {
                ViewBag.ErrorMessage = "Unable to update the profile: " + results.Errors.First().Description;
            }

            return View(profileDto);
        }

        /* Notifies the user that access denied but alternatively i could have redirected the user to home page. Its just preference */
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        public IActionResult Password()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Password(PasswordDto passwordDto)
        {
            if (!ModelState.IsValid)
            { 
                return View();
            }

            var appUser = await userManager.GetUserAsync(User);
            if( appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            //Update the user password
            var results = await userManager.ChangePasswordAsync(appUser, passwordDto.CurrentPassword, passwordDto.NewPassword);
            if (results.Succeeded)
            {
                ViewBag.SuccessMessage = "Password updated successfully";
            }
            else 
            {
                ViewBag.ErrorMessage = "Error: " + results.Errors.First().Description;
            }
            return View();
        }

        public IActionResult ForgotPassword()
        {
            if(signInManager.IsSignedIn(User))
            {
               return RedirectToAction("Index", "Home");
            }

            return View();
        }

        //Remember we didnt provide a model for this
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(EmailViewModel model)
        {
            #region FirstApproach to sending an email
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var appUser = await userManager.FindByEmailAsync(model.Email);
            if (appUser != null)
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                var token = await userManager.GeneratePasswordResetTokenAsync(appUser);
                var resetUrl = Url.ActionLink("ResetPassword", "Account", new { token }) ?? "URL Error";
                string userName = textInfo.ToTitleCase(appUser.FirstName.ToLower()) + " " + textInfo.ToTitleCase(appUser.LastName.ToLower());
                var subject = "Password Reset";
                var message = "Dear " + userName + ",\n\nYou can reset your password using the following link:\n\n" + resetUrl + 
                              "\n\nWarm regards,\nBestore Service Center";

                await emailSender.SendEmailAsync(model.Email, subject, message);

                ViewBag.SuccessMessage = "Please check your email for the password reset link.";
            }
            else
            {
                ViewBag.EmailError = "User not found.";
            }
            
            #endregion

           
            return View();
        }



        //Reset Password Method
        public IActionResult ResetPassword(string? token)
        {
            //Checking if the user is signed in
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        //updating the password
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string? token, PasswordResetDto model)
        {
            //Checking if the user is signed in
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ViewBag.ErrorMessage = "Token not valid!";
                return View(model);
            }

            var result = await userManager.ResetPasswordAsync(user,token, model.Password);

            if (result.Succeeded)
            {
                ViewBag.SucessMessage = "Password reset successfully";
            }
            else
            {
                //Adding the general errors 
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
    }
}
