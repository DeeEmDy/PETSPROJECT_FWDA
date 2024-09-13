using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.PetDtos;
using api.Dtos.UserDtos;
using api.Models;


namespace api.Mappers
{
    public static class PetMapper
    {
        // Método para convertir Pet a PetDto
        public static PetDto ToDto(this Pet pet)
        {
            return new PetDto
            {
                Id = pet.Id,
                Name = pet.Name,
                Animal = pet.Animal, 
                UserId = pet.UserId,
                User = pet.User != null ? new UserDto
                {
                    Id = pet.User.Id,
                    FirstName = pet.User.FirstName,
                    LastName = pet.User.LastName,
                    Age = pet.User.Age
                } : null
            };
        }

        // Método para convertir CreatePetRequestDto a Pet
        public static Pet ToPetFromCreateDto(this CreatePetRequestDto createPetRequest)
        {
            return new Pet
            {
                Name = createPetRequest.Name,
                Animal = createPetRequest.Animal,
                UserId = createPetRequest.UserId,
            };
        }

        // Método para convertir CreatePetRequestForUsersNPets a Pet
        public static Pet ToPetFromCreateDto(this CreatePetRequestForUsersNPets createPetRequest)
        {
            return new Pet
            {
                Name = createPetRequest.Name,
                Animal = createPetRequest.Animal,
                // El campo UserId se omite aquí ya que el usuario se asocia en el controlador al crear la el usuario primero y luego asociarle la mascota.
            };
        }
    }
}