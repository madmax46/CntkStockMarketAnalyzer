using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarketAnalyzer
{
    public enum NetworkState : int
    {
        NotCreated = 0,
        Created = 1,
        Train,
        Test,
        Ready
    }
}
