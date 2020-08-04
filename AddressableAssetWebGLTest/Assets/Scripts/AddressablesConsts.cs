using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WTC.Resource
{
    static class AddressablesConsts
    {
        public static string RuntimePath { get; set; }

        public static string ParseDynamicPath(string input)
        {
            string ret = input;
            string str = "{WTC.Resource.AddressablesConsts.RuntimePath}";
            if (ret.Contains(str))
            {
                ret = ret.Replace(str, WTC.Resource.AddressablesConsts.RuntimePath);
            }
            return ret;
        }
    }
}
