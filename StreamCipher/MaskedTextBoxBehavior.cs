using System.Text;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace StreamCipher.Behaviors
{
    public class MaskedTextBoxBehavior : Behavior<TextBox>
    {
        public string Mask { get; set; }
        public char InputSimbol { get; set; }
        private char _separateSimbol;
        protected override void OnAttached()
        {
            initSeparateSimbol();
            AssociatedObject.TextChanged += associatedObjectOnTextChanged;
            //AssociatedObject.Text = Mask;
            base.OnAttached();
        }

        private void initSeparateSimbol()
        {
            var separete = Mask.Replace(InputSimbol.ToString(), "");
            if (separete.Length > 0)
                _separateSimbol = separete[0];
        }

        private void associatedObjectOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            AssociatedObject.TextChanged -= associatedObjectOnTextChanged;

            var textBox = sender as TextBox;
            if (textBox != null)
            {
                var inputText = textBox.Text.Replace(_separateSimbol.ToString(), "").Replace(InputSimbol.ToString(), "");
                var maskedText = new StringBuilder(Mask);
                int caretIndex = 0;
                foreach (char c in inputText)
                    for (; caretIndex < maskedText.Length; caretIndex++)
                        if (maskedText[caretIndex] == InputSimbol)
                        {
                            maskedText[caretIndex] = c;
                            caretIndex++;
                            break;
                        }
                textBox.Text = maskedText.ToString();
                textBox.CaretIndex = caretIndex;
            }

            AssociatedObject.TextChanged += associatedObjectOnTextChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.TextChanged += associatedObjectOnTextChanged;
            base.OnDetaching();
        }
    }
}
