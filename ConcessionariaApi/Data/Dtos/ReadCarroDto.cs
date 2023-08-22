using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace ConcessionariaApi.Data.Dtos;

public class ReadCarroDto
{
    public string Marca { get; set; }

    public string Modelo { get; set; }

    public int Ano { get; set; }

    public float Preco { get; set; }

    public DateTime DataCadastro { get; set; }
    public bool Vendido { get; set; }
}
