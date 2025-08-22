using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LargeProb.ML.Application.Contracts;
using LargeProb.ML.Application.Share;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;
using Microsoft.ML.Transforms.Onnx;
using Microsoft.ML;
using SixLabors.ImageSharp;

namespace LargeProb.ML.Application.Predictors
{
    /// <summary>
    /// ML.NET预测器服务
    /// </summary>
    public class MLPredictorService : PredictorBase, IPredictorService
    {
        public class StopSignInput
        {
            [LoadColumn(0)]
            public string ImagePath;

            public string Label;

            public string Extension;

            public static IEnumerable<StopSignInput> ReadFromFile(string imageFolder)
            {
                return Directory
                    .GetFiles(imageFolder)
                    .Where(filePath => System.IO.Path.GetExtension(filePath) != ".md")
                    .Select(filePath =>
                    {
                        return new StopSignInput
                        {
                            ImagePath = filePath,
                            Label = System.IO.Path.GetFileName(filePath),
                            Extension = System.IO.Path.GetExtension(filePath),
                        };
                    });
            }
        }

        public class PredictPrediction
        {
            [ColumnName("output0")]
            public float[] Output { get; set; }

        }

        /// <summary>
        /// MLContext
        /// </summary>
        private MLContext _mLContext { get; set; }

        /// <summary>
        /// Transformer
        /// </summary>

        private TransformerChain<OnnxTransformer> _model;

        public MLPredictorService(string modelPath, string[] classes, Color[] classColors) : base(modelPath, classes, classColors, false)
        {   
            _inferenceSession?.Dispose();

            //加载模型管道
            InitPipeline();
        } 

        /// <summary>
        /// 定义模型管道
        /// </summary>
        private void InitPipeline()
        {


            _mLContext = new MLContext();

            // 定义评分管道
            var pipeline =
                //1加载图片
                _mLContext.Transforms.LoadImages(outputColumnName: "image", imageFolder: "", inputColumnName: nameof(StopSignInput.ImagePath))
                //2将图片大小缩放为模型输入大小
                .Append(
                _mLContext.Transforms.ResizeImages(resizing: ImageResizingEstimator.ResizingKind.Fill, outputColumnName: "image", imageWidth: ModelInputHeight, imageHeight: ModelInputHeight, inputColumnName: "image"))
                //3调整图片输入张量
                .Append(
                _mLContext.Transforms.ExtractPixels("images", "image", interleavePixelColors: false, scaleImage: 1f / 255f))
                //4应用模型
                .Append(
                _mLContext.Transforms.ApplyOnnxModel(modelFile: _modelPath, outputColumnNames: new[] { ModelOutputName }, inputColumnNames: new[] { ModelInputName }));

            //将空数据类型填充到管道
            _model = pipeline.Fit(_mLContext.Data.LoadFromEnumerable(new List<StopSignInput>()));
        }

        /// <summary>
        ///  批量预测
        /// </summary>
        /// <param name="imagesFolder"></param>
        /// <param name="outFolder"></param>
        public IEnumerable<float[]> Transform(string imagesFolder)
        {
            //IEnumerable<StopSignInput> images = StopSignInput.ReadFromFile(imagesFolder);
            //IDataView imageDataView = _mLContext.Data.LoadFromEnumerable(images);
            //IDataView scoredData = _model.Transform(imageDataView);
            //IEnumerable<float[]> probabilities = scoredData.GetColumn<float[]>("output0");

            //for (int i = 0; i < images.Count(); i++)
            //{
            //    var entry = images.ElementAt(i);
            //    using var image = Image.Load(entry.ImagePath);

            //    //预测结果
            //    var boxes = SuppressModelOutput(ParseModelOutput(probabilities.ElementAt(i), image.Width, image.Height));

            //    //绘制边框
            //    DrawBoxes(image, boxes);
            //    image.Save(System.IO.Path.Combine(outFolder, $"{entry.Label}{entry.Extension}"));
            //}


            IEnumerable<StopSignInput> images = StopSignInput.ReadFromFile(imagesFolder);
            IDataView imageDataView = _mLContext.Data.LoadFromEnumerable(images);
            IDataView scoredData = _model.Transform(imageDataView);
            IEnumerable<float[]> probabilities = scoredData.GetColumn<float[]>(ModelOutputName);
            return probabilities;
        }

        /// <summary>
        /// 单个预测
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="outFolder"></param>
        public float[] Predict(ImageWrapper imageWrapper)
        {
            var predictionEngine = _mLContext.Model.CreatePredictionEngine<StopSignInput, PredictPrediction>(_model);
            return predictionEngine.Predict(new StopSignInput() { ImagePath = imageWrapper.Path }).Output;
        }
    }
}
