using Matritsa.PDFGenerator.Util;
using System;

namespace Matritsa.PDFGenerator.Data {

    /// <summary>
    /// Параметры файла.
    /// </summary>
    public class PDFOptions {

        private Dimensions<float>? _MFSPointCache;
        private Dimensions<float>? _MFSCache;
        private float? _MSCache;
        private float? _MSPointCache;

        /// <summary>
        /// Вид бумаги.
        /// </summary>
        public PaperType PaperType;
        /// <summary>
        /// Размер матрицы в миллиметрах.
        /// </summary>
        public float MatrixSize {
            get {
                if (_MSCache == null) {
                    throw new NullReferenceException("Matrix size is null");
                }
                return _MSCache.Value;
            }

            set {
                _MSCache = value;
                _MSPointCache = PointConversion.ToPoints((float)_MSCache, MeasurementUnit.Millimeter);
            }
        }
        /// <summary>
        /// Размер матрицы в поинтах.
        /// </summary>
        public float MatrixSizeInPoints {
            get {
                if (_MSPointCache == null) {
                    if (_MSCache == null) {
                        throw new NullReferenceException("Matrix size is null");
                    }
                    _MSPointCache = PointConversion.ToPoints((float)_MSCache, MeasurementUnit.Millimeter);
                }
                return (float)_MSPointCache;
            }
        }
        /// <summary>
        /// Размер рамки матрицы (пространства вокруг нее).
        /// </summary>
        public Dimensions<float> MatrixFrameSize {
            get {
                if (_MFSCache == null) {
                    throw new NullReferenceException("Matrix frame size is null");
                } else {
                    return _MFSCache;
                }
            }
            set {
                _MFSCache = value;
                _MFSPointCache = value.ToPoints();
            }
        }
        /// <summary>
        /// Размер рамки матрицы (пространства вокруг нее) в поинтах.
        /// </summary>
        public Dimensions<float> MatrixFrameSizeInPoints {
            get {
                if (_MFSPointCache == null) {
                    _MFSPointCache = MatrixFrameSize.ToPoints();
                }
                return _MFSPointCache;
            }
        }

        public PDFOptions(PaperType type, Dimensions<float> frameSize, float matrixSize) {
            this.PaperType = type;
            this.MatrixFrameSize = frameSize;
            this.MatrixSize = matrixSize;
        }
    }
}
