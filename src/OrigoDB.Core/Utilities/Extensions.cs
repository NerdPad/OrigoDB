﻿using System;
using System.Collections.Generic;

namespace OrigoDB.Core
{
    public static class Extensions
    {

        public static UInt64 ParsePadded(this string number)
        {
            //Get rid of the leading zeros
            number = number.TrimStart('0');
            if (number == "") return 0;
            return UInt64.Parse(number);

        }

        public static Dictionary<string,string> ParseProperties(this string @string)
        {
            var dictionary = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            //Do some pre cleaning
            @string = @string.Trim();
            if (@string.EndsWith(";"))
            {
                @string = @string.Remove(@string.Length - 1);
            }

            var properties = @string.Split(';');
            foreach (string property in properties)
            {
                var pair = property.Split('=');
                if (pair.Length != 2) throw new InvalidOperationException("Invalid properties string");
                dictionary[pair[0]] = pair[1];
            }
            return dictionary;
        }
    }
}
