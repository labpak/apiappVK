using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace apiappVK.Models.Enum
{
    //[JsonConverter(typeof(StringEnumConverter))]
    public enum GroupCode
    {
        [Display(Name = "Администратор")]
        Admin = 0,
        [Display(Name = "Пользователь")]
        User = 1
    }
}
