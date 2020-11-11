using InvoiceApplication.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web;

namespace InvoiceApplication.MEF_Export
{
    [Export(typeof(IOperation))]
    [ExportMetadata("CountryName","HR")]
    public class HRTax : IOperation
    {
        public double Operate(double priceToOperate)
        {
            return priceToOperate*1.25; 
        }
    }
}