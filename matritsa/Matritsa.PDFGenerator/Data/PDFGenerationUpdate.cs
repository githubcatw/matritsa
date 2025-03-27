namespace Matritsa.PDFGenerator.Data {
    /// <summary>
    /// Информация о процессе генерации PDF-файла.
    /// </summary>
    public class PDFGenerationUpdate {
        /// <summary>
        /// Стадия генерации.
        /// </summary>
        public PDFGenerationStage Stage { get; }
        /// <summary>
        /// Текущая страница (только в случае PDFGenerationStage.Render, иначе null).
        /// </summary>
        public int? CurrentPage { get; }
        /// <summary>
        /// Текущий элемент.
        /// </summary>
        public int CurrentElement { get; }
        /// <summary>
        /// Прогресс.
        /// </summary>
        public float Progress { get; }
        /// <summary>
        /// Общее количество страниц.
        /// </summary>
        public int TotalPages { get; }

        public PDFGenerationUpdate(PDFGenerationStage stage, int currentElement, float progress, int totalPages, int? currentPage = null) {
            Stage = stage;
            CurrentPage = currentPage;
            CurrentElement = currentElement;
            TotalPages = totalPages;
            Progress = progress;
        }
    }
}
