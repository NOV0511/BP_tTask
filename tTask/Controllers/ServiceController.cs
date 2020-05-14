using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tTask.Models;
using tTask.ORM;
using tTask.ORM.DAO;
using tTask.ORM.DTO;
using tTask.ViewModels;

namespace tTask.Controllers
{
    [Authorize(Roles = nameof(Roles.DomainAdmin))]
    [Authorize(Policy = "TenantPolicy")]
    public class ServiceController : Controller
    {
        private readonly ServiceOrderTable _serviceOrderTable;
        private readonly TenantTable _tenantTable;
        private readonly ProjectTable _projectTable;
        private readonly UserTable _userTable;
        private readonly PaymentTable _paymentTable;

        public ServiceController(ServiceOrderTable serviceOrderTable, TenantTable tenantTable, ProjectTable projectTable, UserTable userTable, PaymentTable paymentTable)
        {
            _serviceOrderTable = serviceOrderTable;
            _tenantTable = tenantTable;
            _projectTable = projectTable;
            _userTable = userTable;
            _paymentTable = paymentTable;
        }
        public IActionResult Index()
        {
            var model = new ServiceViewModel()
            {
                ServiceTenantOrder = _serviceOrderTable.GetNewestServiceOrderToTenantByTenantId(_tenantTable.GetTenantId(HttpContext.Items["domain"] as string)),
                Basic = _serviceOrderTable.GetServiceById((int)Services.Basic),
                Pro = _serviceOrderTable.GetServiceById((int)Services.Pro),
                Business = _serviceOrderTable.GetServiceById((int)Services.Business)
            };
            return View(model);
        }

        [HttpGet]
        public IActionResult GetTenantNumbers()
        {
            var output = new TenantNumbers()
            {
                NOProjects = _projectTable.GetNOProject(),
                NOUsers = _userTable.GetNOUsers()
            };
            return Json(output);
        }

        [HttpGet]
        public IActionResult GetService(int idService)
        {
            var output = _serviceOrderTable.GetServiceById(idService);
            return Json(output);
        }

        [HttpPost]
        public IActionResult Pay(int idService)
        {
            var service = _serviceOrderTable.GetServiceById(idService);
            var idTenant = _tenantTable.GetTenantId(HttpContext.Items["domain"] as string);
            var so = _serviceOrderTable.GetNewestServiceOrderToTenantByTenantId(idTenant);

            var dayDifference = 0;

            if(so.IdService == idService)
            {
                if (so.IdPaymentNavigation.Paid.HasValue)
                {
                    dayDifference = (int)(so.IdPaymentNavigation.Paid.Value.AddDays(30) - DateTime.Now).TotalDays;
                }
            }
            dayDifference = dayDifference > 0 ? dayDifference : 0;

            var payment = new Payment()
            {
                IdPayment = _paymentTable.GetMaxId(),
                Paid = DateTime.Now.AddDays(dayDifference),
                Price = (int)service.Price
            };

            var serviceOrder = new ServiceOrder()
            {
                IdPayment = payment.IdPayment,
                IdService = service.IdService,
                IdTenant = idTenant,
                OrderDate = DateTime.Now
            };

            _paymentTable.NewPayment(payment);
            _serviceOrderTable.NewServiceOrder(serviceOrder);

            return RedirectToAction("Index", "Profile");
        }
    }
}