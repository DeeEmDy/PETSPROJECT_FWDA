using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.PetDtos;
using System.ComponentModel.DataAnnotations;


namespace api.Dtos.UserDtos
{
    public class CreateUserNPetAssociated
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(12, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 12 caracteres.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(12, MinimumLength = 3, ErrorMessage = "El apellido debe tener entre 3 y 12 caracteres.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "El apellido solo puede contener letras y espacios.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "La edad es obligatoria.")]
        [Range(1, 99, ErrorMessage = "La edad debe tener entre 1 y 99 años.")]
        [RegularExpression(@"^\d{1,2}$", ErrorMessage = "La edad solo puede contener 1 o 2 dígitos numéricos.")]
        public int Age { get; set; }
        public List<CreatePetRequestDto> Pets { get; set; } // Lista de mascotas a asociar al registro del usuario.
    }
}