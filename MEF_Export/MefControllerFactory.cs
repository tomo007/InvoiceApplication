using InvoiceApplication.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace InvoiceApplication.MEF_Export
{
    [HandleError]
    public class MEFControllerFactory : DefaultControllerFactory
    {
        static CompositionContainer _container;
   
        static MEFControllerFactory()
        {
            var catalog = new AggregateCatalog();
            var path = AppDomain.CurrentDomain.BaseDirectory;
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(MEFControllerFactory).Assembly));
            _container = new CompositionContainer(catalog);

        }

        
        public override IController CreateController(RequestContext requestContext, string controllerName)
        {

            var controller = base.CreateController(requestContext, controllerName);

            _container.ComposeParts(controller);

            return controller;
        }
    }
}