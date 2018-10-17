Pour passer à Rdd v3.0, indépendamment de la version de laquelle vous venez, vous devrez faire un certains nombre de modifications de masse dans votre projet.

**RDD => Rdd**

C'est sans doute la plus grosse modif, mais aussi la plus facile à gérer. Il suffit de faire un remplacer par dans l'ensemble de la solution de `RDD.` par `Rdd.`, cela va impacter tous vos `usings`.

**TEntity, TKey**

La plupart des classes qui avaient `TEntity, Tkey` comme arguments génériques ne prennent désormais plus qu'un `TKey`. Là y'a pas de miracle (sauf si vous êtes un regex master), il faut chercher tout ce qui commence par `EntityBase<` par ex et virer la classe correspondant au `TEntity`.

**IExecutionContext execution, ICombinationsHolder combinationsHolder**

Dans les controllers applicatifs et les collections, on se trimbalait jusqu'à présent ces 2 dépendances. Bonne nouvelle, vous pouvez les virer !

**IPatcherProvider => IPatcher<TEntity>, IInstantiator<TEntity>**

Vous êtes certainement aussi concerné par ce changement de logique : chaque entité est adossé à un patcher et à un instanciateur.

**Historique des modifs**

La version 3.0 introduit un gros breaking change sur la structure des projets, du coup toute navigation dans l'historique via GitHub semble vaine. C'est sans compter sur une extension miracle, cf https://github.com/LuccaSA/RestDrivenDomain/issues/235

**IPrincipal**

Le principal est désormais injectable directement, moyennant de gérer ça dans le startup notamment en utilisant Lucca.Core.Rights et de créer une interface IPrincipal dans votre Domain.

Ceci remplace le `IExecutionContext` et tout ce qui a trait au user connecté en général.

**IStorageService => IUnitOfWork**

Dans la couche Application, les AppControllers dépendent désormais d'un `IUnitOfWork` plutôt que d'un `IStorageService` pour commiter les modifs en base. Dans la pratique cela revient au même car le `EFStorageService` implémente les 2 interfaces. Mais si vous avez besoin de faire la différence dans votre projet c'est désormais possible.
