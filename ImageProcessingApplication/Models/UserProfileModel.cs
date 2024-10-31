using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ImageProcessingApplication.Areas.Api.Controllers
{
    public class UserProfileModel : IValidatableObject
    {
        [Required]
        public string UserName { get; set; }
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
        public string PasswordRepeat { get; set; }
        public string PreviousPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Password != PasswordRepeat)
            {
                yield return new ValidationResult("Passwords must match.", new[] { nameof(Password), nameof(PasswordRepeat) });
            }
        }
    }
}