using System.Text.Json.Serialization;

namespace DocumentsModel;

[JsonSerializable(typeof(BenefitOfDoubt))]
[JsonSerializable(typeof(OcrBookInfo))]
[JsonSerializable(typeof(OcrElement))]
[JsonSerializable(typeof(OcrPage))]
[JsonSerializable(typeof(OcrPageMeta))]
[JsonSerializable(typeof(OcrRect))]
[JsonSerializable(typeof(OcrWord))]
[JsonSerializable(typeof(PageRange))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class ModelJsonContext : JsonSerializerContext
{
}
