using Autofac;
using CecoChat.Autofac;
using Microsoft.Extensions.Configuration;

namespace CecoChat.Client.IdGen;

public sealed class IdGenAutofacModule : Module
{
    private readonly IConfiguration _idGenConfiguration;

    public IdGenAutofacModule(IConfiguration idGenConfiguration)
    {
        _idGenConfiguration = idGenConfiguration;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<IdGenClient>().As<IIdGenClient>().SingleInstance();
        builder.RegisterType<IdChannel>().As<IIdChannel>().SingleInstance();
        builder.RegisterOptions<IdGenOptions>(_idGenConfiguration);
    }
}
