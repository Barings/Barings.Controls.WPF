using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace Barings.Controls.WPF.CodeEditors.Highlighting
{
    public class HighlightMatchingParentheses : DocumentColorizingTransformer
    {

        public int[] Parentheses { set; get; }

        public HighlightMatchingParentheses(int[] parentheses)
        {
            Parentheses = parentheses;
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            if (Parentheses == null) return;

            var backgroundBrush = new SolidColorBrush(Color.FromArgb(100, 255, 175, 100));

            var leftLine = CurrentContext.Document.GetLineByOffset(Parentheses[0]);
            var rightLine = CurrentContext.Document.GetLineByOffset(Parentheses[1]);

            // Left parenthesis
            if (Parentheses[0] >= 0 && leftLine == line)
                ChangeLinePart(
                    Parentheses[0],
                    Parentheses[0] + 1,
                    element =>
                    {
                        element.TextRunProperties.SetBackgroundBrush(backgroundBrush);
                    });

            // Right parenthesis
            if (Parentheses[1] >= 0 && rightLine == line)
                ChangeLinePart(
                    Parentheses[1],
                    Parentheses[1] + 1,
                    element =>
                    {
                        element.TextRunProperties.SetBackgroundBrush(backgroundBrush);
                    });
        }
    }
}
