using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace ConcessionariaApi.Data.Dtos;

public class MarcarCarroVendidoDto
{
    public bool Vendido { get; set; }
}
