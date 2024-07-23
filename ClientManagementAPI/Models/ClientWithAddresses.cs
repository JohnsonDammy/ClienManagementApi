﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClientManagementAPI.Models
{
    public class ClientWithAddresses
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string Details { get; set; }
        public List<Address> Addresses { get; set; }
    }
}