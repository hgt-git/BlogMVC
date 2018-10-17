using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BlogMVC.Models
{
    public class Image
    {
        
        public int Id { get; set; }
        public string Name { get; set; }

        public Image() 
            {
            BlogPostModels = new HashSet<BlogPostModel>();
            }

        public virtual ICollection<BlogPostModel> BlogPostModels { get; set; }
    }
}