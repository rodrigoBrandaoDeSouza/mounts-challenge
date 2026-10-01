using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Application.Users.UpdateUser;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Users;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserNameDto, PersonName>().ReverseMap();

        CreateMap<UserAddressDto, Address>()
            .ForMember(d => d.Latitude, o => o.MapFrom(s => s.Geolocation.Lat))
            .ForMember(d => d.Longitude, o => o.MapFrom(s => s.Geolocation.Long));

        CreateMap<Address, UserAddressDto>()
            .ForMember(d => d.Geolocation, o => o.MapFrom(s => new GeolocationDto { Lat = s.Latitude, Long = s.Longitude }));

        CreateMap<User, UserResult>();

        // Password is hashed and Status/Role defaults are resolved by the handlers.
        CreateMap<CreateUserCommand, User>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Password, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.Role, o => o.Ignore());

        CreateMap<UpdateUserCommand, User>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Password, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.Role, o => o.Ignore());
    }
}
