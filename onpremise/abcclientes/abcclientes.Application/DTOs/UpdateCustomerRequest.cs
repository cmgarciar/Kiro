using System.ComponentModel.DataAnnotations;

namespace abcclientes.Application.DTOs;

/// <summary>
/// Request DTO for updating an existing customer.
/// </summary>
public class UpdateCustomerRequest
{
    [Required(ErrorMessage = "El campo Nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El campo Nombre no puede superar 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "El campo Direccion no puede superar 200 caracteres.")]
    public string? Direccion { get; set; }

    [MaxLength(20, ErrorMessage = "El campo Telefono no puede superar 20 caracteres.")]
    [RegularExpression(@"^\d*$", ErrorMessage = "El campo Telefono solo debe contener digitos.")]
    public string? Telefono { get; set; }
}
