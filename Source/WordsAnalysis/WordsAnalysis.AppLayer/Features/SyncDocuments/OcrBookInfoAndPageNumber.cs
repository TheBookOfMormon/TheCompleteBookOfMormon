using DocumentsModel;

namespace WordsAnalysis.AppLayer.Features.SyncDocuments;

public record OcrBookInfoAndPageNumber(OcrBookInfo BookInfo, int PageNumber);
