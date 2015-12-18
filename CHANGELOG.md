# Change log

## 1.1.0 - Integrate with CloudControl

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

## 1.0.1 - Integrate with WS Auth

### Resolved issues
 - [#1](https://github.com/LuccaSA/RestDrivenDomain/issues/1) - DELETE not working

## 1.0.0 - Integrate with Hangfire

### New features
 - Manual Dependency Injection, waiting for Unity and StructureMap bootStrappers

#### Dependencies
 - NExtends
 - Newtonsoft.Json
 - EntityFramework
 - LinqKit
 
