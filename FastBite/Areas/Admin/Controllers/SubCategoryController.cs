using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using FastBite.Data;
using FastBite.Models;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FastBite.Models.Viewmodels;
using Microsoft.AspNetCore.Authorization;
using FastBite.Utility;

//using System.Web.Mvc;

namespace FastBite.Areas.Admin.Controllers
{
  
    [Area("Admin")]
    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public SubCategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var subCategories = await _db.SubCategory
                .Include(s => s.category)
                .OrderBy(s => s.Id)
                .ToListAsync();
            return View(subCategories);
        }

        public IActionResult Create()
        {
            var model = new CategoryAndSubCategoryModel
            {
                categoryList = _db.Category.OrderBy(c => c.Id).ToList(),
                subCategory = new SubCategory(),
                subCategoryList = _db.SubCategory.Select(s => s.Name).Distinct().ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryAndSubCategoryModel model, int ddCategoryList)
        {
            if (model == null)
            {
                model = new CategoryAndSubCategoryModel();
            }

            if (model.subCategory == null)
            {
                model.subCategory = new SubCategory();
            }

            model.subCategory.Name = string.IsNullOrWhiteSpace(model.subCategory.Name)
                ? Request.Form["subCategory.Name"].ToString()
                : model.subCategory.Name;
            model.subCategory.CategoryId = ddCategoryList == 0 && int.TryParse(Request.Form["ddCategoryList"], out var createCategoryId)
                ? createCategoryId
                : ddCategoryList;
            ModelState.Remove("subCategory.CategoryId");

            if (!string.IsNullOrWhiteSpace(model.subCategory.Name))
            {
                _db.SubCategory.Add(model.subCategory);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            model.categoryList = _db.Category.OrderBy(c => c.Id).ToList();
            model.subCategoryList = _db.SubCategory.Select(s => s.Name).Distinct().ToList();
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var subCategory = await _db.SubCategory.FindAsync(id);
            if (subCategory == null)
            {
                return NotFound();
            }

            var model = new CategoryAndSubCategoryModel
            {
                subCategory = subCategory,
                categoryList = _db.Category.OrderBy(c => c.Id).ToList(),
                subCategoryList = _db.SubCategory.Select(s => s.Name).Distinct().ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CategoryAndSubCategoryModel model, int ddCategoryList)
        {
            if (model == null)
            {
                model = new CategoryAndSubCategoryModel();
            }

            if (model.subCategory == null)
            {
                model.subCategory = new SubCategory();
            }

            model.subCategory.Name = string.IsNullOrWhiteSpace(model.subCategory.Name)
                ? Request.Form["subCategory.Name"].ToString()
                : model.subCategory.Name;
            model.subCategory.CategoryId = ddCategoryList == 0 && int.TryParse(Request.Form["ddCategoryList"], out var editCategoryId)
                ? editCategoryId
                : ddCategoryList;
            ModelState.Remove("subCategory.CategoryId");

            if (model.subCategory.Id == 0)
            {
                model.categoryList = _db.Category.OrderBy(c => c.Id).ToList();
                model.subCategoryList = _db.SubCategory.Select(s => s.Name).Distinct().ToList();
                return View(model);
            }

            var entity = await _db.SubCategory.FindAsync(model.subCategory.Id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.Name = model.subCategory.Name;
            entity.CategoryId = model.subCategory.CategoryId;
            _db.SubCategory.Update(entity);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var subCategory = await _db.SubCategory.Include(s => s.category).FirstOrDefaultAsync(s => s.Id == id);
            if (subCategory == null)
            {
                return NotFound();
            }

            return View(subCategory);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var subCategory = await _db.SubCategory.FindAsync(id);
            if (subCategory != null)
            {
                _db.SubCategory.Remove(subCategory);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}