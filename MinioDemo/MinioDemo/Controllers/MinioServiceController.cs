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
        //服务器IP
        public static string endpoint = "192.168.199.181:9000";
        //访问密匙
        public static string accessKey = "Minio";
        //密匙
        public static string secretKey = "Test123456";
        //桶的名称
        public static string bucketName = "test1";

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
            //服务器区域
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

        [HttpGet]
        public async Task<IActionResult> GetUploadFile(string FileName)
        {
            FileStreamResult fileStreamResult = null;
            try
            {

                await Task.Run(async () =>
                    {
                        bool found = await minio.BucketExistsAsync(bucketName);

                        if (found)
                        {
                            //如果找不到对象，将引发异常，
                            await minio.StatObjectAsync(bucketName, FileName);



                            //从桶中获取对象的流
                            await minio.GetObjectAsync(bucketName, FileName,
                                                             (stream) =>
                                                             {
                                                                 fileStreamResult = File(stream, "application/octet-stream");
                                                             });

                        }
                        
                    }
                );

                return fileStreamResult;
            }
            catch (MinioException e)
            {
                Console.Out.WriteLine("Error occurred: " + e);
            }
            return fileStreamResult;
        }

    }
}