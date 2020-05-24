using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Cwiczenia5.DTOs.Requests
{
    public class EnrollStudentRequest
    {
        [RegularExpression("^s[0-9]+$")]
        [Required(ErrorMessage ="Must provide index number")]
        public string IndexNumber { get; set; }

        [Required(ErrorMessage ="Must provide e-mail address")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Must provide first name")]
        [MaxLength(10)]
        public string FirstName { get; set; }

        [Required(ErrorMessage ="Must provide last name")]
        [MaxLength(255)]
        public string LastName { get; set; }

        [Required(ErrorMessage ="Must provide birthdate")]
        public DateTime? Birthdate { get; set; }

        [Required(ErrorMessage ="Must provide studies (Id)")]
        public string Studies { get; set; }
    }
}
