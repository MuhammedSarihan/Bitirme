using Microsoft.AspNetCore.Routing.Constraints;
// Dosya Yükleyip Silmek için yardımcı sınıf
namespace odevVb.Utils
{
    public class FileHelper
    {
        public static async Task<string> FileLoaderAsync(IFormFile formFile, string filePath="/img/")
        {
            string fileName = "/img/";
            if (formFile != null && formFile.Length > 0) {
                fileName = formFile.FileName.ToLower();
                string directory = Directory.GetCurrentDirectory() + "/wwwroot" + filePath +fileName;
                using var stream = new FileStream(directory,FileMode.Create);
                await formFile.CopyToAsync(stream);
            }

            return fileName;
        }
        public static bool FileRemover(string fileName, string filePath = "/img/")
        {

            string directory = Directory.GetCurrentDirectory() + "/wwwroot" + filePath + fileName;
            if (File.Exists(directory))
            {
                File.Delete(directory);
                return true;
            }

            return false;
        }


    }
}
