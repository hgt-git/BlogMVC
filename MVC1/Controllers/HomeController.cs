using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using BlogMVC.DAL;
using BlogMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;


namespace BlogMVC.Controllers
{
    public class HomeController : Controller
    {

        private BlogContext db = new BlogContext();
        public ActionResult Index()
        {
            return View(db.BlogPostTable.ToList());
        }
        

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }
            BlogPostModel blogPostModel = db.BlogPostTable.Find(id);
            if (blogPostModel == null)
            {
                return HttpNotFound();
            }
            blogPostModel.Viewed++;
            db.SaveChanges();

            DirectoryInfo uploadedImagesDirectory = new DirectoryInfo(Server.MapPath("~/UploadedImages/"));
            BlogPostViewModel bpViewModel = new BlogPostViewModel(blogPostModel, uploadedImagesDirectory);            
            
            
            return View(bpViewModel);
        }

        
            
    }
}