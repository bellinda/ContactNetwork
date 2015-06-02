using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactNetwork.Models
{
    [Table("FriendsRequests")]
    public class FriendsRequest
    {
        public FriendsRequest(string from, string to)
        {
            this.FromName = from;
            this.ToName = to;
            this.Status = "Pending";
        }

        public FriendsRequest()
        {

        }

        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public string FromName { get; set; }

        public string ToName { get; set; }

        public string Status { get; set; }
    }
}
