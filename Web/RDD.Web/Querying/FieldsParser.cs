using Microsoft.AspNetCore.Http;
using RDD.Domain.Helpers.Expressions;
using RDD.Web.Serialization;

namespace RDD.Web.Querying
{
    public class FieldsParser : IFieldsParser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FieldsParser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public PropertyTreeNode ParseFields() => _httpContextAccessor.HttpContext.ParseFields();
    }

    public interface IFieldsParser
    {
        PropertyTreeNode ParseFields();
    }
}