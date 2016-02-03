# Change log

## 1.0.8

### Resolved issues
 - issue #9, rouding was not working properly with floating numbers


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

#### Dependencies
 - NExtends
 - Newtonsoft.Json
 - EntityFramework
 - LinqKit
 
