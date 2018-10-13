# Futur release
## Breaking changes
 - **Modification**: ``IReflectionProvider``-> ``IReflectionHelper``
 - **Removed**: unused classes `ExpressionHelper` and `NameValueCollectionHelper`
 - **Removed**: `IClonable<>` interface & ``Clone()`` method
 - **Removed**: `Query<TEntity> query` parameter removed from prototype of ``ReadOnlyRepository.Set()`` method 
 - **Modification**: ``ValidateEntity`` on RestCollection is now ``ValidateEntityAsync``
 - **Modification**: ``AppController`` now depends on a `IUnitOfWork`.
 - **Modification**: `IStorageService` cannot directly save changes, use code in application layer.
 - **Removed**: `IStorageService.AddAfterSaveChangesAction`. Use code in application layer instead.
 - **Modification**: RDD namespace renamed to Rdd
 - **Modification**: UseRDD() and AddRDD() extension methods renamed to UseRdd() and AddRdd()
 - **Modification**: Multiple Put now returns a `ISelection` instead of enumerable
 - **Removed**: unused metadata.paging in returned json
 - **Removed**: `IRddSerializer`. replaced by a `RddJsonResult`. `FuncSerializer` is also dropped.
 - **Modification**: ``ISerializer`` nows feeds a `JsonTextWriter` instead of returning an object
 - **Removed**: All protected methods (`ProtectedGetAsync` / `ProtectedPostAsync` / `ProtectedPutAsync` / `DeleteByIdAsync`) are removed. To allow a verb on a controller, manipulate the `AllowedHttpVerbs` and `AllowedByIdHttpVerbs` properties accordingly. To change default routing, override the methods (`public override async Task<IActionResult> GetAsync()`) and decorate with routing attribute (ex `[HttpGet("my-route")]`). This changes allows swagger to properly discover the Rdd apis if you opt-in.
 - **Removed**: Implicit routing is no longer available. Each route need to be routed by attribute **only**
 - **Removed**: `CulturedDescriptionAttribute`, `CultureContext`, `IWebServicesCollection`, `WebService`, `Application`, `Enum`, `IIncludable`, `IDownloadableEntity`
 - **Removed**: `IJsonElement.Map`, `IJsonElement.RemovePath`, most `Period` members
 - **Modification**: `BusinessException` and `TechnicalException` ar now `ApplicationException`. Some exceptions constructors have been removed.
 - **Modification**: `RDD.Domain.ICombinationsHolder` -> `RDD.Domain.Rights.ICombinationsHolder`
 - **Removed**: `OutOfRangeException`. Prefer `BadRequestException` with an innerException of type `ArgumentOutOfRange`.
 - **Modification**: `FieldsParser<T>` -> `FieldsParser`. Methods now carry the generic argument. Thi allows for manipulation where the type is not known at design time.
 - **Modification**: `SerializerProvider._reflectionProvider` ->`SerializerProvider.ReflectionProvider`
 - **Modification**: `SerializerProvider._urlProvider_` ->`SerializerProvider.UrlProvider`
 - **Removed**: `PropertySelector`, `CollectionPropertySelector` and `DictionaryPropertySelector`. Replaced by `IExpression`, `IExpressionChain`, `IExpressionTree` and their implementations. `
 - **Removed**: `FieldExpansionHelper`, `PropertySelectorEqualityComparer`, `PropertySelectorIncludablesExtractor`, `PropertySelectorRootLambdaExtractor`, `PropertySelectorTransferor`, `CollectionFieldsParser`
 - **Modification**: `ICandidate.HasProperty(Expression<Func<TEntity, object>> expression)` -> `ICandidate.HasProperty<TProp>(Expression<Func<TEntity, TProp>> expression)`. Allows for better expression (no conversion required to type object). Other changed member include `WebFiltersContainer.RemoveFilter`, `WebFiltersContainer.GetFilter`, `WebFiltersContainer.HasFilter`
 - **Removed**: `IEnumerable<Field> Query<T>.CollectionFields`, `ISelection.Sum`, `ISelection.Min`,`ISelection.Max`, `DecimalRounding`. This functionality was considered problematic and almost unused.
 - **Removed**: `OrderBysConverter`. Replaced by method `ApplyOrderBy` on the `OrderBy` class directly
 - **Removed**: `Field`. Replaced by a `IExpressionTree`
 - **Modification**: `IEnumerable<Field> Query<T>.Fields` -> `IExpressionTree Query<T>.Fields`
 - **Modification**: `Queue<OrderBy<TEntity>> Query<T>.OrderBys ` -> `List<OrderBy<TEntity>> Query<T>.OrderBys`
 - **Removed**: Unused methods on `QueryBuilder`
 - **Removed**: `SerializationOption` Replaced by an `IExpressionTree`. Some signatures have been changed accordingly.
 - **Modification**: `RestCollection.PatcherProvider` has been replaced by `IPatcher<TEntity> Patcher`. This modification aims at helping the override of tha patching behavior via dependence injection instead of defining a concrete `RestCollection`, and is overall easier to use. This modifies the constructor
 - **Removed**: `RestCollection.GetPatcher`. The patcher should be injected via the constructore. The patcher should be registered in the dependency injection framework.
 - **Modification**: `internal interface IPatcher<T> where T : IJsonElement` -> `public interface IPatcher<T> where T : class`. The generic argument of the `IPatcher` interface references the class the patcher aims to apply to.
 - **Removed**: `IReadOnlyRestCollection.GetAllAsync()`. This method used to apply default paging (10) and was misleading. Developers are encouraged to define their own methods on their Collections concrete implementation.
 - **Modification**: Queries initiated with an expression now do not apply the paging instead of the paging by default (10). To return to the previous behavior, the `Page` property needs to be explicitely set to the desired value.
 - **Modification**: `XXXById` methods on `ReadOnlyRestCollection` no longer throw an exception when not found. An exception is still thrown from the `WebController` if not found: the exception message was modified and included into a native `NotFoundObjectResult` that returns a JSON response.
 - **Removed**: `ReadOnlyRestCollection.TryGetByIdAsync(object id)`. According to the previous modification, this method became obsolete.
 - **Removed**: `NotFoundException` is not used any more and has been removed.
 - **Removed**: `ReadOnlyRestCollection.GetByIdAsync(TKey id, HttpVerbs verb = HttpVerbs.Get)`. Consider using `ReadOnlyRestCollection.GetByIdAsync(TKey id, Query<TEntity> query)`.
 - **Removed**: `RestCollection.CreateAsync(TEntity entity, Query<TEntity> query)`. Consider using `RestCollection.CreateAsync(ICandidate<TEntity, TKey> candidate, Query<TEntity> query)`.
 - **Modification**: `IAppController.DeleteByIdsAsync(IEnumerable<TKey> ids) -> IAppController.DeleteByIdsAsync(IList<TKey> ids)`. This breaking change might require you to change your override signatures.
 - **Modification**: `IRestCollection.DeleteByIdsAsync(IEnumerable<TKey> ids) -> IAppController.DeleteByIdsAsync(IList<TKey> ids)`. This breaking change might require you to change your override signatures.
 - **Modification**: Error messages have been modified.

## New features
 - **Added**: CHANGELOG.md
 - **Added**: `IReflectionHelper.IsPseudoValue(Type type)` for types serialized and deserialized as JSON value.
 - **Added**: Opt-in support of Swagger for RDD controllers. Use `[ApiExplorerSettings(IgnoreApi = false)]` on your actions/controllers to display them.
 - **Added**: Inheritance support. To expose an API from a base class, use `RDDServiceCollectionExtensions.AddRddInheritanceConfiguration`. Then, Rdd will automatically take care of th rest for this API to work as expected. The interface `IInheritanceConfiguration` allows for the description of the diffetents classes to Rdd.
 - **Added**: `FieldsParser` exposes methods `ParseAllProperties` and `ParseFields` that can be used as generic or with type as argument.
 - **Added**: `BaseClassInstanciator`, `BaseClassPatcher` and `BaseClassJsonConverter` to properly manage inheritance schemes during edition.
 - **Added**: `SerializerProvider.InheritanceConfigurations` and `BaseClassSerializer` to properly manage inheritance schemes during exposition.
 - **Added**: `ReadOnlyRepository<T>.IncludeWhiteList`. This allows an automatic white-list approach on the include on a Get query.
 - **Added**: New logic for property and member selection via expression coming from the web. The main interface to manipulate is `IExpression`, that replaces `PropertySelector`. The main implementation to use now are `PropertyExpression`, `EnumerablePropertyExpression`, `ItemExpression`, `ExpressionChain`, or `ExpressionTree`. The parsing and manipulation is ensured by `ExpressionParser` and the visitors `ExpressionChainExtractor`, `ExpressionChainer`.
 - **Added**: `ExpressionEqualityComparer` to compare `Expression` and another one to compare `IExpression`.
 - **Added**: readable static method to construct `OrderBy`
 - **Added**: implicit conversion from a `Filter<T>` to `Expression<T, bool>`.
 - **Added**: `IAppController.CreateAsync(IEnumerable<ICandidate<TEntity, TKey>> candidates, Query<TEntity> query)`. This method allows creations in batch from the controller.
 - **Added**: `IRestCollection.CreateAsync(IEnumerable<ICandidate<TEntity, TKey>> candidates, Query<TEntity> query)`. This method allows creations in batch from a collection.

# 2.2.3
## Bug fixs
 - `PluralizationService` was wrongly registered as singleton, which lead to corrupt service provider.

# 2.2.2
## Breaking changes
 - **Modification**: `IRightExpressionsHelper` was removed and replaced by `IRightExpressionsHelper<T> where T : class`. This change aims at simplifying override of behaviors regarding rights. To upgrade, you might need to split your existing helper into several classes, and register them.

# 2.2.1
## Bug fixs
 - Serialization failed in the absence of requested fields.

# 2.2.0
## Breaking changes
 - **Modification**: Serialization logic has been upgraded to latest Lucca standard. Existing engine has been removed and replaced, which also fixes several bugs.
 - **Modification**: `IPropertySelector.Children` -> `IPropertySelector.Child`. Since `2.1-rc3`, this field was wrongly referencing an enumeration, when a `IPropertySelector` could only have one child.
## New features
 - **Added**: `IPropertySelector.Name`
 - **Added**: `IEnumerable<object> ISelection.GetItems()`.
 - **Added**: `PropertySelector` now implements `IPropertySelector` as expected.

# 2.1
## Bug fixs
 - Fixes an issue with serialization of nested enumerations introduced with `2.1-rc3`
 - An issue with sourcelink in app veyor

# 2.1-rc11
## Bug fixs
 - Issues with CI and msbuild

# 2.1-rc10
## New features
 - **CI**: Builds are now sourcelinked in app veyor

# 2.1-rc9
## Breaking changes
 - **Modification**: the term `Rdd` has been replaced by `RDD` in the different classes and methods of the project.
 - **Modification**: `IPrincipal` now exposes `PrincipalType Type { get; }`, and no longer exposes methods `HasOperation`, `HasAnyOperations`, `GetOperations` and `ApplyRights`. Those methods are now handled by a `IRightExpressionsHelper`. Also, this type needs to be registered in the injection dependency framework. 
 - **Modification**: `ReadOnlyWebController`, `WebController` and `ApiHelper` constructor has been modified to include an `IRDDSerializer` that handles the serialization.
 - **Modification**: `ReadOnlyRepository` implementation of `ApplyRights` now uses an `IRightExpressionsHelper`. This modifies the class and `Repository` constructor.
 - **Modification**: `IRepository<TEntity> ReadOnlyRestCollection.Repository` -> `IReadOnlyRepository<TEntity> ReadOnlyRestCollection.Repository`. This member type has been restrained.
 - **Removed**: `ReadOnlyRepository.GetOperationIds`. Replaced by the injection of `IRightExpressionsHelper`.
 - **Removed**: `RestCollection.InstanciateEntity`. Replaced by the injection of `IInstanciator<TEntity> Instanciator`.
 - **Removed**: `IExecutionContext`, `IExecutionModeProvider` and `ExecutionMode`. This is replaced by the direct injection of `IPrincipal`. `Metadata` and `MetadataHeader` constructors have been modified accordingly.
 - **Removed**: `ICombinationsHolder`. You might find a similar interface in `Lucca.Core.Rights.Integrations.Rdd`.
 - **Removed**: `query.Options.AttachOperations`, `ReadOnlyRestCollection.AppendOperationsToEntities` and `ReadOnlyRestCollection.AttachOperations`. This feature no longer exists as it wasn't correctly implemented.
 - **Removed**: `query.Options.AttachActions` and `ReadOnlyRestCollection.AttachActions`. This feature no longer exists as it wasn't correctly implemented.
 - **Removed**: `ReadOnlyRestCollection.FilterRights`. Replaced by the injection of `IRightExpressionsHelper`.
 - **Removed**: `RestCollection.CheckRightsForCreateAsync`. Replaced by `CreateAsync` overrides.
 - **Removed**: `IIdable<out Key>` as this was a duplicate of `IPrimaryKey`.
 - **Removed**: `IDependencyInjectionResolver`, `IMailService`. Unused
 - **Removed**: `BooleanExpression` and `ParameterChanger`. Thoss out of date services should be replaced by the ones in `Nextends`.

## New features
 - **Added**: `PrincipalType` enum 
 - **Added**: `IInstanciator<TEntity>` interface. This interface responsability is to instanciate a new entity.
 - **Added**: `IRightExpressionsHelper` interface. This interface responsability is to generate a filter from a `Query` that embodies the rights of the current principal.
 - **Added**: `IRDDSerializer` interface. This interface responsability is to serialize an input, given a `Query`.
 - **Added**: `ICandidate<TEntity>` as a lighter and easier version of `ICandidate<TEntity, TKey>`
 - **Added**: the class `RDDServiceCollectionExtensions` now also exposes `AddRDDCore`, `AddRDDRights`, `AddRDDSerialization` to allow for finer registration.
 - **Modification**: `ICandidate<TEntity, TKey>` constraint have been relaxed from `TEntity : IEntityBase<TKey>` to `IPrimaryKey<TKey>`
 - **Modification**: `PredicateService<TEntity, TKey>` constraint have been relaxed from `TEntity : class, IEntityBase<TEntity, TKey>` to `IPrimaryKey<TKey>`
 - **Modification**: `QueryBuilder<TEntity, TKey>` constraint have been relaxed from `TEntity : class, IEntityBase<TEntity, TKey>` to `IPrimaryKey<TKey>`
 - **Modification**: `WebFiltersContainer<TEntity, TKey>` constraint have been relaxed from `TEntity : class, IEntityBase<TEntity, TKey>` to `IPrimaryKey<TKey>`
 - **Modification**: `ApiHelper<TEntity, TKey>` constraint have been relaxed from `TEntity : class, IEntityBase<TEntity, TKey>` to `IEntityBase<TKey>`
 - **Modification**: `QueryFactory<TEntity, TKey>` constraint have been relaxed from `TEntity : class, IEntityBase<TEntity, TKey>` to `IPrimaryKey<TKey>`
 - **Modification**: `WebFiltersParser<TEntity>` constraint have been relaxed from `TEntity : class, IEntityBase` to `class`
 - **CI** : move to dotnet cli

## Bug fixs
 - `Thread.CurrentThread` is no longer used as it could create issues in edge cases
 - `MetadataHeader` members now respect PascalCase convention.
 - `AddRdd` failed in some instances
 - xUnit warnings

# 2.1-rc8
## New features
 - **CI** : add unstable nugets to myget
## Bug fixs
 - List serialization in some cases
 
# 2.1-rc7
## Breaking changes
 - **Modification**: Microsoft.AspNetCore "2.1.1" -> "2.1.2" 

# 2.1-rc6 /!\ incomplete
## Breaking changes
 - **Modification**: `PropertySelector` structure in now that of a chain, and no longer a tree. Its members change accordingly.
 - **Modification**: `Field` structure has been changed as well to reflect `PropertySelector` modifications. Its constructor now requires a `PropertySelector`.
 - **Modification**: `Query.Fields` and `Query.CollectionFields` are now ``IEnumerable<Field>``.
 - **Modification**: `Filter<TEntity>` and `WebFilter<TEntity>` have been distinguished. The first is very similar to an expression, when the later is closer to what used to be `Filter<TEntity>`
 - **Modification**: `List<Filter<TEntity>> Query.Filters` -> `Filter<TEntity> Query.Filter`. Constructors have been changed accordingly.
 - **Removed**: `Filter<TEntity, TProperty>`. Unused
 - **Removed**: `ReadOnlyRepository.PrepareAsync`. forgotten unused function
## New features
## Bug fixs
 - RDD.Domain no longer references LinqKit.Microsoft.EntityFrameworkCore
 - `EnumerateEntitiesAsync` in some cases

 //missing

 
## 1.2

### Breaking changes
- `Forge` on entities is replaced by `ForgeEntity` on collections, in which the data context is larger. Resolves [#18](https://github.com/LuccaSA/RestDrivenDomain/issues/18)

### New features
- From MSTest/NUnit to xUnit for test projects
- Support for Abstract entity RestCollection. Resolves [#28](https://github.com/LuccaSA/RestDrivenDomain/issues/28)

### Resolved issues
- Resolving dependencies with parameter constr. Resolves [#27](https://github.com/LuccaSA/RestDrivenDomain/issues/27)
- Unauthorized Exception when GUID sent in headers is malformed. Resolves [#10](https://github.com/LuccaSA/RestDrivenDomain/issues/10)

## 1.1.3

### New features
- Add `Headers` property to the Query<T> to store the webrequest headers

## 1.1.2

### Resolved issues
- FIX duplicate log in `SmtpMailService.SendMail()` method

## 1.1.1

### New features
- Update NExtends 1.0.7

## 1.1

### New features
- `GetEntityAfterCreate` method added in IRestCollection. It defines how the created entity should be returned to the client. Useful when the entity is not persisted in DB.

### Breaking changes
- `Operations` and `Actions` are renamed into `AuthorizedOperations` and `AuthorizedActions`.

## 1.0.11

### New features
- IDependencyInjectionResolver, compatibility with SimpleInjector. This is useful to create any custom resolver in RDD based on SimpleInjector. See [#16](https://github.com/LuccaSA/RestDrivenDomain/issues/16) for more details.
- The `AsyncService.ContinueAsync()` method now returns the `Task` instead of `void`
- `CheckRightsForCreate` now works!

## 1.0.10

### New features
- IStorageService.AddAfterCommitAction(), ability to perform Action after the Commit(), if successful. This is useful when you want to condition the call of tird party HTTP services to the sucess of the local Commit() against your database.
- IExecutionMode, holds the execution context of the application (dev, test, production, ..). In production ou prerelease mode, stacktrace of errors are not shown. Errors are now sanitized (camelCase, least number of properties returned)
- ADD BooleanExpression from Lucca
- TestBootstrapper now sets the LostLogService as the default ILogService
- NotFoundException is more explicit than generic HttpLikeException
- ADD support for multi-DELETE (ie, DELETE on entities collection)

### Resolved issues
- FIX typo in HttpLikeException
- FIX [#8](https://github.com/LuccaSA/RestDrivenDomain/issues/8), [#13](https://github.com/LuccaSA/RestDrivenDomain/issues/13), [#14](https://github.com/LuccaSA/RestDrivenDomain/issues/14)

### Breaking changes
- HttpLikeException does not handle args after the message, so you have to String.Format() yourself the message with the args, and then call the constructor with only the message parameter.
- In yours tests, if you instantiate a User, you now HAVE TO set a cultureId to that user `new User { CultureId = 106 }`
- Errors are now in camelCase, and only contains (status, message, data, stackTrace)

## 1.0.9 - RDD for WS BI

### New features
 - Add support for MongoDB database - implementing IStorageService
 - Add log and mail services - you can use FileLogService in order to log into files, or SmtpMailService in order to send mails via SMTP
 - HttpLikeException now logs each exception into the ILogService

### Breaking changes
- You have to register an ILogService. By default you can use the LostLogService, which does not log anything
`Resolver.Current().Register<ILogService>(() => new LostLogService());`

## 1.0.8

### Resolved issues
 - [#9](https://github.com/LuccaSA/RestDrivenDomain/issues/9), rouding was not working properly with floating numbers


## 1.0.7 - Post redirect Get strategy

### New features
 - After a Post, we no longer redirect to Get front door, but rather play `GetById()` method with POST Http verb behavior. This way you can customize the way an entity should be sent back to the client between a Post and a real Get.

### Breaking changes
 - Http verb of `Query<T>` is now properly set, so you might have `Unreachable entity type` errors if you don't properly handle right management. In this case, you have to override `FilterRights()` method on your collection.
 - Signature of `ApiHelper.CreateQuery()` now takes a second argument which is the Http Verb corresponding to the current Http Request.

## 1.0.2-6 - Lucca WebServices integration

### New features
 - Get/SetCookie pour faciliter le travail sur les Cookies dans le IWebContext
 - Dependency injection : on peut travailler sur une classe avec 1 param dans le constructeur
 - Expansion des Fields plus facile à utiliser
 - Authentication : on peut différencier une authentification Web vs API
 - On rend disponible le HttpVerb sur la query, et il devient facultatif dans certaines signatures
 - Query.Options est virtuel, vous pouvez créer votre propre Query avec vos propres Options
 - Principal.Id disponible au niveau de IPrincipal
 - On rend fonctionnelles les stopWatch et le principal dans les metadata des réponses d'API
 - Projet de Tests avec [NCrunch](https://www.nuget.org/packages/NCrunch.Framework/)
 - Implémentation des sum/min/max sur les collections, notamment via la classe [DecimalRounding](https://github.com/LuccaSA/RestDrivenDomain/blob/c98868a7044e059775509c727f2e5ab5d3d01b7e/RDD.Domain/Helpers/DecimalRounding.cs)
 - On traque les id des entités dans le InMemoryStorage, comme EF le fait avec les primary key auto increment SQL, pour leur donner un id différent de 0 au moment du Commit()
 - Web/Tests BootStrappers qui sont soit appelés depuis un Global.asax, soit depuis un Class/TestInitialize de tests, et qui permettent d'initialiser les dépendances
 - IRequestMessage/HttpRequestMessageWrapper pour mocker plus facilement la req HTTP dans les ApiController. Ca permet de tester le cycle complet d'un appel d'API
 - HandleSubIncludes fonctionnel, comme dans Lucca. Permet de transférer des includes d'une sous propriété lorsqu'on est sur la collection parent de l'entité.
 - On sait sérialiser une Culture
 - Support des decimals dans les sommes/min/max sur les Selections (source : https://github.com/LuccaSA/ilucca/commit/efd89c176dc73ac58fa5ecf94b457c0170fc391a)
 - On sait désérialiser un string en MailAddress, et donc on peut utiliser ce type dans des objets du Domain
 - Ajout du concept de WebService et gestion de leur authentification (ApiAuthorize)
 - AsyncService : Parallel tasks support
 - Basé sur NExtends 1.0.3 qui gère la désérialisation d'un string en Uri

### Breaking changes
 - Le FieldExpansionHelper n'est plus static, il faut revoir ses appels dans votre code
 - Votre implémentation de IPrincipal doit exposer .Id
 - MetadataHeader suppose que le ExecutionContext.Current est setté, et que son .principal aussi
 - Il faut renommer le BootStrapper
 - HttpContextWrapper est passé de RDD.Web à RDD.Infra (impact sur les usings)
 - Idem pour HttpExecutionContext
 - IPrimaryKey prend désormais un SetId(), à implémenter manuellement si une classe implémente directement IEntityBase

### Resolved issues
 - Le count sur les collections ne marchait pas
 - PropertySelector.Add() : on changeait une référence locale vers l'élément dans la collection, mais pas la référence vers l'élément depuis la collection. Attention, le child se retrouve en dernier dans la collection, en espérant que l'ordre ne soit pas un pb
 - issue #3
 - issue #4 - cependant pas encore de lien previous/next
 - On n'utilise que NUnit, pas MSTest, sinon ça casse le build de myGet

## 1.0.1 - Integrate with WS Auth

### Resolved issues
 - [#1](https://github.com/LuccaSA/RestDrivenDomain/issues/1) - DELETE not working

## 1.0.0 - Integrate with Hangfire

### New features
 - Manual Dependency Injection, waiting for Unity and StructureMap bootStrappers
