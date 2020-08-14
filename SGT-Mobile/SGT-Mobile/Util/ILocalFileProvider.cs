using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SGTMobile.Util
{
    public interface ILocalFileProvider
    {
        Task<string> SaveFileToDisk(Stream stream, string fileName);
    }
}
