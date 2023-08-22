using AutoMapper;
using ConcessionariaApi.Data;
using ConcessionariaApi.Data.Dtos;
using ConcessionariaApi.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ConcessionariaApi.Controllers;

[ApiController]
[Route("[controller]")]
public class CarroController : ControllerBase
{
    private CarroContext _context;
    private IMapper _mapper;
    public CarroController(CarroContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetCarroList([FromQuery] bool? vendido = null, [FromQuery] int pagina = 0, [FromQuery] int? ano = null, [FromQuery] string? modelo = null, [FromQuery] float? precoMaiorQue = null, [FromQuery] float? precoMenorQue = null, [FromQuery] DateTime? dataCadastroMaiorQue = null, [FromQuery] DateTime? dataCadastroMenorQue = null)
    {
        var constructor = Builders<Carro>.Filter;
        var condition = Builders<Carro>.Filter.Empty;

        if (vendido.HasValue)
        {
            condition &= constructor.Eq(c => c.Vendido, vendido.Value);
        }

        if (ano.HasValue)
        {
            condition &= constructor.Eq(c => c.Ano, ano.Value);
        }

        if (!string.IsNullOrEmpty(modelo))
        {
            condition &= constructor.Regex(c => c.Modelo, new BsonRegularExpression(modelo, "i"));
        }

        if (precoMaiorQue.HasValue)
        {
            condition &= constructor.Gt(c => c.Preco, precoMaiorQue.Value);
        }

        if (precoMenorQue.HasValue)
        {
            condition &= constructor.Lt(c => c.Preco, precoMenorQue.Value);
        }

        if (dataCadastroMaiorQue.HasValue)
        {
            condition &= constructor.Gt(c => c.DataCadastro, dataCadastroMaiorQue.Value);
        }

        if (dataCadastroMenorQue.HasValue)
        {
            condition &= constructor.Lt(c => c.DataCadastro, dataCadastroMenorQue.Value);
        }

        int take = 5;
        var quantPorPag = pagina * take;

        var carros = await _context.Carros.Find(condition).ToListAsync();
        var carrosDto = _mapper.Map<List<ReadCarroDto>>(carros.Skip(quantPorPag).Take(take));

        return Ok(carrosDto);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCarroById(string id)
    {
        var constructor = Builders<Carro>.Filter;
        var condition = constructor.Eq(x => x.Id, id);

        var carro = await _context.Carros.Find(condition).FirstOrDefaultAsync();

        if (carro == null) return NotFound();
        var carroDto = _mapper.Map<ReadCarroDto>(carro);
        return Ok(carroDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCarro([FromBody] CreateCarroDto carroDto)
    {
        Carro carro = _mapper.Map<Carro>(carroDto);

        carro.DataCadastro = DateTime.UtcNow;
        carro.Vendido = false;

        await _context.Carros.InsertOneAsync(carro);
        return CreatedAtAction(nameof(GetCarroById), new { id = carro.Id }, carro);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCarro(string id, [FromBody] UpdateCarroDto carroDto)
    {
        var carro = await _context.Carros.Find(c => c.Id == id).FirstOrDefaultAsync();

        if (carro == null)
        {
            return NotFound();
        }

        _mapper.Map(carroDto, carro);

        var filter = Builders<Carro>.Filter.Eq(c => c.Id, id);
        var update = Builders<Carro>.Update
            .Set(c => c.Marca, carro.Marca)
            .Set(c => c.Modelo, carro.Modelo)
            .Set(c => c.Ano, carro.Ano)
            .Set(c => c.Preco, carro.Preco);

        await _context.Carros.UpdateOneAsync(filter, update);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCarro(string id)
    {
        var carro = await _context.Carros.Find(c => c.Id == id).FirstOrDefaultAsync();
        if (carro == null)
        {
            return NotFound();
        }

        await _context.Carros.DeleteOneAsync(c => c.Id == id);
        return NoContent();
    }

    [HttpPatch("{id}/marcar-vendido")]
    public async Task<IActionResult> MarcarVendido(string id, JsonPatchDocument<MarcarCarroVendidoDto> patch, [FromQuery] float desconto = 0)
    {
        if (patch == null)
        {
            return BadRequest();
        }

        var carro = await _context.Carros.Find(c => c.Id == id).FirstOrDefaultAsync();

        if (carro == null)
        {
            return NotFound();
        }

        if (carro.Vendido)
        {
            ModelState.AddModelError("Vendido", "Não é possível marcar como vendido um carro que já foi vendido.");
            return BadRequest(ModelState);
        }

        var carroToUpdate = _mapper.Map<MarcarCarroVendidoDto>(carro);

        patch.ApplyTo(carroToUpdate, ModelState);

        if (!TryValidateModel(carroToUpdate))
        {
            return ValidationProblem(ModelState);
        }

        if (desconto > 0)
        {
            var valorDesconto = (desconto / 100) * carro.Preco;
            carro.PrecoVendido -= valorDesconto;
        }

        carro.Vendido = true;
        carro.DataVenda = DateTime.UtcNow;


        var filter = Builders<Carro>.Filter.Eq(c => c.Id, id);
        var update = Builders<Carro>.Update.Set(c => c.Vendido, carro.Vendido)
            .Set(c => c.PrecoVendido, carro.PrecoVendido)
            .Set(c => c.DataVenda, carro.DataVenda);

        await _context.Carros.UpdateOneAsync(filter, update);

        return NoContent();
    }

    [HttpGet("vendas")]
    public async Task<IActionResult> GetVendas([FromQuery] CheckPeriodoDto periodoDto)
    {
        if (periodoDto == null)
        {
            return BadRequest();
        }

        var filtro = Builders<Carro>.Filter.Gte(c => c.DataVenda, periodoDto.InicioPeriodo) & 
            Builders<Carro>.Filter.Lte(c => c.DataVenda, periodoDto.FinalPeriodo) & 
            Builders<Carro>.Filter.Eq(c => c.Vendido, true);

        var carrosVendidos = await _context.Carros.Find(filtro).ToListAsync();
        float valorTotalVendas = carrosVendidos.Sum(c => c.Preco);
        
        var vendas = new
        {
            CarrosVendidos = carrosVendidos.Count,
            ValorTotalVendas = valorTotalVendas
        };

        return Ok(vendas);

    }
}