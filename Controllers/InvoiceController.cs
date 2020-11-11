
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.Mvc;
using InvoiceApplication.DAL;
using InvoiceApplication.Interface;
using InvoiceApplication.MEF_Export;
using InvoiceApplication.Models;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNetCore.Identity;

namespace InvoiceApplication.Controllers
{
    [Export(typeof(MefControllerFactory))]
    public class InvoiceController : Controller
    {
        private InvoiceContext db = new InvoiceContext();
        protected ApplicationDbContext ApplicationDbContext { get; set; }
        protected Microsoft.AspNet.Identity.UserManager<ApplicationUser> UserManager { get; set; }


        [Import(typeof(ITax))]
        ITax tax;

        // GET: Invoice
      
        public ActionResult Index()
        {
          
            IList<Invoice> invoices = db.Invoices.Include(c => c.Products).ToList();
            return View(invoices);
        }
       

        // GET: Invoice/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }

        // GET: Invoice/Create
        public ActionResult Create()
        {
            var invoice = new Invoice();
            invoice.Products = new List<Product>();
            PopulateProductOnListData(invoice);
            return View();
        }

        // POST: Invoice/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "InvoiceID,NumberOf,CreateDate,BillingDeadline,TotalPrice,TotalPriceWithTax,Buyer,Quantity")] Invoice invoice,string[] selectedProducts)
        {
            if (selectedProducts != null)
            {
                
                invoice.Products = new List<Product>();
                             
                foreach (var product in selectedProducts)
                {
                    var productToAdd = db.Products.Find(int.Parse(product));
                        invoice.Products.Add(productToAdd);
                }
            }

            if (ModelState.IsValid)
            {
                this.ApplicationDbContext = new ApplicationDbContext();
                this.UserManager = new Microsoft.AspNet.Identity.UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this.ApplicationDbContext));
                ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
                
                invoice.MyUser = new MyUser { UserName=user.UserName,Address = user.Address, FullName = user.FullName,Email=user.Email };
       
                if (invoice.Products != null)
                {
                    foreach (Product product in invoice.Products)
                    {
                        invoice.TotalPrice += product.TotalPrice;
                    }
                }
                tax = new TaxCalculate();
                invoice.TotalPriceWithTax = this.tax.CalculateTax("HR", invoice.TotalPrice);
                PopulateProductOnListData(invoice);
                db.Invoices.Add(invoice);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            
            return View(invoice);
        }

        // GET: Invoice/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices
             .Include(i => i.Products)
             .Where(i => i.InvoiceID == id)
             .Single();
            PopulateProductOnListData(invoice);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            PopulateProductOnListData(invoice);
            return View(invoice);
        }

        // POST: Invoice/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "InvoiceID,NumberOf,CreateDate,BillingDeadline,TotalPrice,TotalPriceWithTax")] Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                db.Entry(invoice).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(invoice);
        }

        // GET: Invoice/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }

        // POST: Invoice/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Invoice invoice = db.Invoices.Find(id);
            db.Invoices.Remove(invoice);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);

        }
        private void PopulateProductOnListData(Invoice invoice)
        {
            var allProducts = db.Products;
            var invoiceProducts = new HashSet<int>(invoice.Products.Select(c => c.ProductID));
            var viewModel = new List<ProductOnList>();
            foreach (var product in allProducts)
            {
                viewModel.Add(new ProductOnList
                {
                    InvoiceID= product.ProductID,
                    Name = product.Name,
                    Price=product.Price,
                    Quantity=product.Quantity,
                    TotalPrice=product.TotalPrice,
                    Assigned = invoiceProducts.Contains(product.ProductID)
                });
            }
            ViewBag.Products = viewModel;
        }
        private void UpdateInvoiceProducts(string[] selectedProducts, Invoice invoiceToUpdate)
        {
            if (selectedProducts == null)
            {
                invoiceToUpdate.Products = new List<Product>();
                return;
            }

            var selectedCoursesHS = new HashSet<string>(selectedProducts);
            var invoiceProducts = new HashSet<int>
                (invoiceToUpdate.Products.Select(c => c.ProductID));

            foreach (var product in db.Products)
            {
                if (selectedCoursesHS.Contains(product.ProductID.ToString()))
                {
                    if (!invoiceProducts.Contains(product.ProductID))
                    {
                        invoiceToUpdate.Products.Add(product);
                    }
                }
                else
                {
                    if (invoiceProducts.Contains(product.ProductID))
                    {
                        invoiceToUpdate.Products.Remove(product);
                    }
                }
            }
        }
    }
}
