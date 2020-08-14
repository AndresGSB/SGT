
using System.IO;
using System.Threading.Tasks;
using SGT_Mobile.Droid;
using SGTMobile.Util;
using Xamarin.Forms;

[assembly: Dependency(typeof(LocalFileProvider))]
namespace SGT_Mobile.Droid
{
    public class LocalFileProvider : ILocalFileProvider
    {
       
        private readonly string _rootDir = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "sgt");

        public async Task<string> SaveFileToDisk(Stream pdfStream, string fileName)
        {
            if (!Directory.Exists(_rootDir))
                Directory.CreateDirectory(_rootDir);

            var filePath = Path.Combine(_rootDir, fileName);

            using (var memoryStream = new MemoryStream())
            {
                await pdfStream.CopyToAsync(memoryStream);
                File.WriteAllBytes(filePath, memoryStream.ToArray());
            }
            var files = Android.App.Application.Context.Assets.List("");
            return filePath;
        }
    }
}