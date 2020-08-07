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

        //public static string ParseRuntimeValues(string input)
        //{
        //    string ret = input;
        //    var output = input.Split('{', '}').Where((item, index) => index % 2 != 0).ToList();
        //    for (int i = 0; i < output.Count; i++)
        //    {
        //        var str = output[i];
        //        UnityEngine.Debug.Log("ParseRuntimeValues: " + str);



        //    }
        //    return ret;
        //}

        public static string ParseDynamicPath2(string input)
        {
            string ret = input;
            //ParseRuntimeValues(input);
            string str = "{WTC.Resource.AddressablesConsts.RuntimePath}";
            if (ret.Contains(str))
            {
                ret = ret.Replace(str, WTC.Resource.AddressablesConsts.RuntimePath);
            }
            return ret;
        }
    }
}
