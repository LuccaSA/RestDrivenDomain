using Newtonsoft.Json;
using NExtends.Primitives.Strings;
using NExtends.Primitives.Types;
using RDD.Domain.Exceptions;
using RDD.Domain.Models.Querying;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace RDD.Domain.Helpers
{
    public class PatchEntityHelper
    {
        /// <summary>
        /// Utilisé pour setter les props d'une entité lors d'un POST ou un PUT
        /// Et récursivement pour les sous entités
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="datas"></param>
        public void PatchEntity(object entity, PostedData datas)
        {
            Type entityType = Nullable.GetUnderlyingType(entity.GetType()) ?? entity.GetType();
            PropertyInfo[] props = entityType.GetProperties();

            //On modifie propriété par propriété
            foreach (string key in datas.Keys)
            {
                PropertyInfo property = props.FirstOrDefault(p => p.Name.ToLower() == key);

                if (property == null)
                {
                    throw new BadRequestException(string.Format("Property {0} does not exist on type {1}", key, entityType.Name));
                }

                //Si la propriété n'est pas publique, alors on indique qu'on ne peut pas la modifier
                MethodInfo propertySetter = property.GetSetMethod();

                if (propertySetter == null)
                {
                    throw new BadRequestException(string.Format("Property {0} of type {1} is not writable", property.Name, entityType.Name));
                }

                Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                Type subType = propertyType.GetEnumerableOrArrayElementType();

                //Si c'est un type du domaine, on doit aller chercher les entités via leur repo, pour garantir qu'ils seront récupérés via le contexte courant (et pas créé au moment du commit)
                if (typeof(IEntityBase).IsAssignableFrom(subType))
                {
                    Type entityBaseType = subType.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEntityBase<>));
                    if (entityBaseType != null)
                    {
                        //Collection de Type du domaine (1-N ou N-N)
                        if (propertyType.IsGenericType)
                        {
                            // Si on envoi un tableau d'objets non vides, on récupère ces objets puis on les affecte à la propriété
                            // Le else est utilisé pour gérer l'écrasement lorsqu'on envoi un tableau vide
                            if (datas[key].Subs.Values.Any())
                            {
                                Func<PostedData, IEnumerable<object>> entitiesGetter = GetSimpleEntitiesGetter(datas[key].Subs.Values.First(), subType, entityBaseType);

                                IEnumerable<object> attachedEntities = entitiesGetter(datas[key]);
                                object values = new SerializationService().CastEnumerableIntoStrongType(property.PropertyType, attachedEntities);
                                propertySetter.Invoke(entity, new[] {values});
                            }
                            else
                            {
                                object values = new SerializationService().CastEnumerableIntoStrongType(property.PropertyType, new HashSet<object>());
                                propertySetter.Invoke(entity, new[] {values});
                            }
                        }
                        else //Type du domaine (N-1)
                        {
                            Func<PostedData, object> entityGetter = GetSimpleEntityGetter(datas[key], subType, entityBaseType);

                            object attachedEntity = entityGetter(datas[key]);
                            propertySetter.Invoke(entity, new[] {attachedEntity});
                        }
                    }
                }
                else if (
                    propertyType.IsGenericType
                    && propertyType.GetInterface("IEnumerable") != null // Il ne suffit pas d'être générique pour être une collection (eg EnumClient<UneEnum>)
                    && propertyType.GetGenericTypeDefinition() != typeof(Dictionary<,>)
                    || propertyType.IsArray // Les types [] ne sont pas des Generic mais doivent être traités de la même manière
                ) //Collection d'objets qui ne sont pas du Domaine (ou complexType)
                {
                    string jsonString = string.Join(",", datas[key].Subs.Values.Select(v => v.RawObject != null ? v.RawObject.ToString() : (v.Value != null ? v.Value.ToString() : "null")));

                    if (property.PropertyType.IsEnumerableOrArray())
                    {
                        jsonString = "[" + jsonString + "]";
                    }
                    object propValue = JsonConvert.DeserializeObject(jsonString, property.PropertyType);
                    propertySetter.Invoke(entity, new[] {propValue});
                }
                else if (datas[key].HasSubs) //Type non générique mais potentiellement complexType
                {
                    //On récupère le sous objet
                    object subEntity = property.GetValue(entity);

                    //S'il est NULL, il faut l'instancier
                    if (subEntity == null)
                    {
                        subEntity = Activator.CreateInstance(propertyType);
                    }

                    // Si c'est un dictionnaire on patch différemment
                    // Il faut associer les valeurs à une clé et non à une propriété de l'objet
                    if (propertyType.IsSubclassOfInterface(typeof(IDictionary)))
                    {
                        var dictionary = subEntity as IDictionary;

                        // Si la classe hérite de Dictionary on prend le base type
                        if (!propertyType.IsGenericType)
                        {
                            propertyType = propertyType.BaseType;
                        }
                        // On récupère les génériques
                        Type[] genericArguments = propertyType.GetGenericArguments();

                        // On construit l'objet
                        foreach (KeyValuePair<string, PostedData> sub in datas[key].Subs)
                        {
                            // Une clé JSON est forcément une string - voir http://json.org/
                            // En revanche côté serveur il peut être utile d'avoir des dictionnaires de int,
                            // c'est pourquoi on convertit la clé vers genericArguments[0] au lieu de string
                            object dicKey = sub.Key.ChangeType(genericArguments[0], CultureInfo.InvariantCulture);
                            object dicValue = sub.Value.RawObject != null ? JsonConvert.DeserializeObject(sub.Value.RawObject.ToString(), genericArguments[1]) : null;

                            // Si c'est un type valeur
                            if (dicValue == null && genericArguments[1].IsValueType)
                            {
                                if (sub.Value.Value != null)
                                {
                                    // On ne peut pas convertir un String en Nullable<int>
                                    // On récupère donc le générique (par exemple int)
                                    if (genericArguments[1].IsGenericType)
                                    {
                                        dicValue = sub.Value.Value.ChangeType(genericArguments[1].GenericTypeArguments[0], CultureInfo.InvariantCulture);
                                    }
                                    else
                                    {
                                        dicValue = sub.Value.Value.ChangeType(genericArguments[1], CultureInfo.InvariantCulture);
                                    }
                                }
                                // Il est impossible de setter à NULL une propriété non nullable
                                else if (!genericArguments[1].IsTypeNullable())
                                {
                                    throw new BadRequestException(string.Format("You cannot set a non nullable value to NULL (Property {0})", key));
                                }
                            }

                            dictionary[dicKey] = dicValue;
                        }

                        // Enfin, on patch
                        propertySetter.Invoke(entity, new[] {dictionary});
                    }
                    else
                    {
                        PatchEntity(subEntity, datas[key]);

                        propertySetter.Invoke(entity, new[] {subEntity});
                    }
                }
                else //Type simple, on envoie la valeur
                {
                    propertySetter.Invoke(entity, new[] {datas[key].Value == null ? null : datas[key].Value.ChangeType(propertyType, CultureInfo.InvariantCulture)});
                }
            }
        }

        /// <summary>
        /// Permet d'aller chercher 1 sous entité dans le cadre d'une relation N-1 par exemple
        /// </summary>
        /// <param name="entityData"></param>
        /// <param name="subType"></param>
        /// <param name="entityBaseType"></param>
        /// <returns></returns>
        protected virtual Func<PostedData, object> GetSimpleEntityGetter(PostedData entityData, Type subType, Type entityBaseType)
        {
            //TODO
            //if (entityData.ContainsKey("id"))
            //{
            //    var keyType = entityBaseType.GetGenericArguments()[1];
            //    var repo = _provider.TryGetRepository(_storage, subType, keyType, null);
            //    return (s) => repo.TryGetById(Convert.ChangeType(s["id"].value, keyType));
            //}
            //else
            //{
            return s => s.RawObject.ToObject(subType);
//            }
        }

        /// <summary>
        /// Permet d'aller chercher N entités d'un coup dans le cadre d'une relation 1-N
        /// </summary>
        /// <param name="firstOfArray"></param>
        /// <param name="subType"></param>
        /// <param name="entityBaseType"></param>
        /// <returns></returns>
        protected virtual Func<PostedData, IEnumerable<object>> GetSimpleEntitiesGetter(PostedData firstOfArray, Type subType, Type entityBaseType)
        {
            //TODO
            //if (firstOfArray.ContainsKey("id"))
            //{
            //    var keyType = entityBaseType.GetGenericArguments()[1];
            //    var repo = _provider.TryGetRepository(_context, subType, keyType, null);
            //    return (s) => repo.TryGetByIds(s.subs.Values.Select(el => Convert.ChangeType(el["id"].value, keyType)));
            //}
            //else
            //{
            return s => s.Subs.Values.Select(el => el.RawObject.ToObject(subType));
            //    }
        }
    }
}