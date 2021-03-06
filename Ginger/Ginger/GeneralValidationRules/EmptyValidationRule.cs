﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Amdocs.Ginger.ValidationRules
{
    public class EmptyValidationRule : ValidationRule
    {
        string mMessage = "Value cannot be empty";  //deafult message
        public EmptyValidationRule()
        {

        }

        public EmptyValidationRule(string message)
        {
            mMessage = message;
        }


        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new ValidationResult(false, mMessage);
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
