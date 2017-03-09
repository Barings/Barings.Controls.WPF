using System.Linq;
using ICSharpCode.AvalonEdit.Document;

namespace SolutionsUtilities.UI.WPF.Highlighting
{

    public static class AvalonEditExtensions
    {
        public static readonly string[] StopCharacters =
        {
            "{", "}", "[", "]", "(", ")",
            "\"", "'", ",", "-", ".", "*", "/",
            "=", " ", ";", "\r", "\n", "\t", "<", ">"
        };

        public static int[] CheckForCharacterAtLocation(TextDocument document, TextLocation location, char open, char close)
        {
            if (document.TextLength == 0) return null;

            var offset = document.GetOffset(location);
            var workingOffset = offset;

            if (workingOffset >= document.TextLength)
                workingOffset--;

            if (workingOffset - 1 < 0) return null;
            workingOffset--;


            var textAtOffset = document.GetText(workingOffset, 1);
            int intermediateCount = 0;

            var matches = new int[2];

            if (textAtOffset == close.ToString())
            {
                // Found the right parenthesis, set location
                matches[1] = workingOffset;

                // Now go until the beginning of the document
                while (workingOffset - 1 >= 0)
                {
                    workingOffset--;
                    textAtOffset = document.GetText(workingOffset, 1);

                    // If we run into another right, count it
                    if (textAtOffset == close.ToString())
                    {
                        intermediateCount++;
                        workingOffset--;
                        continue;
                    }
                    // If we run into a left, take away from the intermediate
                    if (textAtOffset == open.ToString())
                    {
                        // If the count is down to zero this is the matcher, set it and return;
                        if (intermediateCount == 0)
                        {
                            matches[0] = workingOffset;
                            return matches;
                        }

                        intermediateCount--;
                    }

                }
            }

            // Reset working offset to original position
            workingOffset = offset;

            if (workingOffset + 1 > document.TextLength) return null;

            textAtOffset = document.GetText(workingOffset, 1);

            if (textAtOffset == open.ToString())
            {
                // Found the left parenthesis, set location
                matches[0] = workingOffset;

                // Now go until end of the document
                while (workingOffset + 1 < document.TextLength)
                {
                    workingOffset++;
                    textAtOffset = document.GetText(workingOffset, 1);

                    // If we run into another left, count it
                    if (textAtOffset == open.ToString())
                    {
                        intermediateCount++;
                        workingOffset++;
                        continue; ;
                    }
                    // If we run into a right, take away from the intermediate
                    if (textAtOffset == close.ToString())
                    {
                        // If the count is down to zero this is the matcher, set it and return
                        if (intermediateCount == 0)
                        {
                            matches[1] = workingOffset;
                            return matches;
                        }

                        intermediateCount--;
                    }
                }
            }

            return null;
        }

        public static string GetWordAtLocation(TextDocument document, TextLocation location)
        {
            if (document.TextLength == 0) return string.Empty;

            string wordHovered = string.Empty;

            var line = location.Line;
            var column = location.Column;

            var offset = document.GetOffset(line, column);
            if (offset >= document.TextLength)
                offset--;

            if (offset - 1 < 0) return string.Empty;
            offset--;
            var textAtOffset = document.GetText(offset, 1);

            // Get text backward of the mouse position, until the first space
            while (!string.IsNullOrWhiteSpace(textAtOffset) && !StopCharacters.Any(textAtOffset.Contains))
            {
                wordHovered = textAtOffset + wordHovered;

                offset--;

                if (offset < 0)
                    break;

                textAtOffset = document.GetText(offset, 1);
            }

            // Get text forward the mouse position, until the first space
            offset = document.GetOffset(line, column);
            if (offset <= 0) return string.Empty;
            if (offset < document.TextLength - 1)
            {
                //offset++;

                textAtOffset = document.GetText(offset, 1);

                while (!string.IsNullOrWhiteSpace(textAtOffset) && !StopCharacters.Any(textAtOffset.Contains))
                {
                    wordHovered = wordHovered + textAtOffset;

                    offset++;

                    if (offset >= document.TextLength)
                        break;

                    textAtOffset = document.GetText(offset, 1);
                }
            }

            return wordHovered;
        }
    }
}
