// Copyright (c) Brice Lambson
// The Brice Lambson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.  Code forked from Brice Lambson's https://github.com/bricelam/ImageResizer/

using System.Windows.Controls;

namespace ImageResizer.Views
{
    public partial class InputPage : UserControl
    {
        public InputPage()
        {
            InitializeComponent();

            // 汉化默认选项，在列表加载完之后
            FitTranslator.Loaded += (s, e) =>
            {
                FitTranslator.SelectedIndex = 1; // 默认选中“缩放”
            };

            // 汉化默认选项，在列表加载完之后
            UnitTranslator.Loaded += (s, e) =>
            {
                UnitTranslator.SelectedIndex = 3; // 默认选中“像素”
            };
        }
    }
}
