using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Dtos.PetDtos
{
    public class CreatePetRequestForUsersNPets
    {
        public string Name { get; set; }
        public string Animal { get; set; }
        
        // El campo UserId se elimina aquí
    }
}