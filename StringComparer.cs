using System.Collections.Generic;

namespace ConsoleApp1
{
    public class StringComparer : IComparer<string>
    {
        private const char Delimiter = '.';

        public int Compare(string s1, string s2)
        {
            var compareResult = string.CompareOrdinal(GetName(s1), GetName(s2));
            if (compareResult == 0)
            {
                var key1 = GetKey(s1);
                var key2 = GetKey(s2);
                return key1 == key2
                    ? 0
                    : key1 < key2 ? -1 : 1;
            }
            else
                return compareResult;
        }

        private string GetName(string str)
        {
            var delimiterIndex = str.IndexOf(Delimiter);
            return str.Substring(delimiterIndex + 1);
        }

        private int GetKey(string str)
        {
            var delimiterIndex = str.IndexOf(Delimiter);
            return int.Parse(str.Substring(0, delimiterIndex));
        }
    }
}