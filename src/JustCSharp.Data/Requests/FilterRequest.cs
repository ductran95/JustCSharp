using System;
using System.Collections;

namespace JustCSharp.Data.Requests
{
    public class FilterRequest: IRequest
    {
        public string Field { get; set; }
        public string ValueString { get; set; }
        public Guid? ValueGuid { get; set; }
        public DateTime? ValueDateTimeFrom { get; set; }
        public DateTime? ValueDateTimeTo { get; set; }
        public float? ValueNumberFrom { get; set; }
        public float? ValueNumberTo { get; set; }
        public bool? ValueBool { get; set; }
        public IEnumerable ValueList { get; set; }
    }
}