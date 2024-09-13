using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.UserDtos;
using api.Models;

namespace api.Mappers
{
    public static class UserMapper
    {
        public static UserDto ToDto(this User user){
            return new UserDto {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Age = user.Age,
                PetList = user.Pets.Select(PetMapper.ToDto).ToList()
            };
        }

        // Método para convertir CreateUserRequestDto a User
        public static User ToUserFromCreateDto(this CreateUserRequestDto createUserRequest){ 
            return new User{
                FirstName = createUserRequest.FirstName,
                LastName = createUserRequest.LastName,
                Age = createUserRequest.Age,
                Pets = new List<Pet>() //
            };
        }

        // Nueva sobrecarga del método para CreateUserWithPetsRequest
        public static User ToUserFromCreateDto(this CreateUserWithPetsRequest createUserWithPetsRequest){ 
            return new User{
                FirstName = createUserWithPetsRequest.FirstName,
                LastName = createUserWithPetsRequest.LastName,
                Age = createUserWithPetsRequest.Age,
                Pets = new List<Pet>() // Inicialmente vacío, las mascotas se agregan en el controlador
            };
        }

        // Método para convertir CreateUserNPetAssociated a User
        public static User ToUserFromCreateDto(this CreateUserNPetAssociated createUserWithPetsRequest)
        {
            return new User
            {
                FirstName = createUserWithPetsRequest.FirstName,
                LastName = createUserWithPetsRequest.LastName,
                Age = createUserWithPetsRequest.Age,
                Pets = new List<Pet>() // Inicialmente vacío, las mascotas se agregan en el controlador
            };
        }

    }
}
