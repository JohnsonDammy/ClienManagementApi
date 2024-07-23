using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClientManagementAPI.Models
{
    public class Contact
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ContactType { get; set; }
        public string ContactNumber { get; set; }
    }
}