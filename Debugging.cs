using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyFramework.Utility
{
    public class Debugging
    {
        #region Debug Logging
        public static string LogThings(object[] args)
        {
            return args.AsEnumerable()
                .Aggregate("", (acc, a) =>
                acc +=
                    "~~~~~~~~~~~~~~" + Environment.NewLine +
                    JsonConvert.SerializeObject(a, Formatting.Indented) + Environment.NewLine +
                    "~~~~~~~~~~~~~~" + Environment.NewLine
                );
        }

        public static string LogHeader(string header)
        {
            string output = "";
            for (var i = 0; i < 5; i++)
            {
                output = output + "~~~~~~~~~~~~~~" + Environment.NewLine;
            }
            output = output + header.ToUpper();
            for (var i = 0; i < 5; i++)
            {
                output = output + "~~~~~~~~~~~~~~" + Environment.NewLine;
            }
            return output;
        }
        #endregion
    }
}
