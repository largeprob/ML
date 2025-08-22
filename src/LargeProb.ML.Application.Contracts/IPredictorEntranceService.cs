using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LargeProb.ML.Application.Contracts
{
    public interface IPredictorEntranceService
    {
        public static string[] ImageTypes = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"];
        public static string[] VideoTypes = { ".mp4", ".avi" };

        /// <summary>
        /// 单个预测
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="outFolder"></param>
        string Predict(IPredictorService predictor, string imagePath, string outFolder);

        /// <summary>
        /// 批量预测
        /// </summary>
        /// <param name="imagesFolder"></param>
        /// <param name="outFolder"></param>
        void Transform(IPredictorService predictor, string imagesFolder, string outFolder);
    }
}
