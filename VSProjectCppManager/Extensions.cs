﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSProjectCppManager
{
    public static class Extensions
    {
        public static string RemoveChars(this string input, params char[] chars)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                if (!chars.Contains(input[i]))
                    sb.Append(input[i]);
            }
            return sb.ToString();
        }
    }
}
