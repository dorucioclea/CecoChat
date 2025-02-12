using CecoChat.Contracts;

namespace CecoChat.Server.Bff.Infra;

public class AutoMapperProfile : AutoMapper.Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Contracts.Bff.RegisterRequest, Contracts.User.ProfileCreate>();
        CreateMap<Contracts.Bff.ChangePasswordRequest, Contracts.User.ProfileChangePassword>()
            .ForMember(
                profileContract => profileContract.Version,
                options => options.MapFrom(request => request.Version.ToUuid()));
        CreateMap<Contracts.Bff.EditProfileRequest, Contracts.User.ProfileUpdate>()
            .ForMember(profileContract => profileContract.Version,
                options => options.MapFrom(request => request.Version.ToUuid()));
        CreateMap<Contracts.User.ProfileFull, Contracts.Bff.ProfileFull>()
            .ForMember(
                profileBff => profileBff.Version,
                options => options.MapFrom(profileContract => profileContract.Version.ToGuid()));
        CreateMap<Contracts.User.ProfilePublic, Contracts.Bff.ProfilePublic>();
    }
}
