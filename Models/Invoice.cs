
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace InvoiceApplication.Models
{
    public class Invoice
    {
        public int InvoiceID { get; set; }
        public int InvoiceNumber { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CreateDate { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime BillingDeadline { get; set; }
        public string Buyer { get; set; }

        public MyUser MyUser { get; set; }
        public ICollection<Product> Products { get; set; }
        public double TotalPrice { get; set; }
        public double TotalPriceWithTax { get; set; }
        public object Catagory { get; internal set; }
    }
}