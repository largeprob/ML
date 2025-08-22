using Google.Protobuf;
using LargeProb.Core.Controller;
using LargeProb.Core.Exceptions;
using LargeProb.ML.Api;
using LargeProb.ML.Application;
using LargeProb.ML.Application.Predictors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenCvSharp;
using SixLabors.ImageSharp;

namespace LargeProbBlog.Api.Main
{
    /// <summary>
    /// 基于ONNX模型的预测
    /// </summary>
    /// <param name="_env"></param>
    /// <param name="_configuration"></param>
    ///<remarks>由YOLO 11n  训练的单一目标检测模型</remarks>
    public class OnnxServiceController(IWebHostEnvironment _env,  IConfiguration _configuration) : ApiService
    {
        /// <summary>
        /// 预测
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        /// <exception cref="SolutionException"></exception>
        /// <exception cref="Exception"></exception>
        [HttpPost("Predictor")]
        [RequestSizeLimit(50 * 1024 * 1024)]
        public async Task<FileContentResult> Predictor(IFormFile formFile)
        {
            if (formFile == null || formFile.Length <= 0)
            {
                throw new SolutionException("文件不能为空");
            }

            var extension = Path.GetExtension(formFile.FileName).ToLower();
            if (PredictorEntranceService.ImageTypes.Contains(extension))
            {
                var maxSize = _configuration["OSS:IMAGE_MAX_SIZE"];
                if (formFile.Length > Convert.ToInt64(maxSize))
                {
                    throw new SolutionException("图片大小超过限制");
                }
            }

            if (PredictorEntranceService.VideoTypes.Contains(extension))
            {
                var maxSize = _configuration["OSS:File_MAX_SIZE"];
                if (formFile.Length > Convert.ToInt64(maxSize))
                {
                    throw new SolutionException("文件大小超过限制");
                }
            }

            if (!PredictorEntranceService.ImageTypes.Contains(extension) && !PredictorEntranceService.VideoTypes.Contains(extension))
            {
                throw new Exception("不支持该类型的文件格式：" + extension);
            }

            string assetsPath = GetAbsolutePath("MLInfo");
            if (!Directory.Exists(assetsPath))
            {
                Directory.CreateDirectory(assetsPath);
            }

            //保存到本地
            var beforePath = Path.Combine(assetsPath, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + extension);
            using (var stream = new FileStream(beforePath, FileMode.Create))
            {
                await formFile.CopyToAsync(stream);
            }

            //预测
            var outFolder = Path.Combine(assetsPath, "predictors");
            var predictor = new PredictorEntranceService(
                new OnnxPredictorService(
                Path.Combine("model", "best.onnx")
                , new[] { "破损土豆", "劣质土豆", "发 霉变质土豆", "好土豆", "发芽土豆" }
                , new[] { Color.Red, Color.Red, Color.Red, Color.Green, Color.Orange }, false));
            var filePath = predictor.Predict(beforePath, outFolder);
          
            try
            {
                using (var fs = System.IO.File.OpenRead(filePath))
                {
                    byte[] buffer = new byte[fs.Length];
                    await fs.ReadExactlyAsync(buffer);
                    return File(buffer, formFile.ContentType);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //删除临时文件
                if (System.IO.File.Exists(beforePath))
                {
                    System.IO.File.Delete(beforePath);
                }
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }

        private string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}
