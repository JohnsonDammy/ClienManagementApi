using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClientManagementAPI.Models
{
    public class Address
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string AddressType { get; set; }
        public string AddressLine { get; set; }
    }
}