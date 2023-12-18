namespace EPR.ProducerContentValidation.TestSupport;

using AutoMapper;

public static class AutoMapperHelpers
{
    public static Mapper GetMapper<T>()
        where T : Profile
    {
        var profiles = new Profile[] { (T)Activator.CreateInstance(typeof(T)) };
        return new Mapper(new MapperConfiguration(config => config.AddProfiles(profiles)));
    }
}