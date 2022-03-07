using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarketAnalyzer
{
    public class ErrorInfo
    {
        public ErrorInfo(Exception exception, string additionalMsg)
        {
            this.exception = exception;
            AdditionalMsg = additionalMsg;
        }

        public Exception exception { get; set; }
        public string AdditionalMsg { get; set; }
    }
}
