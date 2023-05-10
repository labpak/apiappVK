using apiappVK.Models.Enum;
using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace apiappVK.Models.Entity
{
    public class UserGroup
    {
        public int id { get; set; }
        public GroupCode code { get; set; }
        public string description { get; set; }
    }
}
