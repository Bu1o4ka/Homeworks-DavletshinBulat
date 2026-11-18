namespace MiniTemplateEngine;

/// <summary>
/// Определяет контракт HTML-шаблонизатора для формирования HTML-разметки на основе шаблона и модели данных.
/// </summary>
public interface IHtmlTemplateRenderer
{
    /// <summary>
    /// Выполняет обработку HTML-шаблона, переданного в виде строки.
    /// </summary>
    /// <param name="htmlTemplate">Строка, содержащая HTML-шаблон с конструкциями.</param>
    /// <param name="dataModel">Объект модели данных для подстановки.</param>
    /// <returns>Готовая HTML-разметка.</returns>
    string RenderFromString(string htmlTemplate, object dataModel);

    /// <summary>
    /// Выполняет обработку HTML-шаблона, загруженного из файла.
    /// </summary>
    /// <param name="filePath">Путь к исходному HTML-файлу.</param>
    /// <param name="dataModel">Объект модели данных.</param>
    /// <returns>Готовая HTML-разметка.</returns>
    string RenderFromFile(string filePath, object dataModel);

    /// <summary>
    /// Выполняет обработку HTML-шаблона из файла и сохраняет результат в указанный файл.
    /// </summary>
    /// <param name="inputFilePath">Путь к исходному файлу шаблона.</param>
    /// <param name="outputFilePath">Путь для сохранения результата.</param>
    /// <param name="dataModel">Объект модели данных.</param>
    /// <returns>Строка с готовой HTML-разметкой.</returns>
    string RenderToFile(string inputFilePath, string outputFilePath, object dataModel);
}