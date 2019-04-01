using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OpenKuka.KRL.Test
{
    class Program
    {
        // https://github.com/zanders3/json/blob/master/src/JSONParser.cs
        // https://www.koderdojo.com/blog/depth-first-search-algorithm-in-csharp-and-net-core


        static void Main(string[] args)
        {
            var s1 = "{E6POS: X 1.2398, Y 192090, Z -1e989}";

            var krl = s1;
            for (int i = 0; i < krl.Length; i++)
            {
                char c = krl[i];

                if (c == '{')
                {
                    Console.WriteLine("{");


                }
            }

            Console.ReadKey();
        }
    }
}
