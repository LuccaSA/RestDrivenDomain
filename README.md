# Rest Driven Domain

RDD is a structural framework, for easy and fast REST implementation, with an integrated query language, projected on EF Core queries.  It's main goal is to provide a base implementation letting benefit native paging, querying and CRUD operations.

### Bootstraping RDD : 

``` charp
public void ConfigureServices(IServiceCollection services)
{
    // register your dbcontext
    services.AddScoped<DbContext, MyDbContext>();

    // register RDD internal dependencies
    services.AddRdd();
 
    services.AddMvc();
}
```

RDD depends on standard Microsoft.Extensions.DependencyInjection, it's up to you to use any other DI framework (SimpleInjector/Autofac etc).

### Needed registerings : 

The needed registerings are : 

``` charp
services.AddScoped<IExecutionContext>(() => new HttpExecutionContext
{
    curPrincipal = new CurPrincipal()
});

services.AddSingleton<ICombinationsHolder, CombinationsHolder>();
services.AddSingleton<IUrlProvider, UrlProvider>();
services.AddScoped<IStorageService, EFStorageService>();
services.AddScoped<IPatcherProvider, PatcherProvider>();
```

This list is subject to future improvements

### Layer segregation & Inheritance chain

RDD provide 4 distinct layers to structurally enforce Domain isolation.
- **Web** : WebController / ReadOnlyWebController are the main entry point for instanciating the full RDD stack. All web related operations must stay on this layer.
- **Application** : AppController / ReadOnlyAppController are aimed at providing a global functionnal layer
- **Domain** : RestCollection / ReadOnlyRestCollection are placeholders for Domain centric opérations
- **Infra** : Repository / ReadOnlyRepository implements external access to data (via EF, HttpClient, Files, etc)

