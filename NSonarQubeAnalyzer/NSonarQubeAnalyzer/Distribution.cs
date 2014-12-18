using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSonarQubeAnalyzer
{
    public class Distribution
    {
        // TODO Do we want this to be immutable?
        public int[] Ranges { private set; get; }

        public int[] Values { private set; get; }

        public Distribution(params int[] ranges)
        {
            // TODO Check not empty, and sorted
            Ranges = ranges;
            Values = new int[ranges.Length];
        }

        public void Add(int value)
        {
            int i = Ranges.Length - 1;

            while (i > 0 && value < Ranges[i])
            {
                i--;
            }

            Values[i]++;
        }

        public override string ToString()
        {
            return string.Join(";", Ranges.Zip(Values, (r, v) => r.ToString() + "=" + v.ToString()));
        }
    }
}
