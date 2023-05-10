using apiappVK.Models.Enum;
using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace apiappVK.Models.Entity
{
    public class UserState
    {
        public int id { get; set; }
        public StateCode code { get; set; }
        public string description { get; set; }
    }
}
