using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class Pair<First, Second>
    {
        public First Item1 { get; set; }
        public Second Item2 { get; set; }

        public Pair(First f, Second s)
        {
            Item1 = f;
            Item2 = s;
        }
    }
}
