using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace apiappVK.Models.Enum
{
    //[JsonConverter(typeof(StringEnumConverter))]
    public enum StateCode
    {
        Blocked = 0,
        Active = 1
    }
}
