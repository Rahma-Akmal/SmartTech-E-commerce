using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using myshop.DataAccess.Implementation;
using myshop.Entities.Models;
using myshop.Entities.ViewModel;
using myshop.Utilities;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace myshop.web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM shoppinCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppinCartVM = new ShoppingCartVM()
            {
                CartList=_unitOfWork.ShoppingCart.GetAll(u=>u.ApplicationUserId==claim.Value,includeword:"Product"),
                OrderHeaders=new()
            };
            foreach (var item in shoppinCartVM.CartList)
            {
                shoppinCartVM.TotalCarts += (item.Count * item.Product.Price);
            }
            return View(shoppinCartVM);
        }
        public IActionResult Plus(int Cartid)
        {
            var shoppingcart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == Cartid);
            _unitOfWork.ShoppingCart.IncreaseCount(shoppingcart, 1);
            _unitOfWork.Complete();
            return RedirectToAction("Index");
        }
		public IActionResult Minus(int Cartid)
		{
            var shoppingcart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == Cartid);

            if (shoppingcart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(shoppingcart);
                var count = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == shoppingcart.ApplicationUserId).ToList().Count() - 1;
                HttpContext.Session.SetInt32(SD.SessionKey, count);
            }
            else
            {
                _unitOfWork.ShoppingCart.DecreaseCount(shoppingcart, 1);

            }
            _unitOfWork.Complete();
            return RedirectToAction("Index");
        }
        public IActionResult Remove(int Cartid)
        {
            var shoppingcart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == Cartid);
            _unitOfWork.ShoppingCart.Remove(shoppingcart);
            _unitOfWork.Complete();
            var count = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == shoppingcart.ApplicationUserId).ToList().Count();
            HttpContext.Session.SetInt32(SD.SessionKey, count);
            return RedirectToAction("Index");
        }
        [HttpGet]
		public IActionResult Summary()
		{
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppinCartVM = new ShoppingCartVM()
            {
                CartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeword: "Product"),
               OrderHeaders =new()
            };
            shoppinCartVM.OrderHeaders.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == claim.Value);
            shoppinCartVM.OrderHeaders.Name=shoppinCartVM.OrderHeaders.ApplicationUser.Name;
            shoppinCartVM.OrderHeaders.Address = shoppinCartVM.OrderHeaders.ApplicationUser.Address;
            shoppinCartVM.OrderHeaders.City = shoppinCartVM.OrderHeaders.ApplicationUser.City;
            shoppinCartVM.OrderHeaders.Phone = shoppinCartVM.OrderHeaders.ApplicationUser.PhoneNumber;
            foreach (var item in shoppinCartVM.CartList)
            {
                shoppinCartVM.OrderHeaders.TotalPrice += (item.Count * item.Product.Price);
            }
            return View(shoppinCartVM);

        }
        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        
        public IActionResult SummaryPost(ShoppingCartVM shoppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            shoppingCartVM.CartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeword: "Product");
            shoppingCartVM.OrderHeaders.OrderStatus = SD.Pending;
            shoppingCartVM.OrderHeaders.PaymentStatus=SD.Pending;
            shoppingCartVM.OrderHeaders.OrderDate=DateTime.Now;
            shoppingCartVM.OrderHeaders.ApplicationUserId = claim.Value;
            foreach (var item in shoppingCartVM.CartList)
            {
                shoppingCartVM.OrderHeaders.TotalPrice += (item.Count * item.Product.Price);
            }
            _unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeaders);
            _unitOfWork.Complete();
            foreach(var item  in shoppingCartVM.CartList)
            {
                OrderDetails orderDetails = new OrderDetails()
                {
                    ProductId = item.ProductId,
                    OrderHeadersId = shoppingCartVM.OrderHeaders.Id,
                    Price=item.Product.Price,
                    Count=item.Count
                };
                
            }
            var domain = "https://localhost:7007/";
            var options = new SessionCreateOptions
            {
                
                LineItems = new List<SessionLineItemOptions>(),
            
                Mode = "payment",
                SuccessUrl = domain+$"Customer/Cart/OrderConfirmation?id={shoppingCartVM.OrderHeaders.Id}",
                CancelUrl = domain+ $"Customer/Cart/Index",
               
            };
            foreach (var item in shoppingCartVM.CartList)
            {

                var SessionLineOptions= new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price*100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                        },
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(SessionLineOptions);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            shoppingCartVM.OrderHeaders.SessionId = session.Id;
            
            _unitOfWork.Complete();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
            //_unitOfWork.ShoppingCart.RemoveRange(shoppinCartVM.CartList);
            //_unitOfWork.Complete();
            //return RedirectToAction("Index", "Home");
        }
        public IActionResult OrderConfirmation(int id)
        {
            OrderHeaders orderHeaders = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            var service = new SessionService();
            Session session = service.Get(orderHeaders.SessionId);
            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStatus(id, SD.Approve, SD.Approve);
				orderHeaders.PaymentIntentId = session.PaymentIntentId;
				_unitOfWork.Complete();
            }
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeaders.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Complete();
            return View(id);

        }
		



	}
}
