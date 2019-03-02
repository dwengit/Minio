using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel;
using Minio.Exceptions;

namespace MinioDemo.Controllers
{
    public class MinioServiceController : Controller
    {
        public static string endpoint = "172.16.37.168:9000";
        public static string accessKey = "HK0AJ5Y345ZVW2QOSIDD";
        public static string secretKey = "P8Satb3E9+za6nU7NqWcstYf+Z59Vdr6qKuZly9W";

        private MinioClient minio;

        public MinioServiceController()
        {

            minio = new MinioClient(endpoint, accessKey, secretKey);

            var getListBucketsTask = minio.ListBucketsAsync();

            // Iterate over the list of buckets.
            foreach (Bucket bucket in getListBucketsTask.Result.Buckets)
            {
                Console.Out.WriteLine(bucket.Name + " " + bucket.CreationDateDateTime);
            }

        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> UploadFile()
        {
            var bucketName = "test2";
            //var location = "us-east-1";

            try
            {
                bool found = await minio.BucketExistsAsync(bucketName);
                if (found)
                {
                    //await minio.MakeBucketAsync(bucketName);
                    foreach (var file in HttpContext.Request.Form.Files)
                    {
                        Stream stream = file.OpenReadStream();

                        await minio.PutObjectAsync(bucketName,
                                file.FileName,
                                stream,
                                file.Length,
                                file.ContentType);

                    }

                }
            }
            catch (MinioException e)
            {
                Console.WriteLine("File Upload Error: {0}", e.Message);
            }

            return Json(new { Result = "ok" });
        }

    }
}