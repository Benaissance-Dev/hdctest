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
using Renci.SshNet;

namespace hdcNoticeGeneratorConsole
{
    public class NoticeGen
    {
        private const string localFile = "c:\\local\\test.pdf";
        private const bool useLocalStorage = false;

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
                Console.WriteLine($"Generate: Beginning generation for {Body}");
                MemoryStream ms = new MemoryStream();
                ms.Position = 0;

                Pdf pdf = new Pdf(ms);
                //var pdf = new Aspose.Pdf.Generator.Pdf();

                pdf.Author = "Benaissance";
                
                Section section = pdf.Sections.Add();

                section.Paragraphs.Add(new Aspose.Pdf.Generator.Text(Body));
                section.Paragraphs.Add(new Aspose.Pdf.Generator.Text("testing..."));
                //section.AddParagraph(new Text(Body));
                //section.AddParagraph(new Text("testing...."));
                //save it


                //name the blob

                string datePreFix = DateTime.Now.ToString("yyyyMMddhhmmss");

                string fname = $"notice_{identifier}_{datePreFix}_.PDF";

                
                
                Console.WriteLine("Closing PDF...");

                //pdf.Save(ms);

                pdf.Close();


                //TODO: change to keys
                //PasswordAuthenticationMethod pm = new PasswordAuthenticationMethod("preston", "<password here>");

                string noticesPath = string.Empty;

                if (Util.IsWindows())
                    noticesPath = Path.Combine( "c:\\local\\notices" , fname);
                else
                    noticesPath = Path.Combine( "/notices" , fname);

                Console.WriteLine($"file save full path: {noticesPath}");

                //if (File.Exists(localFile)) File.Delete(localFile);
                //pdf.Save(localFile);

                ms.Position = 0;

                if (useLocalStorage)
                {
                    using (FileStream fs = new FileStream(noticesPath, FileMode.CreateNew))
                    {
                        byte[] data = ms.ToArray();

                        fs.Write(data, 0, data.Length);

                        fs.Flush();
                    }
                }
                else
                {
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

                    //string containerPath = "https://benaissancedev.blob.core.windows.net/notices";

                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                    CloudBlobContainer container = blobClient.GetContainerReference("notices");
                    CloudBlockBlob blob = container.GetBlockBlobReference(fname);

                    container.CreateIfNotExists();

                    var t = Task.Factory.FromAsync<Stream>(blob.BeginUploadFromStream, blob.EndUploadFromStream, ms, null);
                    t.Wait();
                }




                Console.WriteLine($"Uploaded pdf {fname}");
                

            }
            catch (AggregateException aex)
            {
                foreach (var ex in aex.InnerExceptions)
                {
                    Console.WriteLine($"Exception!  message:{ex.Message}, stacktrace:{ex.StackTrace}, source:{ex.Source}");
                }
            }

        }


    }
}
