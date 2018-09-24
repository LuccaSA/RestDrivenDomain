using System.IO;
using Newtonsoft.Json;
using RDD.Domain.Helpers.Expressions;

namespace RDD.Web.Serialization
{
    public class TrackedJsonTextWriter : JsonTextWriter
    {
        public TrackedJsonTextWriter(TextWriter textWriter, PropertyTreeNode node)
            : base(textWriter)
        {
            _selectiveSerializationContext = new SelectiveSerialisationContext(node);
            SelectiveSerialisationContext.Current = _selectiveSerializationContext;
        }

        private readonly SelectiveSerialisationContext _selectiveSerializationContext;
        
        public override void WriteStartArray()
        {
            _selectiveSerializationContext.Push();
            base.WriteStartArray();
        }

        public override void WriteStartObject()
        {
            _selectiveSerializationContext.Push();
            base.WriteStartObject();
        }

        public override void WriteEndArray()
        {
            _selectiveSerializationContext.Pop();
            base.WriteEndArray();
        }

        public override void WriteEndObject()
        {
            _selectiveSerializationContext.Pop();
            base.WriteEndObject();
        }

        protected override void Dispose(bool disposing)
        {
            SelectiveSerialisationContext.Current = null;
            base.Dispose(disposing);
        }
    }
}