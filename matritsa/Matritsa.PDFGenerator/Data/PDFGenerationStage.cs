namespace Matritsa.PDFGenerator.Data {
    /// <summary>
    /// Стадия генерации.
    /// </summary>
    public enum PDFGenerationStage {
        /// <summary>
        /// Никакая.
        /// </summary>
        None,
        /// <summary>
        /// Верстка.
        /// </summary>
        Layout,
        /// <summary>
        /// Отрисовка файла.
        /// </summary>
        Render
    }
}
