using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactNetwork.Models
{
    [Table("Messages")]
    public class Message
    {
        public Message(string content, string from, string to, string date)
        {
            this.Content = content;
            this.FromName = from;
            this.ToName = to;
            this.SentDate = date;
            this.Status = "Unread";
        }

        public Message()
        {

        }

        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public string Content { get; set; }

        public string FromName { get; set; }

        public string ToName { get; set; }

        public string SentDate { get; set; }

        public string Status { get; set; }
    }
}
