using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LargeProb.ML.Application.Contracts;
using LargeProb.ML.Application.Predictors;
using LargeProb.ML.Application.Share;
using OpenCvSharp;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace LargeProb.ML.Application
{
    /// <summary>
    /// 预测入口服务
    /// </summary>
    public class PredictorEntranceService 
    {
        public static string[] ImageTypes = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"];
        public static string[] VideoTypes = { ".mp4", ".avi" };
        public class BoundingBox
        {
            public RectangleF Rectangle { get; init; }

            public string Label { get; set; }

            public float Confidence { get; set; }

            public Color BoxColor { get; set; }
        }

        /// <summary>
        /// 字体
        /// </summary>
        private readonly Font _font;


        /// <summary>
        /// 预测服务
        /// </summary>
        private IPredictorService _predictor;

        public PredictorEntranceService(IPredictorService  predictor)
        {
            _predictor = predictor;

            var fontCollection = new FontCollection();
            var fontFamily = fontCollection.Add("./SimHei.ttf");
            _font = fontFamily.CreateFont(24, FontStyle.Bold);
        }


        /// <summary>
        /// 单个预测
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="outFolder"></param>
        public string Predict(string imagePath, string outFolder)
        {

            var extension = Path.GetExtension(imagePath).ToLower();
            if (!ImageTypes.Contains(extension) && !VideoTypes.Contains(extension))
            {
                throw new Exception("不支持该类型的文件格式：" + extension);
            }

            if (!Directory.Exists(outFolder))
            {
                Directory.CreateDirectory(outFolder);
            }

            var savaPath = Path.Combine(outFolder, $"{Path.GetFileName(imagePath)}");

            //如果是视频
            if (VideoTypes.Contains(extension))
            {
                //加载视频
                using var capture = new VideoCapture(imagePath);
                if (capture.Get(VideoCaptureProperties.FrameCount) <= 0)
                {
                    throw new Exception("视频加载失败，内容为空");
                }

                var baseFolder = Path.Combine(outFolder, Path.GetFileName(imagePath).Split('.')[0]);
                if (!Directory.Exists(baseFolder))
                {
                    Directory.CreateDirectory(baseFolder);
                }

                var fileName = Path.GetFileName(imagePath).Split('.')[0];
                var imagesFolder = Path.Combine(baseFolder, Guid.NewGuid().ToString());
                Directory.CreateDirectory(imagesFolder);

                //按帧输出图片
                Mat imageVideo = new Mat();
                int frameIndex = 0;
                while (true)
                {
                    capture.Read(imageVideo);
                    if (imageVideo.Empty())
                    {
                        break;
                    }

                    var tempPath = Path.Combine(imagesFolder, $"{frameIndex}.jpg");
                    var result = Cv2.ImWrite(tempPath, imageVideo);
                    frameIndex++;
                }

                //预测图片
                var predictorsFolder = Path.Combine(baseFolder, "predictors");
                if (Directory.Exists(predictorsFolder))
                {
                    Directory.Delete(predictorsFolder, true);
                }
                Directory.CreateDirectory(predictorsFolder);

                var imaesg = Directory.GetFiles(imagesFolder);
                int width = 0;
                int height = 0;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Console.WriteLine("开始预测模型输出绘制");
                foreach (var path in imaesg)
                {
                    using var image = Image.Load(path);

                    if (width == 0 && height == 0)
                    {
                        width = image.Width;
                        height = image.Height;
                    }

                    //预测结果
                    var boxes = Suppress(ParseOutput(_predictor.Predict(new ImageWrapper { Path = path, Image = image }), image.Width, image.Height));

                    //绘制边框
                    DrawBoxes(image, boxes);
                    image.Save(Path.Combine(predictorsFolder, $"{Path.GetFileName(path)}"));

                    image.Dispose();
                }
                sw.Stop();
                Console.WriteLine("结果预测完成" + sw.Elapsed + "s");

                //合成图片为视频
                var imageFiles = Directory.GetFiles(predictorsFolder).OrderBy(f => int.Parse(Path.GetFileName(f).Split('.')[0])).ToList();
                //using var writer = new VideoWriter(savaPath, VideoWriter.FourCC('m', 'p', '4', 'v'), capture.Fps, new OpenCvSharp.Size(width, height));
                //openh264-1.8.0-win64.dll
                using var writer = new VideoWriter(savaPath, VideoWriter.FourCC('a', 'v', 'c', '1'), capture.Fps, new OpenCvSharp.Size(width, height));
                //using var writer = new VideoWriter(savaPath, FourCC.MP4V, capture.Fps, new OpenCvSharp.Size(width, height));
                foreach (var path in imageFiles)
                {
                    using var frame = Cv2.ImRead(path);
                    writer.Write(frame);
                }

                //删除临时文件夹
                Directory.Delete(baseFolder, true);

            }


            //如果是图片
            if (ImageTypes.Contains(extension))
            {
                using var image = Image.Load(imagePath);

                var output = _predictor.Predict(new ImageWrapper { Path = imagePath, Image = image });

                //预测结果
                var boxes = Suppress(
                    ParseOutput(output, image.Width, image.Height)
                );

                //绘制边框
                DrawBoxes(image, boxes);
                image.Save(savaPath);

            }

            return savaPath;
        }

        /// <summary>
        /// 批量预测
        /// </summary>
        /// <param name="imagesFolder"></param>
        /// <param name="outFolder"></param>
        public void Transform(string imagesFolder, string outFolder)
        {

            if (!Directory.Exists(outFolder))
            {
                Directory.CreateDirectory(outFolder);
            }

            //检验
            var imaesg = Directory.GetFiles(imagesFolder);
            foreach (var item in imaesg)
            {
                var extension = Path.GetExtension(item).ToLower();

                if (VideoTypes.Contains(extension))
                {
                    throw new Exception("批量推理不支持视频");
                }

                if (!VideoTypes.Contains(extension) && !ImageTypes.Contains(extension))
                {
                    throw new Exception("不支持该类型的文件格式：" + extension);
                }
            }

            //预测
            var outputs = _predictor.Transform(imagesFolder);
            for (int i = 0; i < imaesg.Count(); i++)
            {
                using var image = Image.Load(imaesg[i]);

                //预测结果
                var boxes = Suppress(
                    ParseOutput(outputs.ElementAt(i), image.Width, image.Height)
                );

                //绘制边框
                DrawBoxes(image, boxes);
                image.Save(Path.Combine(outFolder, $"{Path.GetFileName(imaesg[i])}"));
            }
        }

        /// <summary>
        /// 去除重叠
        /// </summary>
        /// <param name="predictions"></param>
        /// <returns></returns>
        private IList<BoundingBox> Suppress(IList<BoundingBox> predictions)
        {

            var result = new List<BoundingBox>(predictions);

            foreach (var item in predictions)
            {
                foreach (var current in result.ToList())
                {
                    if (current == item) continue;

                    var (rect1, rect2) = (item.Rectangle, current.Rectangle);

                    RectangleF intersection = RectangleF.Intersect(rect1, rect2);

                    float intArea = intersection.Width * intersection.Height;
                    float unionArea = rect1.Width * rect1.Height + rect2.Width * rect2.Height - intArea;
                    float overlap = intArea / unionArea;

                    if (overlap >= _predictor.Overlap)
                    {
                        if (item.Confidence >= current.Confidence)
                        {
                            result.Remove(current);
                        }
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 格式化输出参数
        /// </summary>
        /// <param name="output"></param>
        /// <param name="originalWidth"></param>
        /// <param name="originalHeight"></param>
        /// <returns></returns>
        private IList<BoundingBox> ParseOutput(float[] output, int originalWidth, int originalHeight)
        {
            var (xGain, yGain) = (_predictor.ModelInputWidth / (float)originalWidth, _predictor.ModelInputHeight / (float)originalHeight);
            var (xPad, yPad) = ((_predictor.ModelInputWidth - originalWidth * xGain) / 2, (_predictor.ModelInputHeight - originalHeight * yGain) / 2);

            List<BoundingBox> claases = new List<BoundingBox>();
            for (int i = 0; i < _predictor.Batch; i++)
            {
                int offset = i * _predictor.ChannelCount * _predictor.AnchorCount;

                for (int j = 0; j < _predictor.AnchorCount; j++)
                {
                    //原始坐标及宽高
                    //绘制框中心点x坐标
                    float cx = output[offset + 0 + j];
                    //绘制框中心点y坐标
                    float cy = output[offset + 1 * _predictor.AnchorCount + j];
                    //绘制框宽度
                    float w = output[offset + 2 * _predictor.AnchorCount + j];
                    //绘制框高度
                    float h = output[offset + 3 * _predictor.AnchorCount + j];

                    //放大到原图坐标及宽高
                    // 计算坐标
                    float zsjx = cx - w / 2;
                    float zsjy = cy - h / 2;

                    float xMin = (cx - w / 2 - xPad) / xGain;
                    float yMin = (cy - h / 2 - yPad) / yGain;
                    float xMax = (cx + w / 2 - xPad) / xGain;
                    float yMax = (cy + h / 2 - yPad) / yGain;


                    xMin = Clamp(xMin, 0, originalWidth - 0);
                    yMin = Clamp(yMin, 0, originalHeight - 0);
                    xMax = Clamp(xMax, 0, originalWidth - 1);
                    yMax = Clamp(yMax, 0, originalHeight - 1);



                    for (int classIdx = 0; classIdx < _predictor.ClassesCount; classIdx++)
                    {
                        float conf = output[(4 + classIdx) * _predictor.AnchorCount + j];

                        // 过滤低置信度
                        if (conf < _predictor.Confidence) continue;

                        claases.Add(new BoundingBox
                        {
                            Rectangle = new RectangleF(xMin, yMin, xMax - xMin, yMax - yMin),
                            Label = _predictor.Classes[classIdx],
                            Confidence = conf,
                            BoxColor = _predictor.ClassesColors[classIdx]
                        });
                    }
                }
            }
            return claases;
        }

        /// <summary>
        /// 绘制预测边框
        /// </summary>
        /// <param name="image"></param>
        /// <param name="predictions"></param>
        private void DrawBoxes(Image image, IList<BoundingBox> predictions)
        {
            foreach (var pred in predictions)
            {
                var originalImageHeight = image.Height;
                var originalImageWidth = image.Width;

                var x = (int)Math.Max(pred.Rectangle.X, 0);
                var y = (int)Math.Max(pred.Rectangle.Y, 0);
                var width = (int)Math.Min(originalImageWidth - x, pred.Rectangle.Width);
                var height = (int)Math.Min(originalImageHeight - y, pred.Rectangle.Height);

                //Note that the output is already scaled to the original image height and width.

                // Bounding Box Text
                string text = $"{pred.Label} [{pred.Confidence}]";
                var size = TextMeasurer.MeasureSize(text, new TextOptions(_font));

                image.Mutate(d => d.Draw(Pens.Solid(pred.BoxColor, 2), new Rectangle(x, y, width, height)));
                image.Mutate(d => d.DrawText(text, _font, pred.BoxColor, new SixLabors.ImageSharp.Point(x, (int)(y - size.Height - 1))));
            }
        }

        private float Clamp(float value, float min, float max)
        {
            return value < min ? min : value > max ? max : value;
        }
    }
}
