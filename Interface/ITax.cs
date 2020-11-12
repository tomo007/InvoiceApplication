using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceApplication.Interface
{
    public interface ITax
    {
        IEnumerable<Lazy<IOperation, IOperationData>> Operations { get;  }
        double CalculateTax(string countryName, double priceToCalculate);
    }
}
