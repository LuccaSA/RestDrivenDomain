using NExtends.Primitives.DateTimes;
using NExtends.Primitives.Strings;
using NExtends.Primitives.Types;
using RDD.Domain.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Serialization;

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
                throw new SerializationException(string.Format("Property {0} does not exist on type {1}", propertyName, declaringType.Name));
            }
            PropertyInfo baseProperty = declaringType.GetPublicProperties().FirstOrDefault(p => p.Name.ToLower() == elements[0]);
            if (baseProperty != null)
            {
                return ConvertWhereValues(values, baseProperty.PropertyType, string.Join(".", elements.Skip(1)));
            }
            throw new SerializationException(string.Format("Property {0} does not exist on type {1}", elements[0], declaringType.Name));
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

                if (correctPropertyType == typeof(Guid))
                {
                    //In case stringValue is incomplete (ie: aabbccdd-eeff in a like clause)
                    //We feed it with 0-based Guid
                    return new GuidHelper().Complete(stringValue);
                }

                return realStringValue.ChangeType(correctPropertyType, culture);
            }
            catch
            {
                throw new SerializationException(string.Format("String {0} is not compatible with type {1}", realStringValue, propertyType.Name));
            }
        }

    }
}