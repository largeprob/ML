using LargeProb.Core.Controller;
using LargeProb.ML.Api;
using LargeProb.ML.Application;
using LargeProb.ML.Application.Contracts;
using LargeProb.ML.Application.Predictors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
 
namespace LargeProbBlog.Api.Main
{
    /// <summary>
    /// 基于ONNX模型，ML.NET预测
    /// </summary>
    ///<remarks>由YOLO 11n  训练的单一目标检测模型</remarks>
    public class MLServiceController() : ApiService
    {

        /// <summary>
        /// 预测
        /// </summary>
        [HttpPost("Predictor")]
        public void Predictor()
        {
            string assetsPath = GetAbsolutePath("model");

            var predictor = new PredictorEntranceService(new MLPredictorService(
                Path.Combine(assetsPath, "best.onnx"),
                new[] { "破损土豆", "劣质土豆", "发霉变质土豆", "好土豆", "发芽土豆" },
                new[] { Color.Red, Color.Red, Color.Red, Color.Green, Color.Orange }
            ));

            predictor.Predict(Path.Combine(assetsPath, "pp.mp4"), Path.Combine(assetsPath, "testOut"));

            //predictor.Predict(Path.Combine(assetsPath, "test/pp3.jpeg"), Path.Combine(assetsPath, "testOut"));
            //predictor.Transform(Path.Combine(assetsPath, "test"), Path.Combine(assetsPath, "testOut"));
        }


        private string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath =Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}
