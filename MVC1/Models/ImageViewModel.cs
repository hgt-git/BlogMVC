using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace BlogMVC.Models
{
    public class ImageViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public FileInfo ImageFile { get; set; }

        public static ICollection<Image> mapImageViewModelsToImages(ICollection<ImageViewModel> imageViewModelsForConvertion)
        {
            ICollection<Image> mappedImages = new List<Image>();
            foreach (ImageViewModel notConvertedImage in imageViewModelsForConvertion)
            {
                mappedImages.Add(new Image { Id = notConvertedImage.Id, Name = notConvertedImage.Name });
            }
            return mappedImages;
        }

       
        public static ICollection<ImageViewModel> mapImagesToImageViewModels(ICollection<Image> imagesForMapping, DirectoryInfo imagesPath)
        {
            ICollection<ImageViewModel> mappedImageViewModels = new List<ImageViewModel>();           
            
            foreach (var image in imagesForMapping)
            if (image.Name != null)
                if (File.Exists(Path.Combine(imagesPath.ToString(), image.Name)))
                {                    
                    mappedImageViewModels.Add(new ImageViewModel { Id = image.Id, Name = image.Name, ImageFile = imagesPath.GetFiles(image.Name).First() });
                }

            return mappedImageViewModels;
        }

    }
}