using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTO
{
    public class RegisterDto
    {
        [Required]
        public string Username { get; set; }

        [Required] 
        public string Email { get; set; }

        [Required]
        [DataType(dataType: DataType.Password)]
        [StringLength(250, MinimumLength = 4)]
        public string Password { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

    }
}
