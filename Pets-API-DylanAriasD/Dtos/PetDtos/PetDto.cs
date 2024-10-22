using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using api.Dtos.UserDtos;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.PetDtos
{
    public class PetDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; }

        [Required]
        public string Animal { get; set; }

        public int? UserId { get; set; }
        public UserDto? User { get; set; }
    }
}