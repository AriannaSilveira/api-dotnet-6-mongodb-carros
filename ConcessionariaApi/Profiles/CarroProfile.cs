using AutoMapper;
using ConcessionariaApi.Data.Dtos;
using ConcessionariaApi.Models;

namespace ConcessionariaApi.Profiles;

public class CarroProfile : Profile
{
    public CarroProfile()
    {
        CreateMap<CreateCarroDto, Carro>();
        CreateMap<UpdateCarroDto, Carro>(); 
        CreateMap<MarcarCarroVendidoDto, Carro>();
        CreateMap<Carro, MarcarCarroVendidoDto>();
        CreateMap<Carro, ReadCarroDto>();
    }
}
