
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using InvoiceApplication.DAL;
using InvoiceApplication.Interface;
using InvoiceApplication.Models;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace InvoiceApplication.Controllers
{
    [Authorize]
    public class InvoiceController : Controller

    {
        [Import(typeof(ITax))]
        ITax tax;

        private InvoiceContext db = new InvoiceContext();
        protected ApplicationDbContext ApplicationDbContext { get; set; }
        protected Microsoft.AspNet.Identity.UserManager<ApplicationUser> UserManager { get; set; }


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
            ViewBag.Countries = GetTaxes();
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
        public ActionResult Create([Bind(Include = "InvoiceID,NumberOf,CreateDate,BillingDeadline,TotalPrice,TotalPriceWithTax,Buyer,Quantity")] Invoice invoice,string[] selectedProducts,string[] quantity,string country)
        {
            if (ModelState.IsValid)
            {
                invoice.MyUser = getMyUser();

                if (selectedProducts != null)
                {
                    invoice.Products = getSelectedProducts(selectedProducts, quantity);
                }

                if (invoice.Products != null)
                {
                    invoice.TotalPrice = getTotalPrice(invoice.Products);
                }
                
                invoice.TotalPriceWithTax =tax.CalculateTax(country, invoice.TotalPrice);
                invoice.CreateDate = DateTime.Now;

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

        public MyUser getMyUser()
        {
            this.ApplicationDbContext = new ApplicationDbContext();
            this.UserManager = new Microsoft.AspNet.Identity.UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this.ApplicationDbContext));
            ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());

            return new MyUser { UserName = user.UserName, Address = user.Address, FullName = user.FullName, Email = user.Email };
        }
        public List<Product> getSelectedProducts(string[] selectedProducts,string[] quantity) 
        {
            //get id of first member in list of products to sync with quantity field
            var firstProduct = db.Products.Find(int.Parse(selectedProducts[0]));
            int FirstMemberInListId = firstProduct.ProductID;

            List<Product> productList= new List<Product>();

            foreach (var product in selectedProducts)
            {
                var productToAdd = db.Products.Find(int.Parse(product));
                if (quantity != null)
                {
                    productToAdd.Quantity = Convert.ToInt32(quantity[productToAdd.ProductID - FirstMemberInListId]);
                    productToAdd.Quantity = Convert.ToInt32(quantity[productToAdd.ProductID - FirstMemberInListId]);
                    productToAdd.TotalPrice = productToAdd.Price * productToAdd.Quantity;
                }
                productList.Add(productToAdd);
            }
            return productList;
        }
        public double getTotalPrice(ICollection<Product> products)
        {
            double totalPrice = 0;
            foreach (Product product in products)
            {
                totalPrice += product.TotalPrice;
            }
            return totalPrice;
        }
     public List<string>  GetTaxes()
        {
            List<string> popis = new List<string>();
          
            foreach (var item in tax.Operations)
            {
                popis.Add(item.Metadata.CountryName);
            }
            return popis;
        }
    }
}
