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
            try
            {
                MemoryStream memory = new MemoryStream();

                Pdf pdf = new Pdf(memory);

                Section section = pdf.Sections.Add();

                section.AddParagraph(new Text(Body));
                section.AddParagraph(new Text("testing...."));
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
                

                pdf.Close();

                blob.UploadFromStream(memory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ex:{ex.Message}, stack:{ex.StackTrace}, source:{ex.Source}");
            }

        }


    }
}
