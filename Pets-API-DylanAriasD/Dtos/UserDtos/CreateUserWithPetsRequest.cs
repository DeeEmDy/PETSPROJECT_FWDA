using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using api.Dtos.PetDtos;

namespace api.Dtos.UserDtos
{
     public class CreateUserWithPetsRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        
        // Lista de identificadores de mascotas que se van a asignar al usuario
        public List<int> PetIds { get; set; } = new List<int>();
    }
}
