using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.PetDtos;

namespace api.Dtos.UserDtos
{
    public class CreateUserNPetAssociated
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public List<CreatePetRequestDto> Pets { get; set; } // Lista de mascotas a asociar al registro del usuario.
    }
}