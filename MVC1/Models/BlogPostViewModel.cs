using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Web;

namespace BlogMVC.Models
{
    public class BlogPostViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The post must have Title.")]
        [StringLength(60, ErrorMessage = "Title cannot be longer than {1} characters.")]
        public string Title { get; set; }

        
        [Required(ErrorMessage = "The post must have text.")]
        public string Text { get; set; } 

        private string description;

        public string Description {
            get { return description ; }
            set {if (Text.Length <= 200)
                    description = Text;
                else
                    description = Text.Substring(0, 200) + "...";
            }
        }
        
        public DateTime PostedOn { get; set; }

        public int Viewed { get; set; }

        public DateTime? Modified { get; set; }

        public BlogPostViewModel()
        {
            Images = new HashSet<ImageViewModel>();
        }

        public BlogPostViewModel(BlogPostModel bpModelForMapping, DirectoryInfo uploadedImagesDirectory)
        {            
            Images = ImageViewModel.mapImagesToImageViewModels(bpModelForMapping.Images, uploadedImagesDirectory);
            Id = bpModelForMapping.Id;
            Text = bpModelForMapping.Text;
            Title = bpModelForMapping.Title;
            Description = bpModelForMapping.Description;
            PostedOn = bpModelForMapping.PostedOn;
            Viewed = bpModelForMapping.Viewed;
            Modified = bpModelForMapping.Modified;
        }

        public virtual ICollection<ImageViewModel> Images { get; set; }

        
    }
}