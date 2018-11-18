using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantageLibrary.Lti;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Areas.Identity.Pages.Account.Manage
{
    public class PeopleModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public PeopleModel(ApplicationDbContext context, 
            UserManager<AdvantagePlatformUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public List<Person> People { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _context.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            People = user.People?.ToList();

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            foreach (var person in People)
            {
                _context.Attach(person).State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();

            return Page();
        }

        /// <summary>
        /// Return an array of <see cref="Role"/> from a comma separated list.
        /// </summary>
        /// <param name="rolesString">Comma separate list of <see cref="Role"/> names.</param>
        /// <returns></returns>
        public static Role[] ParsePersonRoles(string rolesString)
        {
            var roles = new List<Role>();
            foreach (var roleString in rolesString.Split(","))
            {
                if (Enum.TryParse<Role>(roleString, out var role))
                {
                    roles.Add(role);
                }
            }

            return roles.ToArray();
        }
    }
}