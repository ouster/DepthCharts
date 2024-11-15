using AutoMapper;
using DepthCharts.Models;

namespace DepthCharts
{
    public class DefaultAutomapperProfile : Profile
    {
        public DefaultAutomapperProfile()
        {
            // Mapping from PlayerEntryModel to PlayerDto
            CreateMap<PlayerEntryModel, PlayerDto>()
                .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.PlayerNumber))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.PlayerName))
                .ForMember(dest => dest.Position, opt => opt.Ignore()); // Handle Position if necessary
            
            // Mapping from PlayerDto to PlayerEntryModel
            CreateMap<PlayerDto, PlayerEntryModel>()
                .ForMember(dest => dest.PlayerNumber, opt => opt.MapFrom(src => src.Number))
                .ForMember(dest => dest.PlayerName, opt => opt.MapFrom(src => src.Name));
        }
    }
}
