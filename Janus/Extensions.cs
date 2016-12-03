using System.Collections.Generic;
using System.Text;

namespace Janus
{
    public static class Extensions
    {
        public static List<string> SplitEscapable(this string input, char splitter)
        {
            var list = new List<string>();
            var i = 0;
            var buffer = new StringBuilder();
            while(i < input.Length)
            {
                if (input[i] == '\\')
                {
                    if (i < input.Length - 1 && input[i + 1] == splitter)
                    {
                        buffer.Append(splitter);
                        i+=2;
                        continue;
                    }
                }
                if (input[i] == splitter)
                {
                    if (buffer.Length == 0)
                    {
                        i++;
                        continue;
                    }
                    list.Add(buffer.ToString());
                    buffer = new StringBuilder();
                }
                else
                {
                    buffer.Append(input[i]);
                }
                i++;
            }

            if (buffer.Length > 0)
            {
                list.Add(buffer.ToString());
            }

            return list;
        }
    }
}
