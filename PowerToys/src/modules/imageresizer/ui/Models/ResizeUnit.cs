// Copyright (c) Brice Lambson
// The Brice Lambson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.  Code forked from Brice Lambson's https://github.com/bricelam/ImageResizer/

namespace ImageResizer.Models
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;

    [TypeConverter(typeof(ResizeUnitTranslator))]
    public enum ResizeUnit
    {
        [Description("厘米")]
        Centimeter,
        [Description("英寸")]
        Inch,
        [Description("百分比")]
        Percent,
        [Description("像素")]
        Pixel,
    }

#pragma warning disable SA1649 // File name should match first type name
    public class ResizeUnitTranslator : EnumConverter
#pragma warning restore SA1649 // File name should match first type name
    {
        public ResizeUnitTranslator(Type type)
            : base(type)
        {
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value != null)
                {
                    FieldInfo fi = value.GetType().GetField(value.ToString());
                    if (fi != null)
                    {
                        var attributes =
                            (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                        return ((attributes.Length > 0) && (!string.IsNullOrEmpty(attributes[0].Description)))
                            ? attributes[0].Description
                            : value.ToString();
                    }
                }

                return string.Empty;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
