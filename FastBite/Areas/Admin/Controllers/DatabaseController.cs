using System;
using System.IO;
using System.Linq;
using FastBite.Data;
using FastBite.Models;
using FastBite.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace FastBite.Areas.Admin.Controllers
{
    [AllowAnonymous]
    [Area("Admin")]
    public class DatabaseController : Controller
    {
      //  SqlConnection connection = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=FastBite;Trusted_Connection=True;MultipleActiveResultSets=true");
        
         public readonly ApplicationDbContext _db;
       //  public IDbContextTransaction _transaction;
        
        

         public DatabaseController( ApplicationDbContext db){
             _db=db;
            // _transaction=transaction;
         }
         public void deleteContext(){
           _db.Database.EnsureDeleted();
           _db.Database.EnsureCreated();

           // Pre-seed data for Selenium test reliability
           var cat1 = new Category { Name = "Apetizer" };
           var cat2 = new Category { Name = "Biryani" };
           _db.Category.AddRange(cat1, cat2);
           _db.SaveChanges();

           var sub1 = new SubCategory { Name = "Beverages", CategoryId = cat1.Id };
           var sub2 = new SubCategory { Name = "Mocktails", CategoryId = cat1.Id };
           var sub3 = new SubCategory { Name = "Veg", CategoryId = cat2.Id };
           _db.SubCategory.AddRange(sub1, sub2, sub3);
           _db.SaveChanges();

           var offer = new Offer { Name = "100OFF", CouponType = "1", Discount = 100, MinimumAmount = 250, isActive = true };
           _db.Offer.Add(offer);
           _db.SaveChanges();

           var rst1 = new Restaurant { RestaurantName = "Restaurant1", Address = "Wallstreet", imageurl = StaticDefinitions.defaultimage };
           var rst2 = new Restaurant { RestaurantName = "Restaurant2", Address = "Wallstreet", imageurl = StaticDefinitions.defaultimage };
           _db.Restaurant.AddRange(rst1, rst2);
           _db.SaveChanges();

           var mi1 = new MenuItem { Name = "Dumbiryani", description = "RiceItem", price = 190, CategoryId = cat2.Id, SubCategoryId = sub3.Id, RestaurantId = rst2.Id, imageUrl = StaticDefinitions.defaultimage };
           var mi2 = new MenuItem { Name = "Lemonade", description = "MadebyLemon", price = 110, CategoryId = cat1.Id, SubCategoryId = sub1.Id, RestaurantId = rst2.Id, imageUrl = StaticDefinitions.defaultimage };
           var mi3 = new MenuItem { Name = "vegbiryani", description = "RiceItem", price = 130, CategoryId = cat2.Id, SubCategoryId = sub3.Id, RestaurantId = rst1.Id, imageUrl = StaticDefinitions.defaultimage };
           _db.MenuItem.AddRange(mi1, mi2, mi3);
           _db.SaveChanges();

           Console.WriteLine("hello");
           DirectoryInfo di = new DirectoryInfo("./wwwroot/images/restaurant");
if(di.GetFiles().Count()>0){

  foreach (FileInfo file in di.GetFiles())
{

    file.Delete(); 
}
  DirectoryInfo di2 = new DirectoryInfo("./wwwroot/images/menuitems");
if(di2.GetFiles().Count()>0){
  foreach (FileInfo file in di2.GetFiles())
{
 
    file.Delete(); 
}
}

             
         }
       
    }
}
}