using System.IO;
using Newtonsoft.Json;

namespace RDD.Web.Serialization
{
    public class TrackedJsonTextWriter : JsonTextWriter
    {
        public TrackedJsonTextWriter(TextWriter textWriter, Node node)
            : base(textWriter)
        {
            SelectiveSerialisationContext.Current = new SelectiveSerialisationContext(node);
        }

        public override void WriteEndObject()
        {
            SelectiveSerialisationContext.Current.Pop();
            base.WriteEndObject();
        }

        public override void WriteEndArray()
        {
            SelectiveSerialisationContext.Current.Pop();
            base.WriteEndArray();
        }

        public override void WriteStartArray()
        {
            SelectiveSerialisationContext.Current.Push();
            base.WriteStartArray();
        }

        public override void WriteStartObject()
        {
            SelectiveSerialisationContext.Current.Push();
            base.WriteStartObject();
        }

        protected override void Dispose(bool disposing)
        {
            SelectiveSerialisationContext.Current = null;
            base.Dispose(disposing);
        }
    }
}