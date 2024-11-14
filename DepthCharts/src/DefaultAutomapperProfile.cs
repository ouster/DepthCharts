using AutoMapper;
using DepthCharts.Models;

namespace DepthCharts;

public class DefaultAutomapperProfile : Profile
{
    public DefaultAutomapperProfile()
    {
        // Create a map between matching types without specifying properties explicitly
        CreateMap<PlayerEntryModel, PlayerDto>();
    }
}