using System.Collections.Generic;
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

        // Obtener todos los usuarios
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _context.Users.Include(user => user.Pets).ToListAsync();
            var usersDto = users.Select(user => user.ToDto());
            return Ok(usersDto);
        }

        // Obtener un usuario por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var user = await _context.Users.Include(user => user.Pets).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                // Devolver un mensaje personalizado cuando no se encuentra un usuario
                return NotFound(new { message = $"No se ha encontrado un usuario con el ID: {id}" });
            }
            return Ok(user.ToDto());
        }


        // Crear un nuevo usuario
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequestDto userDto)
        {
            // Verificar si ya existe un usuario con el mismo nombre y apellido
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.FirstName.ToLower() == userDto.FirstName.ToLower() &&
                                           u.LastName.ToLower() == userDto.LastName.ToLower());

            if (existingUser != null)
            {
                return BadRequest(new { message = "Ya existe un usuario con el mismo nombre y apellido." });
            }

            var userModel = userDto.ToUserFromCreateDto(); // Convertir el DTO a modelo de usuario
            await _context.Users.AddAsync(userModel); // Agregar el nuevo usuario a la base de datos
            await _context.SaveChangesAsync(); // Guardar los cambios en la base de datos
            return CreatedAtAction(nameof(GetById), new { id = userModel.Id }, userModel.ToDto()); // Retornar el usuario creado
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateUserRequestDto userDto)
        {
            var userModel = await _context.Users.FirstOrDefaultAsync(user => user.Id == id);
            if (userModel == null)
            {
                // Devolver un mensaje personalizado cuando no se encuentra el usuario
                return NotFound(new { message = $"No se ha encontrado un usuario con el ID: {id}" });
            }

            // Verificar si ya existe otro usuario con el mismo nombre y apellido
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.FirstName.ToLower() == userDto.FirstName.ToLower() &&
                                           u.LastName.ToLower() == userDto.LastName.ToLower() &&
                                           u.Id != id); // Asegurarse de que no sea el mismo usuario

            if (existingUser != null)
            {
                return BadRequest(new { message = "Ya existe un usuario con el mismo nombre y apellido." });
            }

            // Actualizar los datos del usuario
            userModel.Age = userDto.Age;
            userModel.FirstName = userDto.FirstName;
            userModel.LastName = userDto.LastName;

            await _context.SaveChangesAsync(); // Guardar cambios
            return Ok(userModel.ToDto()); // Devolver usuario actualizado
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var userModel = await _context.Users.FirstOrDefaultAsync(user => user.Id == id);
            if (userModel == null)
            {
                // Devolver un mensaje personalizado cuando no se encuentra el usuario
                return NotFound(new { message = $"No se ha encontrado un usuario con el ID: {id}" });
            }

            _context.Users.Remove(userModel); // Eliminar el usuario de la base de datos.
            await _context.SaveChangesAsync(); // Guardar los cambios en la base de datos.
            return NoContent(); // Devolver 204 No Content.
        }


        // Asignar una mascota a un usuario
        [HttpPost("{userId}/assign-pet-toUser/{petId}")]
        public async Task<IActionResult> AssignPetToUser([FromRoute] int userId, [FromRoute] int petId)
        {
            var user = await _context.Users.Include(user => user.Pets).FirstOrDefaultAsync(user => user.Id == userId);
            if (user == null)
            {
                // Devolver un mensaje personalizado cuando no se encuentra el usuario
                return NotFound(new { message = $"No se ha encontrado un usuario con el ID: {userId}" });
            }

            var pet = await _context.Pets.FindAsync(petId);
            if (pet == null)
            {
                // Devolver un mensaje personalizado cuando no se encuentra la mascota
                return NotFound(new { message = $"No se ha encontrado una mascota con el ID: {petId}" });
            }

            user.Pets.Add(pet); // Agregar la mascota al usuario.
            await _context.SaveChangesAsync(); // Guardar los cambios en la base de datos.
            return Ok(user.ToDto()); // Devolver 200 Ok y la información del usuario.
        }


        // Desasignar una mascota de un usuario
        [HttpDelete("{userId}/unassign-pet-toUser/{petId}")]
        public async Task<IActionResult> UnassignPetToUser([FromRoute] int userId, [FromRoute] int petId)
        {
            var user = await _context.Users.Include(user => user.Pets).FirstOrDefaultAsync(user => user.Id == userId);
            if (user == null)
            {
                // Devolver un mensaje personalizado cuando no se encuentra el usuario
                return NotFound(new { message = $"No se ha encontrado un usuario con el ID: {userId}" });
            }

            var pet = await _context.Pets.FindAsync(petId);
            if (pet == null)
            {
                // Devolver un mensaje personalizado cuando no se encuentra la mascota
                return NotFound(new { message = $"No se ha encontrado una mascota con el ID: {petId}" });
            }

            if (!user.Pets.Contains(pet))
            {
                // Devolver un mensaje si la mascota no está asignada al usuario
                return BadRequest(new { message = $"La mascota con el ID: {petId} no está asignada al usuario con el ID: {userId}" });
            }

            user.Pets.Remove(pet); // Eliminar la mascota del usuario.
            await _context.SaveChangesAsync(); // Guardar los cambios en la base de datos.
            return Ok(user.ToDto()); // Devolver 200 Ok y la información del usuario.
        }


        // Crear un usuario con mascotas
        [HttpPost("create-user-with-pets")]
        public async Task<IActionResult> CreateUserWithPets([FromBody] CreateUserWithPetsRequest userDto)
        {
            // Validar el cuerpo de la solicitud
            if (userDto == null || string.IsNullOrEmpty(userDto.FirstName) || string.IsNullOrEmpty(userDto.LastName))
            {
                return BadRequest(new { message = "Datos del usuario incompletos. Se requiere nombre y apellidos." });
            }

            // Convertir el DTO a un modelo de usuario
            var userModel = userDto.ToUserFromCreateDto();

            // Agregar el usuario a la base de datos
            await _context.Users.AddAsync(userModel);
            await _context.SaveChangesAsync(); // Guardar cambios para obtener el ID del usuario

            // Asignar mascotas al usuario
            var petsNotFound = new List<int>(); // Para almacenar mascotas no encontradas
            foreach (var petId in userDto.PetIds)
            {
                var existingPet = await _context.Pets.FindAsync(petId);
                if (existingPet != null)
                {
                    userModel.Pets.Add(existingPet); // Asignar la mascota existente al usuario
                }
                else
                {
                    petsNotFound.Add(petId); // Agregar a la lista de mascotas no encontradas
                }
            }

            // Si hay mascotas no encontradas, devolver un error
            if (petsNotFound.Any())
            {
                return BadRequest(new { message = $"Las siguientes mascotas no existen: {string.Join(", ", petsNotFound)}" });
            }

            await _context.SaveChangesAsync(); // Guardar los cambios finales

            // Devolver el usuario creado con código 201 y la URL del recurso creado
            return CreatedAtAction(nameof(GetById), new { id = userModel.Id }, userModel.ToDto());
        }


        // Crear un usuario y asociar mascotas a él
        [HttpPost("create-user-and-pets-associatedToUser")]
        public async Task<IActionResult> CreateUserAndPetsAssociatedToUser([FromBody] CreateUserNPetAssociated userDto)
        {
            // Validar que el DTO del usuario no sea nulo y que los campos requeridos estén completos
            if (userDto == null || string.IsNullOrEmpty(userDto.FirstName) || string.IsNullOrEmpty(userDto.LastName))
            {
                return BadRequest(new { message = "Datos del usuario incompletos. Se requiere nombre y apellidos." });
            }

            // Verificar si el usuario ya existe (por ejemplo, usando nombre completo como identificador)
            var existingUser = await _context.Users
                                             .FirstOrDefaultAsync(u => u.FirstName == userDto.FirstName && u.LastName == userDto.LastName);
            if (existingUser != null)
            {
                return Conflict(new { message = $"El usuario {userDto.FirstName} {userDto.LastName} ya existe." });
            }

            // Validar que las mascotas sean válidas
            if (userDto.Pets == null || !userDto.Pets.Any())
            {
                return BadRequest(new { message = "Debe asociar al menos una mascota al usuario." });
            }

            // Convertir el DTO a un modelo de usuario
            var userModel = userDto.ToUserFromCreateDto();

            // Agregar el usuario a la base de datos
            await _context.Users.AddAsync(userModel);
            await _context.SaveChangesAsync(); // Guardar cambios para obtener el ID del usuario

            // Crear y asociar mascotas al usuario, evitando duplicados
            var invalidPets = new List<string>(); // Lista para mascotas con datos inválidos
            foreach (var petDto in userDto.Pets)
            {
                // Validar que los campos requeridos de la mascota estén completos
                if (string.IsNullOrEmpty(petDto.Name) || string.IsNullOrEmpty(petDto.Animal))
                {
                    invalidPets.Add(petDto.Name ?? "Sin nombre"); // Agregar a la lista de mascotas inválidas
                    continue; // Saltar esta mascota y continuar con la siguiente
                }

                // Verificar si la mascota ya existe (usando nombre y tipo como identificador)
                var existingPet = await _context.Pets
                                                .FirstOrDefaultAsync(p => p.Name == petDto.Name && p.Animal == petDto.Animal);
                if (existingPet != null)
                {
                    // Si ya existe una mascota con el mismo nombre y tipo, evitar duplicados
                    return Conflict(new { message = $"La mascota {petDto.Name} de tipo {petDto.Animal} ya existe." });
                }

                var petModel = petDto.ToPetFromCreateDto();
                petModel.UserId = userModel.Id; // Asignar el ID del usuario al modelo de mascota
                await _context.Pets.AddAsync(petModel); // Agregar la mascota a la base de datos
            }

            // Si hay mascotas con datos inválidos, retornar un error con información
            if (invalidPets.Any())
            {
                return BadRequest(new { message = $"Las siguientes mascotas tienen datos incompletos: {string.Join(", ", invalidPets)}" });
            }

            await _context.SaveChangesAsync(); // Guardar los cambios finales

            // Devolver el usuario creado con código 201 y la URL del recurso creado
            return CreatedAtAction(nameof(GetById), new { id = userModel.Id }, userModel.ToDto());
        }
    }
}
