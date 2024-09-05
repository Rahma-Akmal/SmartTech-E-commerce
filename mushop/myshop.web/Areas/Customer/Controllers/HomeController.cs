using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myshop.DataAccess.Implementation;
using myshop.Entities.Models;
using myshop.Entities.ViewModel;
using myshop.Utilities;
using System.Security.Claims;
using X.PagedList;

namespace myshop.web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index(int ? page)
        {
            var PageNumber = page ?? 1;
            int PageSize = 6;
            var product=_unitOfWork.Product.GetAll().ToPagedList(PageNumber, PageSize);
            return View(product);
        }
        [HttpGet]
        public IActionResult Details(int productId)
        {
            var product = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == productId, includeword: "Category");
            if (product == null)
            {
                return NotFound();
            }
            ShoppingCart cart = new ShoppingCart()
            {
                ProductId = productId,
                Product = product,
                Count = 1
            };
            return View(cart);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;
            ShoppingCart cartobj=_unitOfWork.ShoppingCart.GetFirstOrDefault(
              u=>u.ApplicationUserId==claim.Value && u.ProductId==shoppingCart.ProductId);
            if (cartobj == null)
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Complete();
                HttpContext.Session.SetInt32(SD.SessionKey,
                    _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value).ToList().Count()
                );

            }
            else
            {
                _unitOfWork.ShoppingCart.IncreaseCount(cartobj, shoppingCart.Count);
                _unitOfWork.Complete();
            }
            return RedirectToAction("Index");
        }
    }
}
