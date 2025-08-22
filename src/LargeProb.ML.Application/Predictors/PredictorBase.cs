using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using SixLabors.ImageSharp;

namespace LargeProb.ML.Application.Predictors
{
    /// <summary>
    /// 预测器基类
    /// </summary>
    public class PredictorBase : IDisposable
    {
        /// <summary>
        /// 模型地址
        /// </summary>
        protected readonly string _modelPath;

        /// <summary>
        /// 类别
        /// </summary>
        public string[] Classes { get; protected set; }

        /// <summary>
        /// 类别绘制颜色
        /// </summary>
        public Color[] ClassesColors { get; protected set; }

        /// <summary>
        /// 最低置信度
        /// </summary>
        public float Confidence { get; } = 0.2f;

        /// <summary>
        /// 重叠度
        /// </summary>
        public float Overlap { get; } = 0.45f;


        /// <summary>
        /// 模型输入列名
        /// </summary>
        public string ModelInputName { get; protected set; }

        /// <summary>
        /// 模型输入宽度
        /// </summary>
        public int ModelInputWidth { get; protected set; }

        /// <summary>
        /// 模型输入高度
        /// </summary>
        public int ModelInputHeight { get; protected set; }


        /// <summary>
        /// 模型输出张量列名
        /// </summary>
        public string ModelOutputName { get; protected set; }

        /// <summary>
        /// 模型输出张量Batch
        /// </summary>
        public int Batch { get; protected set; }

        /// <summary>
        /// 模型输出张量绘制+类别
        /// </summary>
        public int ChannelCount { get; protected set; }

        /// <summary>
        /// 模型输出张量绘制+类别
        /// </summary>
        public int ClassesCount { get; protected set; }

        /// <summary>
        /// 模型输出张量预测数量
        /// </summary>
        public int AnchorCount { get; protected set; }

        /// <summary>
        /// ONNX接口
        /// </summary>
        protected InferenceSession _inferenceSession;


        public PredictorBase(string modelPath, string[] classes, Color[] classColors, bool useCuda = false)
        {
            _modelPath = modelPath;
            Classes = classes;
            ClassesColors = classColors;

            //GPU 推理
            if (useCuda)
            {
                //异常 Could not locate cudnn_graph64_9.dll
                //安装cudnn 9.x版本，将bin 目录下的文件全部复制到Cuda的bin目录下
                SessionOptions sessionOptions = new SessionOptions();
                //sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_DISABLE_ALL;
                sessionOptions.AppendExecutionProvider_CUDA(0);
                _inferenceSession = new InferenceSession(_modelPath, sessionOptions);
            }
            //CPU 推理
            else
            {
                //异常 The type initializer for 'Microsoft.ML.OnnxRuntime.NativeMethods' threw an e
                //本机安装VC++运行时
                _inferenceSession = new InferenceSession(_modelPath);
            }

            //获取输入参数
            GetInputDetails();
            //获取输出参数
            GetOutputDetails();
        }

        

        /// <summary>
        /// 输入参数
        /// </summary>
        protected void GetInputDetails()
        {
            ModelInputName = _inferenceSession.InputMetadata.Keys.First();
            ModelInputWidth = _inferenceSession.InputMetadata[ModelInputName].Dimensions[2];
            ModelInputHeight = _inferenceSession.InputMetadata[ModelInputName].Dimensions[3];
        }

        /// <summary>
        /// 输出参数
        /// </summary>
        protected virtual void GetOutputDetails()
        {
            ModelOutputName = _inferenceSession.OutputMetadata.Keys.First();

            Batch = _inferenceSession.OutputMetadata[ModelOutputName].Dimensions[0];
            ChannelCount = _inferenceSession.OutputMetadata[ModelOutputName].Dimensions[1];
            AnchorCount = _inferenceSession.OutputMetadata[ModelOutputName].Dimensions[2];
            ClassesCount = ChannelCount - 4;
        }

        public void Dispose()
        {
            _inferenceSession?.Dispose();
        }
    }
}
