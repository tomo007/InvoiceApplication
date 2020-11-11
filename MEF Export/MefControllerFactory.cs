using InvoiceApplication.Controllers;
using InvoiceApplication.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Composition;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;

namespace InvoiceApplication.MEF_Export
{
    public class MefControllerFactory : DefaultControllerFactory
    {
        private CompositionContainer _container;


        public MefControllerFactory(Assembly assembly)
        {
            var catalog = new AggregateCatalog();
            // Adds all the parts found in the same assembly as the Program class.
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(InvoiceController).Assembly));

            // Create the CompositionContainer with the parts in the catalog.
            _container = new CompositionContainer(catalog);

            // Fill the imports of this object.
            try
            {
                this._container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Console.WriteLine(compositionException.ToString());
            }
        }

        #region IControllerFactory Members

        public IController CreateController(RequestContext requestContext, string controllerName)
        {

            var controller = _container.GetExportedValue<IController>(controllerName);

            if (controller == null)
            {
                throw new HttpException(404, "Not found");
            }

            return controller;
        }

        public void ReleaseController(IController controller)
        {
            // nothing to do
        }
      
        #endregion
    }
}