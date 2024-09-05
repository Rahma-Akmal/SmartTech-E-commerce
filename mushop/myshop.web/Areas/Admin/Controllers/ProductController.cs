using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using myshop.Entities.Models;
using myshop.Entities.ViewModel;

namespace myshop.web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult GetData()
        {
            var products = _unitOfWork.Product.GetAll(includeword: "Category");
            return Json(new {data=products});
        }
        [HttpGet]
        public IActionResult Create()
        {
            ProductVM product = new ProductVM()
            {
                product = new Product(),
                CategoryList=_unitOfWork.Category.GetAll().Select(x=>new SelectListItem
                {
                    Text = x.Name,
                    Value=x.Id.ToString()

                })
            };
            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductVM productvm,IFormFile file)
        {
            if (ModelState.IsValid)
            {
                string rootPath=_webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string filename=Guid.NewGuid().ToString();
                    var Upload=Path.Combine(rootPath, "Images/Product");
                    var ext=Path.GetExtension(file.FileName);
                    using (var fileStream=new FileStream(Path.Combine(Upload,filename+ext),FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productvm.product.Img = "Images/Product/" + filename + ext;
                }
                _unitOfWork.Product.Add(productvm.product);
                _unitOfWork.Complete();
                TempData["Create"] = "Data Has Created Successfully";
                return RedirectToAction("Index");
            }
            return View(productvm);
        }
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null | id == 0)
            {
                NotFound();
            }
            ProductVM productvm = new ProductVM()
            {
                product = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id),
                CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()

                })
            };
            return View(productvm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductVM productVM,IFormFile? file)
        {
            string rootPath = _webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string filename = Guid.NewGuid().ToString();
                var Upload = Path.Combine(rootPath, "Images/Product");
                var ext = Path.GetExtension(file.FileName);
                if (productVM.product.Img!=null)
                {
                    var oldimg=Path.Combine(rootPath,productVM.product.Img.TrimStart('\\'));
                    if (System.IO.File.Exists(oldimg)) 
                    {
                        System.IO.File.Delete(oldimg);
                    }
                }
                using (var fileStream = new FileStream(Path.Combine(Upload, filename + ext), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
                productVM.product.Img = "Images/Product/" + filename + ext;
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Update(productVM.product);
                _unitOfWork.Complete();
                TempData["Update"] = "Data Has Updated Successfully";
                return RedirectToAction("Index");
            }
            return View(productVM.product);
        }
        [HttpDelete]
        public IActionResult Del(int? id)
        {
            var proo = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);
            if (proo == null)
            {
                return Json(new {success=false,message="Error While Deleting"});
            }
            _unitOfWork.Product.Remove(proo);
            var oldimg = Path.Combine(_webHostEnvironment.WebRootPath, proo.Img.TrimStart('\\'));
            if (System.IO.File.Exists(oldimg))
            {
                System.IO.File.Delete(oldimg);
            }
            _unitOfWork.Complete();
            return Json(new { success = true, message = "File Has Been Deleted" });
            
        }
    }
}
