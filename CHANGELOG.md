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

### Breaking changes
 - Le FieldExpansionHelper n'est plus static, il faut revoir ses appels dans votre code
 - Votre implémentation de IPrincipal doit exposer .Id
 - MetadataHeader suppose que le ExecutionContext.Current est setté, et que son .principal aussi

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
 
