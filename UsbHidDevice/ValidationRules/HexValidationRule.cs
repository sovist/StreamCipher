using System.Globalization;
using System.Windows.Controls;

namespace UsbHidDevice.ValidationRules
{
    class HexValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {

            return new ValidationResult(false, "sdfsdgff");
        }
    }
}
