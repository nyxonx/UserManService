using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UserMan
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime Created { get; set; }
        public int DurationInHours { get; set; }
    }
}
