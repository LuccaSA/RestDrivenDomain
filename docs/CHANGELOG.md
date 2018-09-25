# Futur release (3.0)
## Breaking changes
 - **Removed**: All protected methods (`ProtectedGetAsync` / `ProtectedPostAsync` / `ProtectedPutAsync` / `DeleteByIdAsync`) are removed. Now you should directly override the methods you want to expose (like `public GetAsync()`) and decorate with routing attribute (ex `[HttpGet]`). This will allow swagger to discover all the Rdd apis.
 - **Removed**: Implicit routing is no longer available. Each route need to be routed by attribute **only**. 
 - **Modification**: Now the updated entity is a clone of the original entity, and is discarded if `ValidateEntity` throws an exception or returns false. This prevent unwanted commit of unvalidated changes.
 - **Modification**: `ValidateEntity` now return a `bool` to discard changes before update
 - **Modification**: `OnBeforeUpdateEntity` becomes `OnBeforePatchEntity`
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
 - **Modification**: Queries initiated with an expression now apply the maximum paging (1000) instead of the paging by default (10). To return to the previous behavior, the `Page` property needs to be explicitely set to the desired value.
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
 - **Added**: `ReadOnlyRepository<T>.IncludeWhiteList`. This allows an automatic white-list approach on the include on a Get query.
 - **Added**: New logic for property and member selection via expression coming from the web. The main interface to manipulate is `IExpression`, that replaces `PropertySelector`. The main implementation to use now are `PropertyExpression`, `EnumerablePropertyExpression`, `ItemExpression`, `ExpressionChain`, or `ExpressionTree`. The parsing and manipulation is ensured by `ExpressionParser` and the visitors `ExpressionChainExtractor`, `ExpressionChainer`.
 - **Added**: `ExpressionEqualityComparer` to compare `Expression` and another one to compare `IExpression`.
 - **Added**: readable static method to construct `OrderBy`
 - **Added**: implicit conversion from a `Filter<T>` to `Expression<T, bool>`.
 - **Added**: `IAppController.CreateAsync(IEnumerable<ICandidate<TEntity, TKey>> candidates, Query<TEntity> query)`. This method allows creations in batch from the controller.
 - **Added**: `IRestCollection.CreateAsync(IEnumerable<ICandidate<TEntity, TKey>> candidates, Query<TEntity> query)`. This method allows creations in batch from a collection.
 - **Added**: New AppController hook points : 
   - `OnBeforeCreate` / `OnAfterCreate`
   - `OnBeforeUpdate` / `OnAfterUpdate`
 - **Added**: New RestCollection hook points : 
   - `OnBeforeGetAsync` / `OnAfterGetAsync`
   - `OnBeforeCreate` / `OnBeforeUpdateEntity` / `OnBeforePatchEntity` / `OnAfterPatchEntity`

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