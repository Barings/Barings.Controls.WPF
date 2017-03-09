using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
using SolutionsUtilities.UI.WPF.Highlighting;
using System;
using System.IO;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Xml;
using Barings.Controls.WPF.CodeEditors.Highlighting;

namespace Barings.Controls.WPF.CodeEditors
{
    /// <summary>
    /// Interaction logic for XmlEditor.xaml
    /// </summary>
    public partial class XmlEditor
    {
        #region FIELDS AND PROPERTIES

        private bool _NeedRaiseCaretChangedDelayed;
        private string _Mode = "xml";
        private readonly Timer _Timer = new Timer();
        private HighlightMatchingWords _HighlightMatchingWords;
        private HighlightMatchingParentheses _HighlightMatchingParentheses;
        private HighlightMatchingCurlyBraces _HighlightMatchingCurlyBraces;
        private bool _ShowOptions = true;

        public bool ShowOptions
        {
            get { return _ShowOptions; }
            set
            {
                _ShowOptions = value;
                SettingsPanel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private string HighlightedWord { get; set; }
        private int[] Parentheses { get; set; }
        private int[] CurlyBraces { get; set; }

        public string Text
        {
            get { return TextScript.Text; }
            set { TextScript.Text = value; }
        }

        #endregion

        #region CONSTRUCTOR

        public XmlEditor()
        {
            InitializeComponent();

            RegisterHighlighter();

            SetOptions();

            TextScript.TextArea.Caret.PositionChanged += CaretOnPositionChanged;

            SearchPanel.Install(TextScript.TextArea);
        }

        #endregion

        #region METHODS

        private static void ResetTimer(Timer timer)
        {
            timer.Stop();
            timer.Start();
        }

        private void SetParenthesesHighlighter()
        {
                if (_HighlightMatchingParentheses == null)
                {
                    _HighlightMatchingParentheses = new HighlightMatchingParentheses(Parentheses);
                    TextScript.TextArea.TextView.LineTransformers.Add(_HighlightMatchingParentheses);
                }
                else
                {
                    _HighlightMatchingParentheses.Parentheses = Parentheses;
                }

                TextScript.TextArea.TextView.Redraw();
        }

        private void SetCurlyBracesHighlighter()
        {
            if (_HighlightMatchingCurlyBraces == null)
            {
                _HighlightMatchingCurlyBraces = new HighlightMatchingCurlyBraces(CurlyBraces);
                TextScript.TextArea.TextView.LineTransformers.Add(_HighlightMatchingCurlyBraces);
            }
            else
            {
                _HighlightMatchingCurlyBraces.CurlyBraces = CurlyBraces;
            }

            TextScript.TextArea.TextView.Redraw();
        }

        private void SetWordMatchHighlighter()
        {
                if (_HighlightMatchingWords == null)
                {
                    _HighlightMatchingWords = new HighlightMatchingWords();
                    TextScript.TextArea.TextView.LineTransformers.Add(_HighlightMatchingWords);
                }
                else
                {
                    _HighlightMatchingWords.Word = HighlightedWord;
                }

                TextScript.TextArea.TextView.Redraw();
            
        }

        private void SetOptions()
        {
            TextScript.Options.HighlightCurrentLine = true;
            TextScript.Options.EnableHyperlinks = false;
            TextScript.WordWrap = true;

            ShowOptions = false;

            _Timer.Interval = 1000;
            _Timer.Elapsed += TimerOnElapsed;
        }

        private void FlipHighlighterStyle()
        {
                if (_Mode == "xml")
                {
                    _Mode = "json";
                    RegisterHighlighter("json");
                }
                else
                {
                    _Mode = "xml";
                    RegisterHighlighter();
                }
                TextScript.TextArea.TextView.Redraw();
                Dispatcher.Invoke(CaretOnPositionChangedDelayed);
            
        }

        public void SetText(string text)
        {
            if (text.IndexOf('{', 0).Equals(0))
            {
                RegisterHighlighter("json");
            }
            else
            {
                RegisterHighlighter("xml");
            }
            Text = text;
        }

        private void RegisterHighlighter(string mode = "xml")
        {
            //_Mode = mode;

            string fileName = "Barings.Controls.WPF.CodeEditors.Resources.XML-Mode.xshd";

            if (mode == "json")
            {
                fileName = "Barings.Controls.WPF.CodeEditors.Resources.JavaScript-Mode.xshd";
            }
            
                using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
                {
                    if (s == null) throw new InvalidOperationException("Stream could not be opened to file: " + fileName);

                    using (XmlTextReader reader = new XmlTextReader(s))
                    {
                        TextScript.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                }
        }

        private int lastUsedIndex = 0;

        public void Find(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                lastUsedIndex = 0;
                TextScript.ScrollToLine(1);
                return;
            }

            string editorText = TextScript.Text;

            if (string.IsNullOrEmpty(editorText))
            {
                lastUsedIndex = 0;
                return;
            }

            if (lastUsedIndex >= query.Length)
            {
                lastUsedIndex = 0;
            }

            int nIndex = editorText.IndexOf(query, lastUsedIndex, StringComparison.OrdinalIgnoreCase);
            if (nIndex != -1)
            {
                TextScript.Select(nIndex, query.Length);
                var position = TextScript.TextArea.Selection.StartPosition;
                TextScript.ScrollToLine(position.Line);
                lastUsedIndex = nIndex + query.Length;
            }
            else
            {
                lastUsedIndex = 0;
                TextScript.ScrollToLine(1);
            }
        }

        #endregion

        #region EVENTS

        private void CaretOnPositionChanged(object sender, EventArgs eventArgs)
        {
            var caretLocation = TextScript.TextArea.Caret.Location;

            Parentheses = AvalonEditExtensions.CheckForCharacterAtLocation(TextScript.Document, caretLocation, '(', ')');
            CurlyBraces = AvalonEditExtensions.CheckForCharacterAtLocation(TextScript.Document, caretLocation, '{', '}');

            SetParenthesesHighlighter();
            SetCurlyBracesHighlighter();

            _NeedRaiseCaretChangedDelayed = true;
            ResetTimer(_Timer);
        }

        private void CaretOnPositionChangedDelayed()
        {
                if (TextScript.TextArea.Caret == null) return;

                var caretLocation = TextScript.TextArea.Caret.Location;

                HighlightedWord = AvalonEditExtensions.GetWordAtLocation(TextScript.Document, caretLocation);

                SetWordMatchHighlighter();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _Timer.Enabled = false;
            if (_NeedRaiseCaretChangedDelayed)
            {
                _NeedRaiseCaretChangedDelayed = false;
                Dispatcher.Invoke(CaretOnPositionChangedDelayed);
            }
        }

        #endregion

        private void ShowSettings_OnClick(object sender, RoutedEventArgs e)
        {
            var currentValue = ShowOptions;

            ShowOptions = !currentValue;

            ShowSettings.Header = ShowOptions ? "Hide Options" : "Show Options";
        }

        private void CheckBoxDarkMode_OnClick(object sender, RoutedEventArgs e)
        {
            FlipHighlighterStyle();
        }
    }
}
