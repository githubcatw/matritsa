using Matritsa.PDFGenerator.Data;
using matritsa.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matritsa.Util {
    /// <summary>
    /// В этом классе будут дополнительные функции для Matritsa.PDFGenerator, тесно связанные с Матрицей.
    /// </summary>
    internal static class PdfGenExtensions {
        public static string GetProgressString(this PDFGenerationUpdate update) {
            if (update.Stage == PDFGenerationStage.Layout) return Resources.generateProcessLayout;
            else if (update.Stage == PDFGenerationStage.Render) {
                if (update.CurrentPage != null) {
                    return string.Format(Resources.generateProcessRenderPage, update.CurrentPage+1, update.TotalPages);
                }
                return Resources.generateProcessRender;
            }
            return Resources.generateProcess;
        }
    }
}
