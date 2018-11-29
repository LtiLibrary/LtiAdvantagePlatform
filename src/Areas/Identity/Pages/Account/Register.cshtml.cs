using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantage.Lti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

                    // Create the basic platform objects for the new user
                    user.Platform = await CreatePlatformAsync(_context, Request);
                    user.Course = await CreateCourseAsync(_context);
                    user.People = new List<Person>();
                    foreach (var person in await CreatePeopleAsync(_context))
                    {
                        user.People.Add(person);
                    }
                    await _userManager.UpdateAsync(user);
                    // Done creating the basic objects

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

        /// <summary>
        /// Create a default platform.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private static async Task<Platform> CreatePlatformAsync(ApplicationDbContext context, HttpRequest request)
        {
            var nameGenerator = new PersonNameGenerator();
            var contact = nameGenerator.GenerateRandomFirstName().Substring(0, 1).ToLowerInvariant()
                          + nameGenerator.GenerateRandomLastName().ToLowerInvariant();

            var platform = new Platform
            {
                ContactEmail = $"{contact}@example.edu",
                Description = "Auto generated platform",
                Guid = $"{request.Host}",
                Name = ".NET Core Test Platform",
                ProductFamilyCode = "LTI Advantage Platform",
                Url = $"{request.Scheme}://{request.Host}/",
                Version = "1.0"
            };
            await context.Platforms.AddAsync(platform);
            await context.SaveChangesAsync();

            return platform;
        }

        /// <summary>
        /// Create a default course.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<Course> CreateCourseAsync(ApplicationDbContext context)
        {
            var placeGenerator = new PlaceNameGenerator();
            var course = new Course
            {
                Name = $"The People of {placeGenerator.GenerateRandomPlaceName()}",
            };
            course.SisId = course.GetHashCode().ToString();
            await context.Courses.AddAsync(course);
            await context.SaveChangesAsync();

            return course;
        }

        /// <summary>
        /// Create a default set of people that will be members of a course.
        /// </summary>
        /// <param name="context">The db context for Person.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<Person>> CreatePeopleAsync(ApplicationDbContext context)
        {
            var nameGenerator = new PersonNameGenerator();

            var teacher = new Person
            {
                FirstName = nameGenerator.GenerateRandomFirstName(),
                LastName = nameGenerator.GenerateRandomLastName(),
                Roles = string.Join(", ", Role.ContextInstructor.ToString(), Role.InstitutionFaculty.ToString()),
            };
            teacher.SisId = teacher.GetHashCode().ToString();
            teacher.Username = $"{teacher.FirstName.Substring(0, 1)}{teacher.LastName}".ToLowerInvariant();
            await context.People.AddAsync(teacher);

            var student = new Person
            {
                FirstName = nameGenerator.GenerateRandomFirstName(),
                LastName = nameGenerator.GenerateRandomLastName(),
                Roles = string.Join(", ", Role.ContextLearner.ToString(), Role.InstitutionLearner.ToString()),
            };
            student.SisId = teacher.GetHashCode().ToString();
            student.Username = $"{teacher.FirstName.Substring(0, 1)}{teacher.LastName}".ToLowerInvariant();
            await context.People.AddAsync(student);

            await context.SaveChangesAsync();

            return new List<Person> { teacher, student };
        }
    }
}
