using System;
using System.Linq;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using SolutionsUtilities.UI.WPF.Highlighting;

namespace Barings.Controls.WPF.CodeEditors.Highlighting
{

    public class HighlightMatchingWords : DocumentColorizingTransformer
    {
        public string Word { private get; set; }
        //public string Theme { get; set; }

        public HighlightMatchingWords(string word = null)
        {
            Word = word ?? "testing";
        }

        private bool ValidateWord(int startOffset, int endOffset)
        {

            // Validate that a word at the given offset is equal ONLY to the current Word
            var document = CurrentContext.Document;

            // If the text before or after is at the beginning or end of the text, set it to a space to make it a stop character
            var textBeforeOffset = startOffset - 1 < 0 ? " " : document.GetText(startOffset - 1, 1);
            var textAfterOffset = endOffset + 1 > document.TextLength ? " " : document.GetText(endOffset, 1);

            // Return the result of this expression
            return AvalonEditExtensions.StopCharacters.Any(textBeforeOffset.Contains)
                && AvalonEditExtensions.StopCharacters.Any(textAfterOffset.Contains);
        }

        protected override void ColorizeLine(DocumentLine line)
        {

            if (string.IsNullOrEmpty(Word) || Word == Environment.NewLine) return;

            int lineStartOffset = line.Offset;
            string text = CurrentContext.Document.GetText(line);
            int start = 0;
            int index;

            var backgroundBrush = new SolidColorBrush(Color.FromArgb(80, 124, 172, 255));

            while ((index = text.IndexOf(Word, start, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                int startOffset = lineStartOffset + index;
                int endOffset = lineStartOffset + index + Word.Length;

                if (!ValidateWord(startOffset, endOffset))
                {
                    start = index + 1;
                    continue;
                }

                ChangeLinePart(
                    startOffset, // startOffset
                    endOffset, // endOffset
                    element =>
                    {
                        // This lambda gets called once for every VisualLineElement
                        // between the specified offsets.
                        element.TextRunProperties.SetBackgroundBrush(backgroundBrush);
                    });
                start = index + 1; // search for next occurrence
            }
        }
    }
}
