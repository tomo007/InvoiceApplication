using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceApplication.Interface
{
    interface ITax
    {
        double CalculateTax(string countryName, double priceToCalculate);
    }
}
