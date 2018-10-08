namespace Rdd.Domain.Tests.Models
{
    public class CustomField : ICodable
    {
        public CustomField() { }

        public CustomField(string code, string name)
        {
            this.Code = code;
            this.Name = name;
        }

        public CustomField(string code) : this(code, code)
        { }

        public string Code { get; set; }
        private string _name { get; set; }

        // When set from API, only "Code" value is provided, so the NULL constructor is used and the Name property is NULL
        // To avoid that, by default we return the Code value if the Name is NULL
        public string Name
        {
            get
            {
                return _name ?? Code;
            }
            set
            {
                _name = value;
            }
        }
    }
}