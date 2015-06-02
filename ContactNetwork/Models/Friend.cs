using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactNetwork.Models
{
    [Table("Friends")]
    public class Friend
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Username { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string Skype { get; set; }

        public byte[] Image { get; set; }

        public int FriendToId { get; set; }
    }
}
