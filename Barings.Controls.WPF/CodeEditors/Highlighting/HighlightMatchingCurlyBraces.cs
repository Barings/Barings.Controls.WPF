using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace Barings.Controls.WPF.CodeEditors.Highlighting
{

    public class HighlightMatchingCurlyBraces : DocumentColorizingTransformer
    {

        public int[] CurlyBraces { set; get; }

        public HighlightMatchingCurlyBraces(int[] curlyBraces)
        {
            CurlyBraces = curlyBraces;
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            if (CurlyBraces == null) return;

            var backgroundBrush = new SolidColorBrush(Color.FromArgb(100, 255, 175, 100));

            var leftLine = CurrentContext.Document.GetLineByOffset(CurlyBraces[0]);
            var rightLine = CurrentContext.Document.GetLineByOffset(CurlyBraces[1]);

            // Left parenthesis
            if (CurlyBraces[0] >= 0 && leftLine == line)
                ChangeLinePart(
                    CurlyBraces[0],
                    CurlyBraces[0] + 1,
                    element =>
                    {
                        element.TextRunProperties.SetBackgroundBrush(backgroundBrush);
                    });

            // Right parenthesis
            if (CurlyBraces[1] >= 0 && rightLine == line)
                ChangeLinePart(
                    CurlyBraces[1],
                    CurlyBraces[1] + 1,
                    element =>
                    {
                        element.TextRunProperties.SetBackgroundBrush(backgroundBrush);
                    });
        }
    }
}
