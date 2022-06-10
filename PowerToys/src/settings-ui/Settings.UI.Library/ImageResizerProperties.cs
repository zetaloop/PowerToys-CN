// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.PowerToys.Settings.UI.Library
{
    public class ImageResizerProperties
    {
        public ImageResizerProperties()
        {
            ImageresizerSelectedSizeIndex = new IntProperty(0);
            ImageresizerShrinkOnly = new BoolProperty(false);
            ImageresizerReplace = new BoolProperty(false);
            ImageresizerIgnoreOrientation = new BoolProperty(true);
            ImageresizerJpegQualityLevel = new IntProperty(90);
            ImageresizerPngInterlaceOption = new IntProperty();
            ImageresizerTiffCompressOption = new IntProperty();
            ImageresizerFileName = new StringProperty("%1 (%2)");
            ImageresizerSizes = new ImageResizerSizes();
            ImageresizerKeepDateModified = new BoolProperty();
            ImageresizerFallbackEncoder = new StringProperty(new System.Guid("19e4a5aa-5662-4fc5-a0c0-1758028e1057").ToString());
            ImageresizerCustomSize = new ImageResizerCustomSizeProperty(new ImageSize(4, "custom", ResizeFit.缩放, 1024, 640, ResizeUnit.像素));
        }

        public ImageResizerProperties(Func<string, string> resourceLoader)
            : this()
        {
            if (resourceLoader == null)
            {
                throw new ArgumentNullException(nameof(resourceLoader), "Resource loader is null");
            }

            ImageresizerSizes = new ImageResizerSizes(new ObservableCollection<ImageSize>()
            {
                new ImageSize(0, resourceLoader("ImageResizer_DefaultSize_Small"), ResizeFit.缩放, 854, 480, ResizeUnit.像素),
                new ImageSize(1, resourceLoader("ImageResizer_DefaultSize_Medium"), ResizeFit.缩放, 1366, 768, ResizeUnit.像素),
                new ImageSize(2, resourceLoader("ImageResizer_DefaultSize_Large"), ResizeFit.缩放, 1920, 1080, ResizeUnit.像素),
                new ImageSize(3, resourceLoader("ImageResizer_DefaultSize_Phone"), ResizeFit.缩放, 320, 568, ResizeUnit.像素),
            });
        }

        [JsonPropertyName("imageresizer_selectedSizeIndex")]
        public IntProperty ImageresizerSelectedSizeIndex { get; set; }

        [JsonPropertyName("imageresizer_shrinkOnly")]
        public BoolProperty ImageresizerShrinkOnly { get; set; }

        [JsonPropertyName("imageresizer_replace")]
        public BoolProperty ImageresizerReplace { get; set; }

        [JsonPropertyName("imageresizer_ignoreOrientation")]
        public BoolProperty ImageresizerIgnoreOrientation { get; set; }

        [JsonPropertyName("imageresizer_jpegQualityLevel")]
        public IntProperty ImageresizerJpegQualityLevel { get; set; }

        [JsonPropertyName("imageresizer_pngInterlaceOption")]
        public IntProperty ImageresizerPngInterlaceOption { get; set; }

        [JsonPropertyName("imageresizer_tiffCompressOption")]
        public IntProperty ImageresizerTiffCompressOption { get; set; }

        [JsonPropertyName("imageresizer_fileName")]
        public StringProperty ImageresizerFileName { get; set; }

        [JsonPropertyName("imageresizer_sizes")]
        public ImageResizerSizes ImageresizerSizes { get; set; }

        [JsonPropertyName("imageresizer_keepDateModified")]
        public BoolProperty ImageresizerKeepDateModified { get; set; }

        [JsonPropertyName("imageresizer_fallbackEncoder")]
        public StringProperty ImageresizerFallbackEncoder { get; set; }

        [JsonPropertyName("imageresizer_customSize")]
        public ImageResizerCustomSizeProperty ImageresizerCustomSize { get; set; }

        public string ToJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    public enum ResizeFit
    {
        填充 = 0,
        缩放 = 1,
        拉伸 = 2,
    }

    public enum ResizeUnit
    {
        厘米 = 0,
        英寸 = 1,
        百分比 = 2,
        像素 = 3,
    }

    public enum PngInterlaceOption
    {
        Default = 0,
        On = 1,
        Off = 2,
    }

    public enum TiffCompressOption
    {
        Default = 0,
        None = 1,
        Ccitt3 = 2,
        Ccitt4 = 3,
        Lzw = 4,
        Rle = 5,
        Zip = 6,
    }
}
