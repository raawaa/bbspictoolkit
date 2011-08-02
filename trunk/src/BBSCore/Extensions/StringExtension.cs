using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBSCore.Extensions
{
    public static class StringExtension
    {
        public static string Encode(this string self)
        {
            var buff = Encoding.UTF8.GetBytes(self);

            return Convert.ToBase64String(buff);
        }

        public static string Decode(this string self)
        {
            var buff = Convert.FromBase64String(self);
            return Encoding.UTF8.GetString(buff);   
        }
    }
}
