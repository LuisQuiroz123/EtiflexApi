using AutoMapper;
using WebApi.Data;
using WebApi.DTOs.Print;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class MappingProfile : AutoMapper.Profile
{
    public MappingProfile()
    {
        CreateMap<PrintRequest, PrintRequestDto>();
        CreateMap<PrintRequestDto, PrintRequest>();

        CreateMap<ClientData, ClientDataDto>().ReverseMap();
        CreateMap<RequestFile, RequestFileDto>().ReverseMap();
    }
}