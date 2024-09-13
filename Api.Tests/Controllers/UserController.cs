using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using api.Controllers;
using api.Data;
using api.Models;
using api.Dtos.UserDtos;

namespace Api.Tests.Controllers
{
    //Comment to probe tests.
    public class UserControllerTests
    {
        private readonly UserController _controller; //Referencia al UserController que se desea testear.
        private readonly ApplicationDBContext _context; //Si o si se requiere para conectar a la base de datos de prueba.

        public UserControllerTests() //Constructor que inicializa el contexto de la base de datos y el controlador.
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>() //Crea un contexto de base de datos en memoria. Es decir, no debemos conectarnos a un SQL Server.
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDBContext(options); //Para que cree el contexto de la base de datos en memoria.

            _controller = new UserController(_context); //Para que cree el controlador de usuario con el contexto de la base de datos en memoria.

            SeedDatabase();
        }

        private void SeedDatabase() //Método que se encarga de crear registros de prueba en la base de datos en memoria.
        {
            if (!_context.Users.Any())
            {
                _context.Users.AddRange(new List<User>
                {
                    new User { Id = 1, FirstName = "John", LastName = "Doe", Age = 30 },
                    new User { Id = 2, FirstName = "Jane", LastName = "Doe", Age = 25 }
                });
                _context.SaveChanges();
            }
        }

        [Fact] //Método que se encarga de testear el método GetAll del controlador de usuario.
        public async Task GetAll_ReturnsOkResult_WithListOfUsers()
        {
            // Act
            var result = await _controller.GetAll(); //Llama al método GetAll del controlador de usuario.

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result); //Verificar que el result nos brinde de vuelta una respuesta Ok. de exito.
            var returnUsers = okResult.Value as IEnumerable<UserDto>; //Convertir el resultado en una lista de usuarios.

            Assert.NotNull(returnUsers); //Verificar que la lista de usuarios NO sea nula.
            var userList = returnUsers.ToList(); //Convertir la lista de usuarios en una lista de usuarios.

            Assert.Equal(2, userList.Count); //Verificar que la lista de usuarios tenga 2 elementos. Los cuales fueron creados en el SEED anteriormente.
            Assert.Contains(userList, u => u.FirstName == "John" && u.LastName == "Doe"); //Verificar que la lista de usuarios contenga un usuario con nombre John y apellido Doe.
            Assert.Contains(userList, u => u.FirstName == "Jane" && u.LastName == "Doe"); //Verificar que la lista de usuarios contenga un usuario con nombre Jane y apellido Doe.
        }

        [Fact] //Método que se encarga de testear el método GetById del control
        public async Task GetById_ReturnsOkResult_WithUser()
        {
            // Act
            var result = await _controller.getById(1); //Llama al método GetById del control

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result); //Verificar que el result nos brinde de vuelta una respuesta Ok. de exito.
            var returnUser = Assert.IsType<UserDto>(okResult.Value); //Convertir el resultado en un usuario.
            Assert.Equal(1, returnUser.Id); //Verificar que el usuario tenga el ID 1.
            Assert.Equal("John", returnUser.FirstName); //Verificar que el usuario tenga el nombre John.
        }

        [Fact] //Método que se encarga de testear el método Create del controlador.
        public async Task Create_ReturnsCreatedResult_WithNewUser()
        {
            // Arrange
            var newUser = new CreateUserRequestDto
            {
                FirstName = "Mark",
                LastName = "Smith",
                Age = 28
            };

            // Act
            var result = await _controller.Create(newUser);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnUser = Assert.IsType<UserDto>(createdResult.Value); //Convertir el resultado en un usuario.
            Assert.Equal("Mark", returnUser.FirstName);
            Assert.Equal("Smith", returnUser.LastName);

            // Verify that the user was actually added to the database
            var userInDb = await _context.Users.FindAsync(returnUser.Id);
            Assert.NotNull(userInDb);
        }

        [Fact]
        public async Task Update_ReturnsOkResult_WithUpdatedUser()
        {
            // Arrange
            var updatedUserDto = new UpdateUserRequestDto
            {
                FirstName = "John Updated",
                LastName = "Doe Updated",
                Age = 31
            };

            // Act
            var result = await _controller.Update(1, updatedUserDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnUser = Assert.IsType<UserDto>(okResult.Value); //Verificamos que lo que devuelva sea un UserDto.
            Assert.Equal("John Updated", returnUser.FirstName);
            Assert.Equal("Doe Updated", returnUser.LastName);

            // Verify that the user was actually updated in the database
            var userInDb = await _context.Users.FindAsync(1);
            Assert.NotNull(userInDb);
            Assert.Equal("John Updated", userInDb.FirstName);
            Assert.Equal("Doe Updated", userInDb.LastName);
        }

        [Fact]
        public async Task Delete_ReturnsNoContentResult()
        {
            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify that the user was actually removed from the database
            var userInDb = await _context.Users.FindAsync(1);
            Assert.Null(userInDb);
        }
    }
}