using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LargeProb.ML.Application.Share;
using SixLabors.ImageSharp;

namespace LargeProb.ML.Application.Contracts
{
    public interface IPredictorService : IDisposable
    {
        /// <summary>
        /// 类别
        /// </summary>
        string[] Classes { get; }

        /// <summary>
        /// 类别绘制颜色
        /// </summary>
        Color[] ClassesColors { get; }

        /// <summary>
        /// 最低置信度
        /// </summary>
        float Confidence { get; }

        /// <summary>
        /// 重叠度
        /// </summary>
        float Overlap { get; }


        /// <summary>
        /// 模型输入列名
        /// </summary>
        string ModelInputName { get; }

        /// <summary>
        /// 模型输入宽度
        /// </summary>
        int ModelInputWidth { get; }

        /// <summary>
        /// 模型输入高度
        /// </summary>
        int ModelInputHeight { get; }


        /// <summary>
        /// 模型输出张量列名
        /// </summary>
        string ModelOutputName { get; }

        /// <summary>
        /// 模型输出张量Batch
        /// </summary>

        int Batch { get; }

        /// <summary>
        /// 模型输出张量绘制+类别
        /// </summary>
        int ChannelCount { get; }

        /// <summary>
        /// 模型输出张量绘制+类别
        /// </summary>
        int ClassesCount { get; }

        /// <summary>
        /// 模型输出张量预测数量
        /// </summary>
        int AnchorCount { get; }

        /// <summary>
        /// 批量预测
        /// </summary>
        /// <param name="imagesFolder"></param>
        /// <returns></returns>
        IEnumerable<float[]> Transform(string imagesFolder);

        /// <summary>
        /// 单个预测
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="outFolder"></param>
        float[] Predict(ImageWrapper imagePath);
    }
}
