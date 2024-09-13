using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Mappers;
using api.Dtos.PetDtos;
using api.Models;


namespace api.Controllers
{
    [Route("api/pet")]
    [ApiController]

    public class PetController : ControllerBase
    {

        private readonly ApplicationDBContext _context;
        public PetController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var pets = await _context.Pets.Include(pet => pet.User).ToListAsync();
            var petsDto = pets.Select(pet => pet.ToDto());
            return Ok(petsDto);
        }
        
        [HttpGet("{id}")] //Ejemplo prueba: http://localhost:5034/api/pet/1

        //Método para obtener una mascota por en específico por su ID.
        public async Task<IActionResult> getById([FromRoute] int id)
        {
            var pet = await _context.Pets.Include(pet => pet.User).FirstOrDefaultAsync(p => p.Id == id);
            if (pet == null)
            {
                return NotFound();
            }
            return Ok(pet.ToDto()); //Devolver un 200 Ok y la información de la mascota convertida a DTO.
        }
        
        //Método para crear una nueva mascota.
        [HttpPost] //Ejemplo prueba: http://localhost:5034/api/pet
        public async Task<IActionResult> Create([FromBody] CreatePetRequestDto petDto)
        {
            var petModel = petDto.ToPetFromCreateDto(); //Le asignamos al modelo de la mascota, lo que contiene la mascota de dto. Ya que lo que se envía a la base de datos es el modelo Pet.
            await _context.Pets.AddAsync(petModel); //Agregar la mascota a la base de datos.
            await _context.SaveChangesAsync(); //Guardar los cambios en la base de datos.
            return CreatedAtAction(nameof(getById), new { id = petModel.Id }, petModel.ToDto()); //Devolver un 201 Created y la información de la mascota creada pero se devuelve el DTO.
        }

        //Método para modificar una mascota.
        [HttpPut] //Ejemplo prueba: http://localhost:5034/api/pet/1
        [Route("{id}")] //El ID para identificar, que mascota en específico se desea actualizar.
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdatePetRequestDto petDto)
        {
            var petModel = await _context.Pets.FirstOrDefaultAsync(pet => pet.Id == id);
            if (petModel == null)
            {
                return NotFound();
            }
            //Si la mascota fue encontrada, se actualizan los datos de la mascota de los que vienen del DTO.
            petModel.Name = petDto.Name;
            petModel.Animal = petDto.Animal;
            petModel.UserId = petDto.UserId;
            await _context.SaveChangesAsync(); //Guardar los cambios en la base de datos.
            return Ok(petModel.ToDto()); //Devolver un 200 Ok y la información de la mascota actualizada convertida a DTO.
        }

        //Método para eliminar una mascota.
        [HttpDelete] //Ejemplo prueba: http://localhost:5034/api/pet/1
        [Route("{id}")] //El ID para identificar, que mascota en específico se desea eliminar.
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var petModel = await _context.Pets.FirstOrDefaultAsync(pet => pet.Id == id);
            if (petModel == null)
            {
                return NotFound();
            }
            _context.Pets.Remove(petModel); //Eliminar la mascota de la base de datos.
            await _context.SaveChangesAsync(); //Guardar los cambios en la base de datos.
            return Ok(petModel.ToDto()); //Devolver un 200 Ok y la información de la mascota eliminada convertida a DTO.
        }
    }
}