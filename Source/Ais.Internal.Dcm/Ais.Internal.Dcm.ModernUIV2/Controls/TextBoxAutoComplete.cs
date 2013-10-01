using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Ais.Internal.Dcm.ModernUIV2.Common;

namespace Ais.Internal.Dcm.ModernUIV2.Controls
{
    public static class TextBoxAutoComplete
    {
        #region Dependency Properties
        public static readonly DependencyProperty WordAutoCompleteSourceProperty;
        public static readonly DependencyProperty WordAutoCompleteSeparatorsProperty;
        public static readonly DependencyProperty WordAutoCompletePopupProperty;

        private static readonly DependencyProperty WordAutoCompleteWordsHostProperty;
        private static readonly DependencyPropertyKey WordAutoCompleteWordsHostPropertyKey;
        private static readonly DependencyProperty IsSelectionChangeCausedByTextInputProperty;
        private static readonly DependencyPropertyKey IsSelectionChangeCausedByTextInputPropertyKey;
        #endregion

        #region Initialization
        static TextBoxAutoComplete()
        {
            var metadata = new FrameworkPropertyMetadata(OnWordAutoCompleteSourceChanged);
            WordAutoCompleteSourceProperty = DependencyProperty.RegisterAttached("WordAutoCompleteSource", typeof(IEnumerable), typeof(TextBoxAutoComplete), metadata);

            metadata = new FrameworkPropertyMetadata(",;");
            WordAutoCompleteSeparatorsProperty = DependencyProperty.RegisterAttached("WordAutoCompleteSeparators", typeof(string), typeof(TextBoxAutoComplete), metadata);

            metadata = new FrameworkPropertyMetadata(OnWordAutoCompletePopupChanged);
            WordAutoCompletePopupProperty = DependencyProperty.RegisterAttached("WordAutoCompletePopup", typeof(Popup), typeof(TextBoxAutoComplete), metadata);

            metadata = new FrameworkPropertyMetadata();
            WordAutoCompleteWordsHostPropertyKey = DependencyProperty.RegisterAttachedReadOnly("WordAutoCompleteWordsHost", typeof(Selector), typeof(TextBoxAutoComplete), metadata);
            WordAutoCompleteWordsHostProperty = WordAutoCompleteWordsHostPropertyKey.DependencyProperty;

            metadata = new FrameworkPropertyMetadata((object)false);
            IsSelectionChangeCausedByTextInputPropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsSelectionChangeCausedByTextInput", typeof(bool), typeof(TextBoxAutoComplete), metadata);
            IsSelectionChangeCausedByTextInputProperty = IsSelectionChangeCausedByTextInputPropertyKey.DependencyProperty;
        }
        #endregion

        #region Dependency Properties Getters And Setters
        public static void SetWordAutoCompleteSource(TextBox element, IEnumerable value)
        {
            element.SetValue(WordAutoCompleteSourceProperty, value);
        }

        public static IEnumerable GetWordAutoCompleteSource(TextBox element)
        {
            return (IEnumerable)element.GetValue(WordAutoCompleteSourceProperty);
        }

        public static void SetWordAutoCompleteSeparators(TextBox element, string value)
        {
            element.SetValue(WordAutoCompleteSeparatorsProperty, value);
        }

        public static string GetWordAutoCompleteSeparators(TextBox element)
        {
            return (string)element.GetValue(WordAutoCompleteSeparatorsProperty);
        }

        public static void SetWordAutoCompletePopup(TextBox element, Popup value)
        {
            element.SetValue(WordAutoCompletePopupProperty, value);
        }

        public static Popup GetWordAutoCompletePopup(TextBox element)
        {
            return (Popup)element.GetValue(WordAutoCompletePopupProperty);
        }
        #endregion

        #region Dependency Properties Callbacks
        private static void OnWordAutoCompleteSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = (TextBox)d;
            SetWordsHostSourceAndHookupEvents(textBox, (IEnumerable)e.NewValue, GetWordAutoCompletePopup(textBox));
        }

        private static void OnWordAutoCompletePopupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = (TextBox)d;
            SetWordsHostSourceAndHookupEvents(textBox, GetWordAutoCompleteSource(textBox), (Popup)e.NewValue);
        }

        private static void SetWordsHostSourceAndHookupEvents(TextBox textBox, IEnumerable source, Popup popup)
        {
            if (source != null && popup != null)
            {
                //TODO: make sure we do this only this once, in case for some reason somebody re-sets one of the attached properties.
                textBox.PreviewKeyDown += new KeyEventHandler(TextBox_PreviewKeyDown);
                textBox.SelectionChanged += new RoutedEventHandler(TextBox_SelectionChanged);
                textBox.TextChanged += new TextChangedEventHandler(TextBox_TextChanged);

                Selector wordsHost = (Selector)popup.FindName("PART_WordsHost");
                if (wordsHost == null)
                    throw new InvalidOperationException("Can't find the PART_WordsHost element in the auto-complete popup control.");
                wordsHost.IsSynchronizedWithCurrentItem = true;
                wordsHost.ItemsSource = source;
                textBox.SetValue(WordAutoCompleteWordsHostPropertyKey, wordsHost);
            }
        }
        #endregion

        #region TextBox Event Handlers
        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Popup popup = GetWordAutoCompletePopup(textBox);
            char[] separators = GetWordAutoCompleteSeparators(textBox).ToCharArray();

            int previousSeparatorOffset;
            int nextSeparatorOffset;
            GetPreviousAndNextSeparatorOffsets(textBox.Text, separators, textBox.CaretIndex, out previousSeparatorOffset, out nextSeparatorOffset);

            string currentWord = textBox.Text.Substring(previousSeparatorOffset + 1, nextSeparatorOffset - (previousSeparatorOffset + 1));
            if (currentWord.Length > 0)
            {
                // Filter all the auto-complete suggestions with what the user is currently typing.
                Selector wordsHost = (Selector)textBox.GetValue(WordAutoCompleteWordsHostProperty);
                wordsHost.Items.Filter = o => GetTextSearchText(wordsHost, o).StartsWith(currentWord, StringComparison.CurrentCultureIgnoreCase);
                if (wordsHost.Items.IsEmpty)
                {
                    // Nothing matched... hide the popup.
                    popup.IsOpen = false;
                }
                else
                {
                    // Some matches have been found... show the popup, and select the first
                    // item if the previously selected item is now excluded.
                    if (popup.IsOpen)
                    {
                        if (wordsHost.Items.IsCurrentAfterLast || wordsHost.Items.IsCurrentBeforeFirst)
                        {
                            wordsHost.Items.MoveCurrentToFirst();
                        }
                    }
                    else
                    {
                        wordsHost.Items.MoveCurrentToFirst();
                        popup.IsOpen = true;
                    }
                }
            }
            else
            {
                popup.IsOpen = false;
            }

            textBox.SetValue(IsSelectionChangeCausedByTextInputPropertyKey, true);
        }

        private static void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (!(bool)textBox.GetValue(IsSelectionChangeCausedByTextInputProperty))
            {
                Popup popup = GetWordAutoCompletePopup(textBox);
                if (popup.IsOpen && (currentKey == Key.Tab || currentKey == Key.Enter || currentKey == Key.Escape))
                    popup.IsOpen = false;
            }

            // Reset to default (false).
            textBox.SetValue(IsSelectionChangeCausedByTextInputPropertyKey, false);
        }

        private static Key currentKey;
        private static int i = 0;
        private static void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                currentKey = e.Key;
                TextBox textBox = (TextBox)sender;
                Popup popup = GetWordAutoCompletePopup(textBox);

                if (popup.IsOpen)
                {
                    popup.LostMouseCapture += (o, args) =>
                        {
                            if (!popup.IsOpen)
                            {
                                return;
                            }
                            Debug.WriteLine(string.Format("entered {0}", (i++).ToString()));
                            Selector wordsHost = (Selector)textBox.GetValue(WordAutoCompleteWordsHostProperty);

                            if (wordsHost.Items.CurrentItem != null)
                            {
                                string text = textBox.Text;
                                string selection = wordsHost.Items.CurrentItem.ToString();
                                char[] separators = GetWordAutoCompleteSeparators(textBox).ToCharArray();

                                int previousSeparatorOffset;
                                int nextSeparatorOffset;
                                GetPreviousAndNextSeparatorOffsets(text, separators, textBox.CaretIndex, out previousSeparatorOffset, out nextSeparatorOffset);

                                string completedText = string.Empty;
                                if (previousSeparatorOffset > 0)
                                    completedText += text.Substring(0, previousSeparatorOffset + 1);
                                completedText += selection;
                                if (nextSeparatorOffset < text.Length)
                                    completedText += text.Substring(nextSeparatorOffset, text.Length - nextSeparatorOffset);
                                else
                                    completedText += separators[0];

                                textBox.Text = completedText;
                                textBox.CaretIndex = textBox.Text.Length;
                                popup.IsOpen = false;
                                popup.ReleaseMouseCapture();
                                popup.LostMouseCapture -= delegate(object sender1, MouseEventArgs eventArgs) { };
                                e.Handled = true;
                            }
                        };
                    if (e.Key == Key.Escape)
                    {
                        // Escape closes the popup.
                        popup.IsOpen = false;
                        e.Handled = true;
                    }
                    else if (e.Key == Key.Home || e.Key == Key.End)
                    {
                        // Jumping to the beginning or the end of the text closes the popup,
                        // but we don't set the event as handled because we want to still let
                        // the text box move the caret.
                        popup.IsOpen = false;
                    }
                    else
                    {
                        // Navigating or accepting auto-complete suggestions.
                        Selector wordsHost = (Selector)textBox.GetValue(WordAutoCompleteWordsHostProperty);
                        if (e.Key == Key.Up)
                        {
                            // Up or wrap around to the last suggestion.
                            wordsHost.Items.MoveCurrentToPrevious();
                            if (wordsHost.Items.IsCurrentBeforeFirst)
                                wordsHost.Items.MoveCurrentToLast();
                            e.Handled = true;
                        }
                        else if (e.Key == Key.Down)
                        {
                            // Down or wrap around to the first suggestion.
                            wordsHost.Items.MoveCurrentToNext();
                            if (wordsHost.Items.IsCurrentAfterLast)
                                wordsHost.Items.MoveCurrentToFirst();
                            e.Handled = true;
                        }
                        else if (e.Key == Key.Tab || e.Key == Key.Enter || e.Key == Key.Return)
                        {
                            // Accept the current suggestion... rebuild the text with the completed entry.
                            if (wordsHost.Items.CurrentItem != null)
                            {
                                string text = textBox.Text;
                                string selection = wordsHost.Items.CurrentItem.ToString();
                                char[] separators = GetWordAutoCompleteSeparators(textBox).ToCharArray();

                                int previousSeparatorOffset;
                                int nextSeparatorOffset;
                                GetPreviousAndNextSeparatorOffsets(text, separators, textBox.CaretIndex, out previousSeparatorOffset, out nextSeparatorOffset);

                                string completedText = string.Empty;
                                if (previousSeparatorOffset > 0)
                                    completedText += text.Substring(0, previousSeparatorOffset + 1);
                                completedText += selection;
                                if (nextSeparatorOffset < text.Length)
                                    completedText += text.Substring(nextSeparatorOffset, text.Length - nextSeparatorOffset);
                                else
                                    completedText += separators[0];

                                textBox.Text = completedText;
                                textBox.CaretIndex = textBox.Text.Length;

                                e.Handled = true;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                UIHelper.ShowMessage(exception.Message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
            }
        }

        private static void GetPreviousAndNextSeparatorOffsets(string text, char[] separators, int caretIndex, out int previousSeparatorOffset, out int nextSeparatorOffset)
        {
            previousSeparatorOffset = -1;
            if (caretIndex > 0)
                previousSeparatorOffset = text.LastIndexOfAny(separators, caretIndex - 1, caretIndex);

            nextSeparatorOffset = text.Length;
            if (caretIndex < text.Length)
            {
                nextSeparatorOffset = text.IndexOfAny(separators, caretIndex);
                if (nextSeparatorOffset < 0)
                    nextSeparatorOffset = text.Length;
            }
        }

        private static string GetTextSearchText(Selector wordsHost, object item)
        {
            //TODO: ideally we want something better than just using the ToString() representation of items.
            return item.ToString();
        }
        #endregion
    }
}
