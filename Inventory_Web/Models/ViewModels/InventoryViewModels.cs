using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Web.Models.ViewModels
{
    // مدل محصول
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام کالا الزامی است")]
        [Display(Name = "نام کالا")]
        public string Name { get; set; }

        [Display(Name = "نام دوم")]
        public string Name2 { get; set; }

        [Required(ErrorMessage = "کد اصلی الزامی است")]
        [Display(Name = "کد اصلی")]
        public string MainCode { get; set; }

        [Display(Name = "کد دوم")]
        public string Code2 { get; set; }

        [Display(Name = "کد سوم")]
        public string Code3 { get; set; }

        [Display(Name = "تعداد موجودی کل")]
        public int TotalQuantity { get; set; }

        [Required(ErrorMessage = "نقطه سفارش الزامی است")]
        [Display(Name = "نقطه سفارش")]
        [Range(0, int.MaxValue, ErrorMessage = "مقدار باید عدد مثبت باشد")]
        public int ReorderPoint { get; set; }

        [Display(Name = "موجودی اطمینان")]
        [Range(0, int.MaxValue, ErrorMessage = "مقدار باید عدد مثبت باشد")]
        public int SafetyStock { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        public List<BrandViewModel> Brands { get; set; } = new List<BrandViewModel>();
        public List<InventoryViewModel> Inventories { get; set; } = new List<InventoryViewModel>();
    }

    public class CreateProductViewModel
    {
        [Required(ErrorMessage = "نام کالا الزامی است")]
        [Display(Name = "نام کالا")]
        public string Name { get; set; }

        [Display(Name = "نام دوم")]
        public string Name2 { get; set; }

        [Required(ErrorMessage = "کد اصلی الزامی است")]
        [Display(Name = "کد اصلی")]
        public string MainCode { get; set; }

        [Display(Name = "کد دوم")]
        public string Code2 { get; set; }

        [Display(Name = "کد سوم")]
        public string Code3 { get; set; }

        [Required(ErrorMessage = "نقطه سفارش الزامی است")]
        [Display(Name = "نقطه سفارش")]
        [Range(0, int.MaxValue, ErrorMessage = "مقدار باید عدد مثبت باشد")]
        public int ReorderPoint { get; set; }

        [Display(Name = "موجودی اطمینان")]
        [Range(0, int.MaxValue, ErrorMessage = "مقدار باید عدد مثبت باشد")]
        public int SafetyStock { get; set; }
    }

    // مدل برند
    public class BrandViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    // مدل ارتباط محصول و برند
    public class ProductBrandViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
    }

    // مدل موجودی
    public class InventoryViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int Quantity { get; set; }
    }
}