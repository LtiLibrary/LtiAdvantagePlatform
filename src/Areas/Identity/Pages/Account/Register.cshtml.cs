using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantageLibrary.Lti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RandomNameGeneratorLibrary;

namespace AdvantagePlatform.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<AdvantagePlatformUser> _signInManager;
        private readonly UserManager<AdvantagePlatformUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            ApplicationDbContext context,
            UserManager<AdvantagePlatformUser> userManager,
            SignInManager<AdvantagePlatformUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new AdvantagePlatformUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Create a Platform for the new user and generate keys
                    var platform = CreatePlatform(Request, user);
                    await _context.Platforms.AddAsync(platform);

                    // Create a Course for the new user
                    var course = CreateCourse(user);
                    await _context.Courses.AddAsync(course);

                    // Create a default set of people for the course
                    await CreatePeopleAsync(_context, user);

                    // Save the platform, course, student, and teacher
                    await _context.SaveChangesAsync();

                    // Attach the platform, course, student, and teacher to local user
                    user.Platform = platform;
                    user.Course = course;
                    await _userManager.UpdateAsync(user);

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        public static Platform CreatePlatform(HttpRequest request, AdvantagePlatformUser user)
        {
            var platform = new Platform
            {
                User = user,
                ContactEmail = user.Email,
                Description = "Auto generated platform",
                Guid = $"{request.Host}",
                Name = ".NET Core Test Platform",
                ProductFamilyCode = "LTI Advantage Platform",
                Url = $"{request.Scheme}://{request.Host}/"
            };
            return platform;
        }

        public static Course CreateCourse(AdvantagePlatformUser user)
        {
            var placeGenerator = new PlaceNameGenerator();
            var course = new Course
            {
                Name = $"The People of {placeGenerator.GenerateRandomPlaceName()}",
                User = user
            };
            course.SisId = course.GetHashCode().ToString();
            return course;
        }

        /// <summary>
        /// Create a default set of people that will be members of a course.
        /// </summary>
        /// <param name="context">The db context for Person.</param>
        /// <param name="user">The application user.</param>
        /// <returns></returns>
        public static async Task CreatePeopleAsync(ApplicationDbContext context, AdvantagePlatformUser user)
        {
            // Already have the default number of people?
            if (user.People.Count >= 2)
            {
                return;
            }

            var nameGenerator = new PersonNameGenerator();

            if (!user.People.Any(p => p.Roles.Contains(Role.ContextInstructor.ToString())))
            {
                var person = new Person
                {
                    FirstName = nameGenerator.GenerateRandomFirstName(),
                    LastName = nameGenerator.GenerateRandomLastName(),
                    Roles = string.Join(",", new {Role.ContextInstructor, Role.InstitutionFaculty}),
                    User = user
                };
                person.SisId = person.GetHashCode().ToString();
                await context.People.AddAsync(person);
                user.People.Add(person);
                context.Update(user);
            }

            
            if (!user.People.Any(p => p.Roles.Contains(Role.ContextInstructor.ToString())))
            {
                var person = new Person
                {
                    FirstName = nameGenerator.GenerateRandomFirstName(),
                    LastName = nameGenerator.GenerateRandomLastName(),
                    Roles = string.Join(",", new {Role.ContextLearner, Role.InstitutionLearner}),
                    User = user
                };
                person.SisId = person.GetHashCode().ToString();
                await context.People.AddAsync(person);
                user.People.Add(person);
                context.Update(user);
            }

            await context.SaveChangesAsync();
        }
    }
}
