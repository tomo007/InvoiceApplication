using InvoiceApplication.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web;

namespace InvoiceApplication.MEF_Export
{
    [Export(typeof(ITax))]
    public class TaxCalculate : ITax
    {
        [ImportMany]
        IEnumerable<Lazy<IOperation, IOperationData>> operations;
        public IEnumerable<Lazy<IOperation, IOperationData>> Operations
        {
            get
            {
                return operations;
            }
        }
  
        public double CalculateTax(string countryName, double priceToCalculate)
        {
            foreach (Lazy<IOperation, IOperationData> i in operations)
            {
                if (i.Metadata.CountryName.Equals(countryName))
                {
                    return i.Value.Operate(priceToCalculate);
                }
            }
            return 0;   
        }
    }
}