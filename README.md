RDD does not depend on any Dependency Injection framework, but we often use it with SimpleInjector. Below are the bootstrapping code for your application using SimpleInjector.

Use the base [bootstrapping](http://simpleinjector.readthedocs.io/en/latest/using.html) from SimpleInjector

    public class Startup
    {
		private Container _container = new Container();
    	
    	public void ConfigureServices(IServiceCollection services)
		{
			...
		}
	}

Use the AsyncScopedLifestyle injection, as RDD is fully async

    _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
Add required .NET Core dependencies as below

    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    services.AddSingleton<IControllerActivator>(
		new SimpleInjectorControllerActivator(_container));
	services.EnableSimpleInjectorCrossWiring(_container);
    services.UseSimpleInjectorAspNetRequestScoping(_container);
	services.AddMvc()
		.AddJsonOptions(options =>
		{
			options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
		});
Add recommanded RDD dependencies

    _container.Register<IWebContextWrapper, HttpContextWrapper>(Lifestyle.Scoped);
    
    _container.Register<IWebContext>(() => _container.GetInstance<IWebContextWrapper>(), Lifestyle.Scoped);
    
    _container.Register<IExecutionContext, HttpExecutionContext>(Lifestyle.Scoped);
    
    _container.Register<ICombinationsHolder>(() => {yourCombinationsHoledHere}, Lifestyle.Singleton);

For storage, we mainly use Entity Framework. Here is how to handle dependencies upon EF in RDD

    _container.Register<IWebServicesCollection, WebServicesCollection>(Lifestyle.Scoped);
    
    _container.RegisterConditional<IStorageService, InMemoryStorageService>(Lifestyle.Singleton,

    var builder = new DbContextOptionsBuilder<AuthenticationContext>();
    builder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
    
    _container.Register<DbContextOptions<AuthenticationContext>>(() => builder.Options, Lifestyle.Singleton);
    
    _container.RegisterConditional<IStorageService, EFStorageService>(Lifestyle.Scoped,
				c => !c.Handled);
				
    _container.Register<DbContext, {yourDbContextHere}>(Lifestyle.Scoped);

For your Repositories dependencies, here is an example

    _container.Register<IRepository<MyClass>, MyClassRepo>(Lifestyle.Scoped);
    
    _container.RegisterConditional(typeof(IRepository<>), typeof(EFRepository<>), c => !c.Handled);

Mostly used web dependencies

    _container.Register(typeof(ApiHelper<,>), typeof(ApiHelper<,>), Lifestyle.Scoped);
    
    _container.Register<IContractResolver, CamelCasePropertyNamesContractResolver>(Lifestyle.Singleton);
    
    _container.Register<IEntitySerializer, EntitySerializer>(Lifestyle.Scoped);

