// Copyright (c) Brice Lambson
// The Brice Lambson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.  Code forked from Brice Lambson's https://github.com/bricelam/ImageResizer/

using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace ImageResizer.Models
{
    [TypeConverter(typeof(ResizeFitTranslator))]
    public enum ResizeFit
    {
        [Description("填充")]
        Fill,
        [Description("缩放")]
        Fit,
        [Description("拉伸")]
        Stretch,
    }

#pragma warning disable SA1649 // File name should match first type name
    public class ResizeFitTranslator : EnumConverter
#pragma warning restore SA1649 // File name should match first type name
    {
        public ResizeFitTranslator(Type type)
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
