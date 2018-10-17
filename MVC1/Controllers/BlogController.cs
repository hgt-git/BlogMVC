using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BlogMVC.DAL;
using BlogMVC.Models;
using System.IO;
using System.Collections.Generic;
using System.Web;
using System.Data.Entity.Infrastructure;

namespace BlogMVC.Controllers
{
  [Authorize(Roles ="Admin")]
    public class BlogController : Controller
    {
        private BlogContext db = new BlogContext();

        // GET: Blog
        public ActionResult Index()
        {
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
            }
            
            return View(db.BlogPostTable.ToList());
        }

        // GET: Blog/Details/id
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPostModel blogPostModel = db.BlogPostTable.Find(id);
            if (blogPostModel == null)
            {
                return HttpNotFound();
            }

            DirectoryInfo uploadedImagesDirectory = new DirectoryInfo(Server.MapPath("~/UploadedImages/"));
            BlogPostViewModel bpViewModel = new BlogPostViewModel(blogPostModel, uploadedImagesDirectory);

            return View(bpViewModel);
        }

        
        // GET: Blog/Edit/id
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPostModel blogPostModel = db.BlogPostTable.Include(x => x.Images).FirstOrDefault(p => p.Id == id); 
            if (blogPostModel == null)
            {
                return HttpNotFound();
            }
            Session["editingBlogPostModel"] = blogPostModel;
            if (TempData["ManagingImages"] != null)
            {
                ViewBag.ManagingImages = TempData["ManagingImages"].ToString();
            }

            return View(blogPostModel);
        }

        // POST: Blog/Edit/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Text,PostedOn,Viewed")] BlogPostModel blogPostModel)
        {
            if (ModelState.IsValid)
            {
                blogPostModel.Modified = System.DateTime.Now;
                db.Entry(blogPostModel).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(blogPostModel);
        }


        [HttpGet]
        public ActionResult ManageImagesInPost()
        {
            
            if (Session["editingBlogPostModel"] != null)
            {
                BlogPostModel blogPostModel = (BlogPostModel)Session["editingBlogPostModel"];
                DirectoryInfo uploadedImagesDirectory = new DirectoryInfo(Server.MapPath("~/UploadedImages/"));
                ICollection<Image> allImages = db.ImagesTable.ToList();
                ICollection<ImagesInPostViewModel> allImageInPostViewModels;
                allImageInPostViewModels = ImagesInPostViewModel.mapImagesToImageViewModels(allImages, uploadedImagesDirectory);
                //checks which of all images are in this post and modifies their 'isImageInPost'
                foreach (var imageItem in allImageInPostViewModels)
                {
                    foreach (var imageInPost in blogPostModel.Images)
                        if (imageItem.Id == imageInPost.Id)
                        {
                            imageItem.IsImageInPost = true;
                            break;
                        }

                }                
                if (TempData["Uploaded"] != null)
                {
                    ViewBag.UploadStatus = TempData["Uploaded"].ToString();
                }

                return View(allImageInPostViewModels);
            }
            else
            {
                TempData["ErrorMessage"] = "Your session has timed out. Try again";
                
                return RedirectToAction("Index");
            }           
        }

        [HttpPost]
        public ActionResult ManageImagesInPost(ICollection<int> imageCheckBox)
        {
            if (Session["editingBlogPostModel"] != null)
            {
                ICollection<Image> imagesWithIds = new List<Image>();
                ICollection<Image> allImages = db.ImagesTable.ToList();
                BlogPostModel blogPostModel = (BlogPostModel)Session["editingBlogPostModel"];
                blogPostModel.Modified = System.DateTime.Now;
                if(imageCheckBox!=null)
                foreach (var item in imageCheckBox)
                {
                    foreach (var imageFromDB in allImages)
                        if (imageFromDB.Id == item)
                        {
                            imagesWithIds.Add(imageFromDB);
                            break;
                        }
                }                             
                blogPostModel = db.BlogPostTable.Include(x=>x.Images).FirstOrDefault(p=>p.Id==blogPostModel.Id);
                blogPostModel.Images = imagesWithIds;               
                db.SaveChanges();
                TempData["ManagingImages"] = "Image operations completed successfully.";               
                Session["editingBlogPostModel"] = null;
                return RedirectToAction("Edit",new { id = blogPostModel.Id });
                
            }
            else
            {
                TempData["ErrorMessage"] = "Your session has timed out. Try again";                               
                return RedirectToAction("Index");
            }
            
        }


        // GET: Blog/Delete/id
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPostModel blogPostModel = db.BlogPostTable.Find(id);
            if (blogPostModel == null)
            {
                return HttpNotFound();
            }
            return View(blogPostModel);
        }

        // POST: Blog/Delete/id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BlogPostModel blogPostModel = db.BlogPostTable.Find(id);
            db.BlogPostTable.Remove(blogPostModel);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        

        [HttpGet]
        public ActionResult BeginCreation()
        {
            return View();
        }

                
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BeginCreation([Bind(Include = "Id,Title,Text")] BlogPostModel blogPostModel)
        {
            if (ModelState.IsValid)
            {               
                Session["currentBlogModel"] = blogPostModel;
                return RedirectToAction("SelectImages");
            }

            return View(blogPostModel);
        }        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadImages(HttpPostedFileBase[] files)
        {
            int count = 0;
            count = uploadFiles(files);            
            //assigning file uploaded status to TempData for assigning the value in ViewBag and showing message to user.
            if (count==0)
                TempData["Uploaded"] = "No files uploaded.";
            else
                TempData["Uploaded"] = count + " files uploaded successfully.";
            
            return RedirectToAction("SelectImages");
                       
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadInManagingImages(HttpPostedFileBase[] files)
        {
            int count = 0;
            count = uploadFiles(files);            
            //assigning file uploaded status to TempData for assigning the value in ViewBag and showing message to user.
            if (count == 0)
                TempData["Uploaded"] = "No files uploaded.";
            else
                TempData["Uploaded"] = count + " files uploaded successfully.";

            return RedirectToAction("ManageImagesInPost");

        }

        int uploadFiles(HttpPostedFileBase[] files)
        {
            int count = 0;
            if (files != null)
                foreach (HttpPostedFileBase file in files)
            {
                if (file != null)
                {
                    var InputFileName = Path.GetFileName(file.FileName);
                    Image myImg = new Image();
                    myImg.Name = InputFileName;
                    db.ImagesTable.Add(myImg);
                    db.SaveChanges();
                    var ServerSavePath = Path.Combine(Server.MapPath("~/UploadedImages/") + InputFileName);
                    //Save file to server folder  
                    file.SaveAs(ServerSavePath);
                    count++;
                }
            }
           return count;
        }

        [HttpGet]
        public ActionResult SelectImages()
        {
            if (Session["currentBlogModel"] != null)
            {
                DirectoryInfo uploadedImagesDirectory = new DirectoryInfo(Server.MapPath("~/UploadedImages/"));
                ICollection<Image> allImages = db.ImagesTable.ToList();
                ICollection<ImageViewModel> allImageViewModels;
                allImageViewModels = ImageViewModel.mapImagesToImageViewModels(allImages, uploadedImagesDirectory);
                if (TempData["Uploaded"] != null)
                {
                    ViewBag.UploadStatus = TempData["Uploaded"].ToString();
                }

                return View(allImageViewModels);
            }
            else
            {
                TempData["ErrorMessage"] = "Your session has timed out. Try again";
                return RedirectToAction("Index");
            }
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FinalizingCreation(ICollection<int> imageCheckBox)
        {
            if (Session["currentBlogModel"] != null)
            {
                ICollection<Image> imagesWithIds = new List<Image>();
                ICollection<Image> allImages = db.ImagesTable.ToList();
                BlogPostModel blogPostModel = (BlogPostModel)Session["currentBlogModel"];
                blogPostModel.PostedOn = System.DateTime.Now;
                if(imageCheckBox!=null)
                foreach (var item in imageCheckBox)
                {
                    foreach (var imageFromDB in allImages)
                        if (imageFromDB.Id == item)
                        {
                            imagesWithIds.Add(imageFromDB);
                            break;
                        }
                }
                
                blogPostModel.Images = imagesWithIds;                
                db.BlogPostTable.Add(blogPostModel);
                db.SaveChanges();
                Session["currentBlogModel"] = null;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = "Your session has timed out. Try again";
                return RedirectToAction("Index");
            }
        }

                
        [HttpGet]        
        public ActionResult DeleteImages()
        {
            DirectoryInfo uploadedImagesDirectory = new DirectoryInfo(Server.MapPath("~/UploadedImages/"));    
            ICollection<Image> allImages = db.ImagesTable.ToList();
            ICollection<ImageViewModel> allImageViewModels;
            allImageViewModels = ImageViewModel.mapImagesToImageViewModels(allImages, uploadedImagesDirectory);  
            if (TempData["Deleted"] != null)
            {
                ViewBag.DeletedImages = TempData["Deleted"].ToString();
            }            

            return View(allImageViewModels);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteImages(ICollection<int> imageCheckBox)
        {
            if (imageCheckBox != null)
            {
                int count = 0;
                DirectoryInfo uploadedImagesDirectory = new DirectoryInfo(Server.MapPath("~/UploadedImages/"));
                ICollection<Image> allImages = db.ImagesTable.ToList();
                foreach (var item in imageCheckBox)
                {
                    foreach (var imageFromDB in allImages)
                        if (imageFromDB.Id == item)
                        {
                            if (System.IO.File.Exists(Path.Combine(uploadedImagesDirectory.ToString(), imageFromDB.Name)))
                            {
                                count++;
                                System.IO.File.Delete(Path.Combine(uploadedImagesDirectory.ToString(), imageFromDB.Name));
                                db.ImagesTable.Remove(imageFromDB);
                                break;
                            }
                        }
                }
                db.SaveChanges();
                TempData["Deleted"] = count + " files deleted successfully.";
                return RedirectToAction("DeleteImages");
            }
            else
            {
                TempData["Deleted"] = "No files were selected.";
                return RedirectToAction("DeleteImages");
            }
        }
        
    }
}
