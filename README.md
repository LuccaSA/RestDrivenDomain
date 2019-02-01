# Rest Driven Domain

[![Build status](https://ci.appveyor.com/api/projects/status/edtq86puuj8qma2h?svg=true)](https://ci.appveyor.com/project/LuccaIntegration/restdrivendomain)
[![codecov](https://codecov.io/gh/LuccaSA/RestDrivenDomain/branch/master/graph/badge.svg)](https://codecov.io/gh/LuccaSA/RestDrivenDomain)
[![Sonarcloud coverage](https://sonarcloud.io/api/project_badges/measure?project=RestDrivenDomain&metric=coverage)](https://sonarcloud.io/dashboard?id=RestDrivenDomain)
[![Sonarcloud Status](https://sonarcloud.io/api/project_badges/measure?project=RestDrivenDomain&metric=alert_status)](https://sonarcloud.io/dashboard?id=RestDrivenDomain)
[![Sonarcloud Debt](https://sonarcloud.io/api/project_badges/measure?project=RestDrivenDomain&metric=sqale_index)](https://sonarcloud.io/dashboard?id=RestDrivenDomain)
[![Sonarcloud Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=RestDrivenDomain&metric=vulnerabilities)](https://sonarcloud.io/dashboard?id=RestDrivenDomain)

Rdd is a structural framework, for easy and fast REST implementation, with an integrated query language, projected on EF Core queries.  It's main goal is to provide a base implementation letting benefit native paging, querying and CRUD operations.



### Layer segregation & Inheritance chain

Rdd provide 4 distinct layers to structurally enforce Domain isolation.

- **Web** : WebController / ReadOnlyWebController are the main entry point for instanciating the full Rdd stack. All web related operations must stay on this layer.
- **Application** : AppController / ReadOnlyAppController are aimed at providing a global functionnal layer
- **Domain** : RestCollection / ReadOnlyRestCollection are placeholders for Domain centric opérations
- **Infra** : Repository / ReadOnlyRepository implements external access to data (via EF, HttpClient, Files, etc)



# Get Started

### Bootstraping Rdd: 

``` charp
public void ConfigureServices(IServiceCollection services)
{
    // register Rdd internal dependencies (method come from RddServiceCollectionExtensions)
    services.AddRdd<yourDbContext>(); //yourDbContext inheritate from EF Core DbContext
 
    services.AddMvc();
}
```

Rdd depends on standard Microsoft.Extensions.DependencyInjection, it's up to you to use any other DI framework (SimpleInjector/Autofac etc).

### Create your first Entity: 

Create the entity you want to query: 

```
public class MyFirstEntity : EntityBase<int> //int is your PK type (Id)
{
    public int Id { get; set; }
}
```

Add this entity in your `DbContext`

`public DbSet<MyFirstEntity> MyFirstEntities { get; set; }`

### Create your first Controller: 

The most simple controller is the `ReadOnlyWebController`

    [Route("api/[controller]")]
    [ApiController]
    public class MyFirstController : ReadOnlyWebController<MyFirstEntity, int>
    {
        protected MyFirstController(IReadOnlyAppController<MyFirstEntity, int> appController, IQueryParser<MyFirstEntity> queryParser) : base(appController, queryParser)
        { }
    }
You don't need to register this controller.

### Register all others layers:

You need to register all others layers:

     services.AddRdd<yourDbContext>()
             .AddReadOnlyRepository<Repository<MyFirstEntity>, MyFirstEntity>()
             .AddReadOnlyRestCollection<ReadOnlyRestCollection<MyFirstEntity, int>, MyFirstEntity, int>()
             .AddReadOnlyAppController<ReadOnlyAppController<MyFirstEntity, int>, MyFirstEntity, int>();
You can override any layer to personalize behavior.

### Remove Rights

If you don't want to setup rights:

            var rddBuilder = services.AddRdd<CleemyDbContext>();
            rddBuilder.WithDefaultRights(RightDefaultMode.Open);
