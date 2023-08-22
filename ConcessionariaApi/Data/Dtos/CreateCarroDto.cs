using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace ConcessionariaApi.Data.Dtos;

public class CreateCarroDto
{
   
    [Required(ErrorMessage = "Por gentileza, digite a marca do carro.")]
    public string Marca { get; set; }

    [Required(ErrorMessage = "O modelo do carro é obrigatório")]
    [StringLength(50, ErrorMessage = "O tamanho do modelo do carro não pode exceder 50 caracteres")]
    public string Modelo { get; set; }

    [Required]
    [Range(1886, 2050, ErrorMessage = "Por gentileza, digite um ano entre 1886 e 2000.")]
    public int Ano { get; set; }

    [Required(ErrorMessage = "Por gentileza, digite o preço do carro.")]
    public float Preco { get; set; }
}
