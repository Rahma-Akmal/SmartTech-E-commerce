using Microsoft.AspNetCore.Mvc;
using myshop.DataAccess.Implementation;
using myshop.Entities.Models;
using myshop.Entities.ViewModel;
using myshop.Utilities;
using Stripe;

namespace myshop.web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetData()
        {
            IEnumerable<OrderHeaders> orderHeaders;
            orderHeaders = _unitOfWork.OrderHeader.GetAll(includeword: "ApplicationUser");
            return Json(new { data = orderHeaders });
        }
        public IActionResult Details(int orderid)
        {
            OrderVM orderVM=new OrderVM()
            {
                orderHeaders=_unitOfWork.OrderHeader.GetFirstOrDefault(y=>y.Id==orderid,includeword:"ApplicationUser"),
                orderDetails=_unitOfWork.OrderDetails.GetAll(x=>x.OrderHeadersId==orderid, includeword:"Product"),
            };
            return View(orderVM);
        }

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult UpdateOrderDetails()
		{
			var orderfromdb = _unitOfWork.OrderHeader.GetFirstOrDefault(u=>u.Id==OrderVM.orderHeaders.Id);
			orderfromdb.Name = OrderVM.orderHeaders.Name;
			orderfromdb.Phone = OrderVM.orderHeaders.Phone;
			orderfromdb.Address = OrderVM.orderHeaders.Address;
			orderfromdb.City = OrderVM.orderHeaders.City;

			if (OrderVM.orderHeaders.Carrier != null)
			{
				orderfromdb.Carrier = OrderVM.orderHeaders.Carrier;
			}

			if (OrderVM.orderHeaders.TrackingNumber != null)
			{
				orderfromdb.TrackingNumber = OrderVM.orderHeaders.TrackingNumber;
			}

			_unitOfWork.OrderHeader.Update(orderfromdb);
			_unitOfWork.Complete();
			TempData["Update"] = "Item has Updated Successfully";
			return RedirectToAction("Details", "Order", new { orderid = orderfromdb.Id });
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult StartProccess()
		{
			_unitOfWork.OrderHeader.UpdateStatus(OrderVM.orderHeaders.Id, SD.Processing, null);
			_unitOfWork.Complete();

			TempData["Update"] = "Order Status has Updated Successfully";
			return RedirectToAction("Details", "Order", new { orderid = OrderVM.orderHeaders.Id });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult StartShip()
		{
			var orderfromdb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.orderHeaders.Id);
			orderfromdb.TrackingNumber = OrderVM.orderHeaders.TrackingNumber;
			orderfromdb.Carrier = OrderVM.orderHeaders.Carrier;
			orderfromdb.OrderStatus = SD.Shipped;
			orderfromdb.ShippingDate = DateTime.Now;

			_unitOfWork.OrderHeader.Update(orderfromdb);
			_unitOfWork.Complete();

			TempData["Update"] = "Order has Shipped Successfully";
			return RedirectToAction("Details", "Order", new { orderid = OrderVM.orderHeaders.Id });
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult CancelOrder()
		{
			var orderfromdb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.orderHeaders.Id);
			if (orderfromdb.PaymentStatus == SD.Approve)
			{
				var option = new RefundCreateOptions
				{
					Reason = RefundReasons.RequestedByCustomer,
					PaymentIntent = orderfromdb.PaymentIntentId
				};

				var service = new RefundService();
				Refund refund = service.Create(option);

				_unitOfWork.OrderHeader.UpdateStatus(orderfromdb.Id, SD.Cancelled, SD.Refund);
			}
			else
			{
				_unitOfWork.OrderHeader.UpdateStatus(orderfromdb.Id, SD.Cancelled, SD.Cancelled);
			}
			_unitOfWork.Complete();

			TempData["Update"] = "Order has Cancelled Successfully";
			return RedirectToAction("Details", "Order", new { orderid = OrderVM.orderHeaders.Id });
		}

	}
}
