using NExtends.Attributes;
using NExtends.Primitives.Enums;
using System;
using System.Linq;

namespace Rdd.Domain.Tests.Models
{
    public abstract class EnumClient
    {
        public string Name { get { return CultureName(); } set { CultureName = () => value; } }
        internal Func<string> CultureName { get; set; }
    }

    public class EnumClient<TEnum> : EnumClient
        where TEnum : System.Enum
    {
        public EnumClient()
        { }

        public EnumClient(TEnum value)
            : this(value, () => GetNameFromEnumValue(value))
        { }

        public EnumClient(TEnum value, Func<string> label)
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("TEnum must be an enumerated type");
            }

            this.Value = value;
            this.CultureName = label;
        }

        /// <summary>
        /// Enum to serialise
        /// </summary>
        public TEnum Value { get; protected set; }

        /// <summary>
        /// Enum "int" identifier (stored in DB)
        /// </summary>
        public int Id
        {
            get { return Convert.ToInt32(Value); }
            set { SetValueWithoutLabel((TEnum)(object)Value); }
        }

        /// <summary>
        /// Enum "word" identifier
        /// </summary>
        public string Code
        {
            get { return Convert.ToString(Value); }
            set { SetValueWithoutLabel(EnumExtensions.Parse<TEnum>(value)); }
        }

        private void SetValueWithoutLabel(TEnum newValue)
        {
            // Used when deserialising JSON
            // If ID AND tag are both present in JSON, the last value will be set
            //  - {"tag": "invitation", "id": 1} => enum value corresponding to 1
            //  - {"id": 1, "tag": "invitation"} => enum value corresponding to "invitation"
            //
            // Unfortunately cannot detect if both value are specified and different when deserialising

            if (!newValue.Equals(this.Value))
            {
                this.Value = newValue;
                this.CultureName = () => GetNameFromEnumValue(newValue);
            }
        }

        // static so we can use it in constructor "this(...)"
        private static string GetNameFromEnumValue(TEnum enumValue)
        {
            var attribute = (CulturedDescriptionAttribute)typeof(TEnum).GetMember(enumValue.ToString())[0].GetCustomAttributes(typeof(CulturedDescriptionAttribute), false).FirstOrDefault();

            return attribute == null ? "<undefined>" : attribute.Description;
        }
    }
}
