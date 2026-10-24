using System.ComponentModel.DataAnnotations;

namespace abcclientes.Application.DTOs;

public class UpdateCustomerRequest
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Direccion { get; set; }

    [MaxLength(20)]
    [RegularExpression(@"^\d*$")]
    public string? Telefono { get; set; }
}
