﻿using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using Barings.Controls.WPF.CodeEditors.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
using SolutionsUtilities.UI.WPF.Highlighting;

namespace Barings.Controls.WPF.CodeEditors
{
    public partial class CSharpEditor
    {
        #region FIELDS AND PROPERTIES

        private bool _NeedRaiseCaretChangedDelayed;
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

        public CSharpEditor()
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

        //private void FlipHighlighterStyle()
        //{
        //        if (_Mode == "standard")
        //        {
        //            var converter = new BrushConverter();

        //            TextScript.Foreground = (Brush)converter.ConvertFromString("#dbe0e1");
        //            TextScript.Background = (Brush)converter.ConvertFromString("#182a54");
        //            RegisterHighlighter("dark");
        //            //CheckBoxDarkMode.IsChecked = true;
        //        }
        //        else
        //        {
        //            TextScript.Foreground = Brushes.Black;
        //            TextScript.Background = Brushes.White;
        //            RegisterHighlighter();
        //            //CheckBoxDarkMode.IsChecked = false;
        //        }
        //        TextScript.TextArea.TextView.Redraw();
        //        Dispatcher.Invoke(CaretOnPositionChangedDelayed);
            
        //}

        private void RegisterHighlighter(string mode = "standard")
        {
            const string fileName = "Barings.Controls.WPF.CodeEditors.Resources.CSharp-Mode.xshd";
            
                using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
                {
                    if (s == null) throw new InvalidOperationException("Stream could not be opened with path: " + fileName);

                    using (XmlTextReader reader = new XmlTextReader(s))
                    {
                        TextScript.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
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

        //private void CheckBoxDarkMode_OnClick(object sender, RoutedEventArgs e)
        //{
        //    FlipHighlighterStyle();
        //}

        private void FindText_OnClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
