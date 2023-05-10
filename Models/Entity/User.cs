using apiappVK.Models.Enum;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace apiappVK.Models.Entity
{
    public class User
    {
        public int id { get; set; }
        public string login { get; set; }
        public string password { get; set; }
        public DateTime created_date { get; set; }
        public int user_group_id { get; set; }
        public int user_state_id { get; set; }
        //INSERT INTO Users VALUES (1, 'manul','1234', current_timestamp, 1, 1)

    }
}
