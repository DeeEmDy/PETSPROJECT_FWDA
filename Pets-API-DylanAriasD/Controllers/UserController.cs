using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Mappers;
using api.Dtos.UserDtos;


namespace api.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public UserController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(){
            var users = await _context.Users.Include(user => user.Pets).ToListAsync();
            var usersDto = users.Select(users => users.ToDto());
            return Ok(usersDto);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> getById([FromRoute] int id){
            var user =  await _context.Users.Include(user => user.Pets).FirstOrDefaultAsync(u => u.Id == id);
            if(user == null){ //Si el usuario no existe, devolver un error 404.
                return NotFound(); //Devolver un error 404.
            }
            return Ok(user.ToDto()); //Devolver un 200 Ok y la información del usuario.
        }

        //Crear un nuevo usuario.
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequestDto userDto){
            var userModel = userDto.ToUserFromCreateDto(); //Le asignamos al modelo del Usuario, lo que contiene el usuario de dto. Ya que lo que se envia a la base de datos es el modelo User.
            await _context.Users.AddAsync(userModel); //Agregar el usuario a la base de datos.
            await _context.SaveChangesAsync(); //Guardar los cambios en la base de datos.
            return CreatedAtAction(nameof(getById), new { id = userModel.Id}, userModel.ToDto()); //Devolver un 201 Created y la información del usuario creado pero se devuelve el DTO.
        }

        //Modificar un usuario.
        [HttpPut]
        [Route("{id}")] //El ID para identificar, que usuario en especifico se desea actualizar.
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateUserRequestDto userDto){
            var userModel = await _context.Users.FirstOrDefaultAsync(user => user.Id == id);
            if (userModel == null){ //Si el usuario no existe, devolver un error 404.
                return NotFound(); //Devolver un error 404.
            }
            //Si el usuario fue encontrado, se actualizan los datos del usuario de los que vienen del DTO.
            userModel.Age = userDto.Age;
            userModel.FirstName = userDto.FirstName;
            userModel.LastName = userDto.LastName;

            await _context.SaveChangesAsync(); //Guardar los cambios en la base de datos.
            return Ok(userModel.ToDto()); //Devolver un 200 Ok y la información del usuario actualizado pero convertida en DTO.
        }
        
        //Eliminar un usuario.
        [HttpDelete]
        [Route("{id}")] //El ID para identificar, que usuario en especifico se desea eliminar.
        public async Task<IActionResult> Delete([FromRoute] int id){
            var userModel = await _context.Users.FirstOrDefaultAsync(user => user.Id == id);
            if (userModel == null){ //Si el usuario no existe, devolver un error 404.
                return NotFound(); //Devolver un error 404.
            }
            _context.Users.Remove(userModel); //Eliminar el usuario de la base de datos.

            await _context.SaveChangesAsync(); //Guardar los cambios en la base de datos.
            return NoContent(); //Devolver un 204 No Content.
        }

        //Método para asignar una mascota a un usuario.
        [HttpPost]
        [Route("{userId}/assign-pet-toUser/{petId}")] //Se recibe el ID del usuario y el ID de la mascota.
        public async Task<IActionResult> AssignPetToUser([FromRoute] int userId, [FromRoute] int petId){
            var user = await _context.Users.Include(user => user.Pets).FirstOrDefaultAsync(user => user.Id == userId);
            if (user == null){ //Si el usuario no existe, devolver un error 404.
                return NotFound(); //Devolver un error 404.
            }
            var pet = await _context.Pets.FirstOrDefaultAsync(pet => pet.Id == petId);
            if (pet == null){ //Si la mascota no existe, devolver un error 404.
                return NotFound(); //Devolver un error 404.
            }
            user.Pets.Add(pet); //Agregar la mascota al usuario.
            await _context.SaveChangesAsync(); //Guardar los cambios en la base de datos.
            return Ok(user.ToDto()); //Devolver un 200 Ok y la información del usuario.
        }

        //Método para desasignar una mascota a un usuario.
        [HttpDelete]
        [Route("{userId}/unassign-pet-toUser/{petId}")] //Se recibe el ID del usuario y el ID de la mascota.
        public async Task<IActionResult> UnassignPetToUser([FromRoute] int userId, [FromRoute] int petId){
            var user = await _context.Users.Include(user => user.Pets).FirstOrDefaultAsync(user => user.Id == userId);
            if (user == null){ //Si el usuario no existe, devolver un error 404.
                return NotFound(); //Devolver un error 404.
            }
            var pet = await _context.Pets.FirstOrDefaultAsync(pet => pet.Id == petId);
            if (pet == null){ //Si la mascota no existe, devolver un error 404.
                return NotFound(); //Devolver un error 404.
            }
            user.Pets.Remove(pet); //Eliminar la mascota del usuario.
            await _context.SaveChangesAsync(); //Guardar los cambios en la base de datos.
            return Ok(user.ToDto()); //Devolver un 200 Ok y la información del usuario.
        }

        [HttpPost]
        [Route("create-user-with-pets")]
        public async Task<IActionResult> CreateUserWithPets([FromBody] CreateUserWithPetsRequest userDto)
        {
            // Convertir el DTO a un modelo de usuario
            var userModel = userDto.ToUserFromCreateDto(); 

            // Agregar el usuario a la base de datos
            await _context.Users.AddAsync(userModel);
            await _context.SaveChangesAsync(); // Guardar cambios para obtener el ID del usuario

            // Buscar cada mascota por ID y agregarla al usuario
            foreach (var petId in userDto.PetIds)
            {
                var existingPet = await _context.Pets.FindAsync(petId);
                if (existingPet != null)
                {
                    userModel.Pets.Add(existingPet); // Asignar la mascota existente al usuario
                }
                else
                {
                    // Si el ID de mascota no existe, se puede manejar el error según el caso
                    return BadRequest($"La mascota con ID {petId} no existe.");
                }
            }

            // Guardar los cambios finales
            await _context.SaveChangesAsync(); 

            // Retornar el usuario creado
            return CreatedAtAction(nameof(getById), new { id = userModel.Id }, userModel.ToDto());
        }

        [HttpPost]
        [Route("create-user-and-pets-associatedToUser")]
        public async Task<IActionResult> CreateUserAndPetsAssociatedToUser([FromBody] CreateUserNPetAssociated userDto)
        {
            // Convertir el DTO a un modelo de usuario
            var userModel = userDto.ToUserFromCreateDto();

            // Agregar el usuario a la base de datos
            await _context.Users.AddAsync(userModel);
            await _context.SaveChangesAsync(); // Guardar cambios para obtener el ID del usuario

            // Agregar las mascotas al usuario
            foreach (var petDto in userDto.Pets)
            {
                var petModel = petDto.ToPetFromCreateDto(); // Convertir el DTO de mascota a un modelo de mascota
                petModel.UserId = userModel.Id; // Asignar el ID del usuario al modelo de mascota
                await _context.Pets.AddAsync(petModel); // Agregar la mascota a la base de datos
            }

            // Guardar los cambios finales
            await _context.SaveChangesAsync();

            // Retornar el usuario creado
            return CreatedAtAction(nameof(getById), new { id = userModel.Id }, userModel.ToDto());
        }
    }
}