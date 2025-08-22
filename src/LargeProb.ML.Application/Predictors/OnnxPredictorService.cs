using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LargeProb.ML.Application.Contracts;
using LargeProb.ML.Application.Share;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Advanced;

namespace LargeProb.ML.Application.Predictors
{
    /// <summary>
    /// ONNX预测器服务
    /// </summary>
    public class OnnxPredictorService : PredictorBase, IPredictorService
    {
        public OnnxPredictorService(string modelPath, string[] classes, Color[] classColors, bool useCuda = false) :
         base(modelPath, classes, classColors, useCuda)
        {
        }

        /// <summary>
        ///  批量预测
        /// </summary>
        /// <param name="imagesFolder"></param>
        public IEnumerable<float[]> Transform(string imagesFolder)
        {
            var imaesg = Directory.GetFiles(imagesFolder);
            foreach (var item in imaesg)
            {
                yield return Predict(new ImageWrapper() { Path = item, Image = Image.Load(item) });
            }
        }

        /// <summary>
        /// 单个预测
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="outFolder"></param>
        public float[] Predict(ImageWrapper imageWrapper)
        {
            //加载图片
            Image originalImage = imageWrapper.Image;

            //缩放为模型输入大小
            Image resizeImg = null;
            if (originalImage.Width != ModelInputWidth || originalImage.Height != ModelInputHeight)
            {
                resizeImg = originalImage.Clone(x => x.Resize(ModelInputWidth, ModelInputHeight));
            }
            else
            {
                resizeImg = originalImage;
            }

            //调正输入张量
            var tensor = new DenseTensor<float>(new[] { 1, 3, ModelInputWidth, ModelInputHeight });
            using (var img = resizeImg.CloneAs<Rgb24>())
            {
                Parallel.For(0, img.Height, y => {
                    var pixelSpan = img.DangerousGetPixelRowMemory(y).Span;
                    for (int x = 0; x < img.Width; x++)
                    {
                        tensor[0, 0, y, x] = pixelSpan[x].R / 255.0F; // r
                        tensor[0, 1, y, x] = pixelSpan[x].G / 255.0F; // g
                        tensor[0, 2, y, x] = pixelSpan[x].B / 255.0F; // b
                    }
                });
            }

            //模型输入
            using var inputOrtValue = OrtValue.CreateTensorValueFromMemory(OrtMemoryInfo.DefaultInstance, tensor.Buffer, new long[] { 1, 3, ModelInputWidth, ModelInputHeight });
            var inputs = new Dictionary<string, OrtValue> { { ModelInputName, inputOrtValue } };

            //预测结果
            using IDisposableReadOnlyCollection<OrtValue> results = _inferenceSession.Run(new RunOptions(), inputs, _inferenceSession.OutputNames);
            return results.First().GetTensorDataAsSpan<float>().ToArray();
        }
    }
}
