using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Aspose.Pdf;
using Aspose.Pdf.Generator;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace hdcNoticeGeneratorConsole
{
    public class NoticeGen
    {
        public string Body { get; set; }
        public NoticeGen() { }

        public NoticeGen(string body)
        {
            Body = body;
        }

        public void Generate(string identifier)
        {
            Pdf pdf = new Pdf();

            Section section = pdf.Sections.Add();

            section.Paragraphs.Add(new Text(Body));

            //save it
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

            //string containerPath = "https://benaissancedev.blob.core.windows.net/notices";

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference("notices");

            container.CreateIfNotExists();

            //name the blob

            string datePreFix = DateTime.Now.ToString("yyyyMMddhhmmss");

            string fname = $"notice_{identifier}_{datePreFix}_.PDF";

            CloudBlockBlob blob = container.GetBlockBlobReference(fname);

            MemoryStream memory = new MemoryStream();

            pdf.Save(memory);

            blob.UploadFromStream(memory);


        }


    }
}
