﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BankersCup.Models
{
    public class Player
    {
        public int PlayerId { get; set; }
        public string Name { get; set; }
        public string RegistrationCode { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
        public bool IsRemoved { get; set; }
    }
}
