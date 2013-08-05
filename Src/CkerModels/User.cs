using System;

namespace Cker.Models
{
    public enum UserType
    {
        Administrator,
        Operator,
    }

    public class User
    {
        public UserType Type { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ImageSourcePath { get; set; }
    }
}
