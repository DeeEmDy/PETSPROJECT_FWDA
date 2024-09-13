using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using System.Text.Json.Serialization;

namespace api.Dtos.PetDtos
{
    public class CreatePetRequestDto
    {
        public string Name { get; set; }
        public string Animal { get; set; }
        
        [JsonIgnore]
        public int? UserId { get; set; } // Ignorar en Swagger
        
    }
}