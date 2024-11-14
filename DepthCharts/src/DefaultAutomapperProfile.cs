using AutoMapper;
using DepthCharts.Models;

namespace DepthCharts;

public class DefaultAutomapperProfile : Profile
{
    public DefaultAutomapperProfile()
    {
        CreateMap<PlayerEntryModel, PlayerDto>()
            .ConstructUsing(src =>
                new PlayerDto(src.PlayerNumber, src.PlayerName, ""));
        
        CreateMap<PlayerDto, PlayerEntryModel>()
            .ConstructUsing(src =>
                new PlayerEntryModel(src.Number, src.Name));
    }
}