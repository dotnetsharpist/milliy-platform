using MilliyMock.DataAccess.IRepositories;
using MilliyMock.DataAccess.Repositories;

namespace MilliyMock.Configurations.Layers;

public static class DataAccessConfiguration
{
    public static void ConfigureDataAccess(this WebApplicationBuilder builder)
    {
        //-> DI containers, IoC containers
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}