using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace LonestarShowdown.Converters
{
    /// <summary>
    ///     Abstract base class for value converters that convert between Boolean
    ///     values and other types.
    /// </summary>
    public abstract class BooleanConverter<T> : IValueConverter
    {
        protected BooleanConverter(T trueValue, T falseValue)
        {
            TrueValue = trueValue;
            FalseValue = falseValue;
        }

        public T TrueValue { get; set; }
        public T FalseValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(value, true) ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(value, TrueValue);
        }
    }

    /// <summary>
    ///     Converts true to false and false to true.
    /// </summary>
    [ValueConversion(typeof (bool), typeof (bool))]
    public sealed class NegatedBooleanConverter : BooleanConverter<bool>
    {
        public NegatedBooleanConverter()
            : base(false, true)
        {
        }
    }

    /// <summary>
    ///     Converts true and false to two different string values. By default,
    ///     both true and false are converted to null.
    /// </summary>
    [ValueConversion(typeof (bool), typeof (string))]
    public sealed class BooleanToStringConverter : BooleanConverter<string>
    {
        public BooleanToStringConverter()
            : base(null, null)
        {
        }
    }

    /// <summary>
    ///     Converts Boolean values to Visibility values. By default, true is
    ///     converted to Visible and false is converted to Collapsed.
    /// </summary>
    [ValueConversion(typeof (bool), typeof (Visibility))]
    public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter()
            : base(Visibility.Visible, Visibility.Collapsed)
        {
        }
    }

    /// <summary>
    ///     Converts Boolean values to Visibility values. By default, true is
    ///     converted to Visible and false is converted to Collapsed.
    /// </summary>
    [ValueConversion(typeof (bool), typeof (Visibility))]
    public sealed class InverseBooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public InverseBooleanToVisibilityConverter()
            : base(Visibility.Collapsed, Visibility.Visible)
        {
        }
    }

    /// <summary>
    ///     Converts Boolean values to double values. By default, true is
    ///     converted to 1 and false is converted to 0.
    /// </summary>
    [ValueConversion(typeof (bool), typeof (double))]
    public sealed class BooleanToDoubleConverter : BooleanConverter<double>
    {
        public BooleanToDoubleConverter()
            : base(1, 0)
        {
        }
    }

    /// <summary>
    ///     Converts Boolean values to Brush values. By default, both
    ///     true and false are converted to null.
    /// </summary>
    [ValueConversion(typeof (bool), typeof (Brush))]
    public sealed class BooleanToBrushConverter : BooleanConverter<Brush>
    {
        public BooleanToBrushConverter()
            : base(null, null)
        {
        }
    }

    /// <summary>
    ///     Converts Boolean values to Brush values. By default, both
    ///     true and false are converted to null.
    /// </summary>
    [ValueConversion(typeof (bool), typeof (GridLength))]
    public sealed class BooleanToGridSizeConverter : BooleanConverter<GridLength>
    {
        public BooleanToGridSizeConverter()
            : base(new GridLength(2, GridUnitType.Star), new GridLength(1, GridUnitType.Star))
        {
        }
    }
}