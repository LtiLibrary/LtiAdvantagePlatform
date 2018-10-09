using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using LtiAdvantageLibrary.NetCore.Lti;
using LtiAdvantageLibrary.NetCore.Utilities;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;

namespace AdvantagePlatform.Pages
{
    public class ResourceLinkModel : PageModel
    {
        public string IdToken { get; set; }

        public void OnGet()
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var request = new LtiResourceLinkRequest
            {
                MessageType = LtiConstants.LtiResourceLinkRequestMessageType,
                Version = LtiConstants.Version,
                DeploymentId = "1",
                ResourceLink = new ResourceLinkClaimValueType
                {
                    Description = "This is a sample toole",
                    Id = "1",
                    Title = "Sample Tool"
                },
                GivenName = "Shanelle",
                MiddleName = "Walker",
                FamilyName = "West",
                Name = "Dr. Shanelle Walker West",
                Picture = "http://example.org/Shanelle.org",
                Locale = CultureInfo.CurrentUICulture.Name,
                Context = new ContextClaimValueType
                {
                    Id = "1",
                    Label = "Test Course",
                    Title = "This is a test course",
                    Type = new[] {ContextType.CourseSection}
                },
                Lis = new LisClaimValueType
                {
                    PersonSourcedId = "sis12345",
                    CourseOfferingSourcedId = "off12345",
                    CourseSectionSourcedId = "sec12345"
                },
                Roles = new[] {Role.ContextLearner, Role.InstitutionStudent, Role.ContextMentor},
                LaunchPresentation = new LaunchPresentationClaimValueType
                {
                    DocumentTarget = DocumentTarget.iframe,
                    Height = 600,
                    Width = 800,
                    ReturnUrl = "http://localhost:44338"
                },
                RoleScopeMentor = new[] {"2"}
            };

            request.UserId = Guid.NewGuid().ToString("N");
            request.Platform = new PlatformClaimValueType
            {
                ContactEmail = "andy@andyfmiller.com",
                Description = "Sample AspNetCore Platform",
                Guid = "1",
                Name = "Advantage Platform",
                ProductFamilyCode = "LtiAdvantageLibrary",
                Url = "https://localhost:44330",
                Version = "1.0"
            };
            request.Custom = new CustomClaimValueType {{"myCustomValue", "123"}};


            const string platformPrivateKey = 
@"-----BEGIN RSA PRIVATE KEY----- 
MIIEpQIBAAKCAQEAxwNk5GjdXmb4iFWOe/LfkWYfuzUhU+rHef4FziWJq31RZUkd 
Kjaul0MyUwPZ/u2Gpzpdr1hNSa3Kmtj4BQk8IUgveVAyvNxTMinsEm6hSjihQHnM 
5LLWGM804uZ8ylS0Rt4ne31hIQSOnxBp6LXjUvxdavl5Zp+tt5aF+5zxE0Viu7s4 
oqwEdr25kCdo/H4zBadLGCmx1IFFYqd8voEMAILwP02jbuOSeSxK86b2uxLl4BZb 
9qL1Itd2+Febtt8PW4vVkcl7jWXQUBhQRn1LGNRmKF4nXZVVAYu1grC4jXqIYX0r 
Y9BuQAgR3W1B+aBWfPCxkOFyCH5re6lNA+OHoQIDAQABAoIBAQCIKqlYout8EQZR 
+gAgjMAtcuuZIrQn/8b2kDkWoJZ+sKMFoy4CWSHCO2buZ/87Lvfn6cZ6F/fhxriO 
lI+2mmf6+bIJ1jaxBCskcoyk2+8donuqauwnOrnEhdnQuhV+NtCWgKARHWtctQqE 
iS193R2qBSRB8mn/Lpgkuif4TZyhKq1nrA1DUc6HSGzDjw19Q+bPx/9Iep9ZkFms 
0HoqQY494Anf707JCmoRFRCDPqP/OT4sEPY9ulJ+jbhKeW0cj0lkmr/F5Ow/mF1v 
gwbgizcbQEwqmyMv19Qh7Cq/HZ7ei5B8r3+eBslM/G1Lr4OQ+Q8S7ZkdfFEn/Y3U 
UVQc8+ABAoGBAPYdKkWMbe8l+JBuF0Fgxd035xP1+WttlZGvuXQ3Ry6/WczuC9V1 
dfGojWFycC1f+9W/80xwzwgOf1mWjykA4nX/2anKz4xCrvlyWpBdgMWVBxDFB1+I 
3XIoqDMMB+wTnIO4pSzPqXF4Q78y2AUHRbZtyWuPGjdHWiCXWJFLg0ShAoGBAM8B 
4laa6MYM5q8bkTamXAlN2hJC/8QexXShSEDz0GAHj3UJfMZrVB0ge+r7S7KMhIyR 
46SDjuEjBtR+R05IDP7pXZJtnqyxVMHqJPSxXM0Xpy1xOjawiMCTwBHVY0G6UHqe 
ZW9ItGqG5AMmX6VLTC/OxjJDuJxThhQQEZavFmMBAoGBALQzPI05uhB9O9b8VJiw 
84767H2n/xySWw4VnPMjukHXJzguH4oGe/oZ0JLGCK6gdbLW3ZxdiKi8fLAq1d1O 
4YFGeKeicH6hIJ5SBU9otu10JX041iI0AvggHq9poq9O2K11V7NMxSLozKnE33gn 
D/r3vZ+8YpSQ8MOmLNoNcSqhAoGBALAmkkvQFjQx1js74h33AmqIbQENMkfZR30v 
oSRkVPTiehjFsf/GZRVEBy62GHwfRsu3eVcM/BBole9MZbEfbq+cNZvc3CFn5Q/+ 
BuSR6L49Wm045KiVIEl/cPm783KC8Ojm0LoZzCy6DF0B0nH92R71fYJYTkf4aye3 
ZMAszYgBAoGAInSaczEujEG6n/dRLo65z+iVJe7EtdRdsst6Lq7KXlUs6uGp2eQe 
xg4nOW7YS1TOt0Do4JkWXvjbcl8X9T/903MppeHGUMalv1NQPjj98TnjeO4x36hp 
fKi1lhYwrl9ObqBjoPkVmP01WGy1tfPkFswvZimOXq+Y53/XVfqs8YM= 
-----END RSA PRIVATE KEY-----";

            var key = new RsaSecurityKey(RsaHelper.PrivateKeyFromPemString(platformPrivateKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

            request.Nonce = LtiResourceLinkRequest.GenerateCryptographicNonce();

            var token = new JwtSecurityToken(
                issuer: Request.GetDisplayUrl(),
                audience:"https://localhost:44308/Tool",
                claims: request.Claims,
                expires: DateTime.UtcNow.AddHours(1.0),
                signingCredentials: creds
                );

            IdToken = new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}