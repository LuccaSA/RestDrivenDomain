using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using NExtends.Primitives;
using NExtends.Primitives.Types;

namespace RDD.Domain.Models.Querying
{
    public class SerializationService
    {
        /// <summary>
        /// Returns a strongly-typed list of values matching asked property type
        /// </summary>
        /// <param name="values"></param>
        /// <param name="declaringType"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public IList ConvertWhereValues(ICollection<string> values, Type declaringType, string propertyName)
        {
            //Si le type déclarant la propriété est une List ou un Array, on s'intéresse au type contenu dedans
            declaringType = declaringType.GetEnumerableOrArrayElementType();

            string[] elements = propertyName.Split('.');

            //Si on demande une propriété directe de T, alors on peut répondre
            if (elements.Length == 1)
            {
                PropertyInfo property = declaringType.GetPublicProperties().FirstOrDefault(p => p.Name.ToLower() == propertyName);
                if (property != null)
                {
                    return ConvertWhereValues(values, property);
                }
                throw new Exception(string.Format("Property {0} does not exist on type {1}", propertyName, declaringType.Name));
            }
            PropertyInfo baseProperty = declaringType.GetPublicProperties().FirstOrDefault(p => p.Name.ToLower() == elements[0]);
            if (baseProperty != null)
            {
                return ConvertWhereValues(values, baseProperty.PropertyType, string.Join(".", elements.Skip(1)));
            }
            throw new Exception(string.Format("Property {0} does not exist on type {1}", elements[0], declaringType.Name));
        }

        public IList ConvertWhereValues(ICollection<string> values, PropertyInfo property)
        {
            Type listConstructorParamType = typeof(List<>).MakeGenericType(property.PropertyType.GetEnumerableOrArrayElementType());
            var result = (IList) Activator.CreateInstance(listConstructorParamType);

            foreach (string value in values)
            {
                result.Add(TryConvert(value, property.PropertyType.GetEnumerableOrArrayElementType()));
            }
            return result;
        }

        private object TryConvert(string stringValue, Type propertyType)
        {
            string realStringValue = stringValue;

            //Cas particulier pour les mots clés
            switch (stringValue)
            {
                case "today":
                    realStringValue = DateTime.Today.ToISO();
                    break;
            }

            Type nullableType = Nullable.GetUnderlyingType(propertyType);

            try
            {
                if ((nullableType != null || !propertyType.IsValueType) && (string.IsNullOrEmpty(realStringValue) || realStringValue == "null"))
                {
                    if (realStringValue == string.Empty)
                    {
                        return string.Empty;
                    }
                    return null;
                }
                Type correctPropertyType = nullableType ?? propertyType;
                CultureInfo culture = CultureInfo.CurrentCulture;

                if (correctPropertyType == typeof(MailAddress))
                {
                    return new MailAddress(realStringValue);
                }

                //On permet, qq soit la culture du user, d'envoyer des doubles avec . ou , comme séparateur décimal
                if (correctPropertyType == typeof(double) || correctPropertyType == typeof(decimal))
                {
                    realStringValue = realStringValue.Replace(",", ".");
                    culture = CultureInfo.InvariantCulture;
                }

                if (correctPropertyType == typeof(DateTime))
                {
                    culture = CultureInfo.InvariantCulture;
                }


                return realStringValue.ChangeType(correctPropertyType, culture);
            }
            catch
            {
                throw new Exception(string.Format("String {0} is not compatible with type {1}", realStringValue, propertyType.Name));
            }
        }

        /// <summary>
        /// Permet de convertir un List of Utilisateur_SPOREOIREUR en APICollection of Utilisateur tout court
        /// </summary>
        /// <param name="propertyType"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public object CastEnumerableIntoStrongType(Type propertyType, IEnumerable<object> elements)
        {
            Type elementType = propertyType.GetEnumerableOrArrayElementType();

            //ON part d'une List<T> fortement typée
            Type stronglyTypedListType = typeof(List<>).MakeGenericType(elementType);
            var listResult = (IList) Activator.CreateInstance(stronglyTypedListType);

            foreach (object element in elements)
            {
                listResult.Add(element);
            }

            if (propertyType.IsArray)
            {
                Array arrayResult = Array.CreateInstance(elementType, listResult.Count);
                listResult.CopyTo(arrayResult, 0);
                return arrayResult;
            }

            Type genericTypeDefinition = propertyType.GetGenericTypeDefinition();

            if (genericTypeDefinition == typeof(List<>)
                || genericTypeDefinition == typeof(ICollection<>)
                || genericTypeDefinition == typeof(IEnumerable<>)
                || genericTypeDefinition == typeof(IList<>)
                || genericTypeDefinition == typeof(IReadOnlyList<>)
                || genericTypeDefinition == typeof(IReadOnlyCollection<>))
            {
                return listResult;
            }

            if (genericTypeDefinition == typeof(HashSet<>)
                || genericTypeDefinition == typeof(ISet<>))
            {
                Type stronglyTypedHashSetType = typeof(HashSet<>).MakeGenericType(elementType);
                return Activator.CreateInstance(stronglyTypedHashSetType, listResult);
            }

            if (genericTypeDefinition == typeof(ISelection<>))
            {
                return propertyType
                    .GetConstructor(new[] {typeof(ICollection<>).MakeGenericType(elementType)})
                    .Invoke(new object[] {listResult});
            }
            throw new Exception(string.Format("Unhandled enumerable type {0}", propertyType.Name));
        }
    }
}