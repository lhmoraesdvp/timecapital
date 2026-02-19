using System.ComponentModel.DataAnnotations;

namespace TimeCapital.Web.ViewModels;

public sealed class CreateAreaViewModel
{
    [Required(ErrorMessage = "Informe o nome da área.")]
    [StringLength(40, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 40 caracteres.")]
    public string Name { get; set; } = "";

    [StringLength(30)]
    public string? Color { get; set; }
}
