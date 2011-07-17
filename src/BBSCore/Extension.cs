using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hammock;
using System.IO;

namespace BBSCore
{
    public static class Extension
    {
        public static string GetGBContent(this RestResponse response)
        {
            string content = null;

            try
            {
                using (var sr = new StreamReader(response.ContentStream, Encoding.GetEncoding("GB2312")))
                {
                    content = sr.ReadToEnd();
                }
            }
            catch
            {
                
            }

            return content;
        }
    }
}
