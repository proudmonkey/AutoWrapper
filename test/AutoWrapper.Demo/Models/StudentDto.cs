using System;
using System.ComponentModel.DataAnnotations;

namespace AutoWrapper.Simple.Models
{
    public class StudentDto
    {

        [Required]
        public string Name { get; set; }

        public DateTime? Birthday { get; set; }

        public int Age { get; set; }
    }
}
