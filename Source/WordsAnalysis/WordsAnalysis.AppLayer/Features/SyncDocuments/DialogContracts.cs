using DocumentsModel;
using System.Collections.Immutable;

namespace WordsAnalysis.AppLayer.Features.SyncDocuments;

// EditWordDialog
public record EditWordDialogContent(EditionState Edition, WordReference WordReference, int PageWidth, int PageHeight, bool IsAdd);
public record EditWordDialogResult(OcrWord? Word, bool After);

// DeleteWordsDialog
public record DeleteWordsDialogContent(EditionState EditionState, KeyValuePair<WordReference, string?>[] Words);
public record DeleteWordsDialogResult(WordReference[] DeletedWords);

// RescanAreaDialog
public record RescanAreaDialogContent(EditionState Edition, WordReference WordReference);
public record RescanAreaDialogResult(IEnumerable<OcrWord> Words);

// SplitWordsDialog
public record SplitWordSuggestion(SplitWord[] Words);
public record SplitWord(string Text, OcrRect Bounds);
public record SplitWordsDialogContent(EditionState Edition, WordReference WordReference, SplitWordSuggestion[] Suggestions, int PageWidth, int PageHeight);
public record SplitWordsDialogResult(SplitWordSuggestion? Suggestion);

// ViewColumnImagesDialog
public record ViewColumnImagesDialogContent(ImmutableDictionary<OcrBookInfo, EditionState> Editions, IEnumerable<WordReference?> WordReferences);
