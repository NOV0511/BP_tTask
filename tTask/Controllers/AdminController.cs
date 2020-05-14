using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tTask.ORM;
using tTask.ORM.DAO;
using tTask.ORM.DTO;
using tTask.ViewModels;

namespace tTask.Controllers
{
    [Authorize(Roles = nameof(Roles.Developer))]
    public class AdminController : Controller
    {
        private readonly TenantTable _tenantTable;
        private readonly ServiceOrderTable _serviceOrderTable;
        private readonly PaymentTable _paymentTable;

        public AdminController(TenantTable tenantTable, ServiceOrderTable serviceOrderTable, PaymentTable paymentTable)
        {
            _tenantTable = tenantTable;
            _serviceOrderTable = serviceOrderTable;
            _paymentTable = paymentTable;
        }
        public IActionResult Index()
        {
            var model = new AdminViewModel()
            {
                Tenants = _tenantTable.GetAllTenants()
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult Disable(int idTenant)
        {
            var payment = _serviceOrderTable.GetNewestServiceOrderToTenantByTenantId(idTenant).IdPaymentNavigation;
            payment.Paid = DateTime.Now.AddDays(-31);

            _paymentTable.UpdatePayment(payment);


            return RedirectToAction("Index");

        }

        [HttpPost]
        public IActionResult ChangeService(int idService, int addDays, int idTenant)
        {
            var payment = new Payment()
            {
                IdPayment = _paymentTable.GetMaxId(),
                Paid = DateTime.Now.AddDays((-30 + addDays)),
                Price = (int)_serviceOrderTable.GetServiceById(idService).Price
            };

            var serviceOrder = new ServiceOrder()
            {
                IdPayment = payment.IdPayment,
                IdService = idService,
                IdTenant = idTenant,
                OrderDate = DateTime.Now
            };
            _paymentTable.NewPayment(payment);
            _serviceOrderTable.NewServiceOrder(serviceOrder);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult AddDays(int addDays, int idTenant)
        {
            var payment = _serviceOrderTable.GetNewestServiceOrderToTenantByTenantId(idTenant).IdPaymentNavigation;

            if (payment.Paid.HasValue)
            {
                payment.Paid = payment.Paid.Value.AddDays(addDays);
            }

            _paymentTable.UpdatePayment(payment);

            return RedirectToAction("Index");
        }
    }
}