using ConcessionariaApi.Models;
using MongoDB.Driver;

namespace ConcessionariaApi.Data;

public class CarroContext
{
    private readonly IMongoDatabase _database;

    public CarroContext(IConfiguration config)
    {
        var client = new MongoClient(config.GetConnectionString("CarroConnection"));
        _database = client.GetDatabase("Concessionaria");
    }

    public IMongoCollection<Carro> Carros => _database.GetCollection<Carro>("Carros");
}
