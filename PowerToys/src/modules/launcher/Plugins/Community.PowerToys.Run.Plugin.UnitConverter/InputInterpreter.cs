// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnitsNet;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.UnitConverter
{
    public static class InputInterpreter
    {
        private static string pattern = @"(?<=\d)(?![,.])(?=\D)|(?<=\D)(?<![,.])(?=\d)";

        public static string[] RegexSplitter(string[] split)
        {
            return Regex.Split(split[0], pattern);
        }

        /// <summary>
        /// Separates input like: "1ft in cm" to "1 ft in cm"
        /// </summary>
        public static void InputSpaceInserter(ref string[] split)
        {
            if (split.Length != 3)
            {
                return;
            }

            string[] parseInputWithoutSpace = Regex.Split(split[0], pattern);

            if (parseInputWithoutSpace.Length > 1)
            {
                string[] firstEntryRemoved = split.Skip(1).ToArray();
                string[] newSplit = new string[] { parseInputWithoutSpace[0], parseInputWithoutSpace[1] };

                split = newSplit.Concat(firstEntryRemoved).ToArray();
            }
        }

        /// <summary>
        /// Replaces a split input array with shorthand feet/inch notation (1', 1'2" etc) to 'x foot in cm'.
        /// </summary>
        public static void ShorthandFeetInchHandler(ref string[] split, CultureInfo culture)
        {
            if (!split[0].Contains('\'') && !split[0].Contains('\"'))
            {
                return;
            }

            // catches 1' || 1" || 1'2 || 1'2" in cm
            // by converting it to "x foot in cm"
            if (split.Length == 3)
            {
                string[] shortsplit = RegexSplitter(split);

                switch (shortsplit.Length)
                {
                    case 2:
                        // ex: 1' & 1"
                        if (shortsplit[1] == "\'")
                        {
                            string[] newInput = new string[] { shortsplit[0], "foot", split[1], split[2] };
                            split = newInput;
                        }
                        else if (shortsplit[1] == "\"")
                        {
                            string[] newInput = new string[] { shortsplit[0], "inch", split[1], split[2] };
                            split = newInput;
                        }

                        break;

                    case 3:
                    case 4:
                        // ex: 1'2 and 1'2"
                        if (shortsplit[1] == "\'")
                        {
                            bool isFeet = double.TryParse(shortsplit[0], NumberStyles.AllowDecimalPoint, culture, out double feet);
                            bool isInches = double.TryParse(shortsplit[2], NumberStyles.AllowDecimalPoint, culture, out double inches);

                            if (!isFeet || !isInches)
                            {
                                // atleast one could not be parsed correctly
                                break;
                            }

                            string convertedTotalInFeet = Length.FromFeetInches(feet, inches).Feet.ToString(culture);

                            string[] newInput = new string[] { convertedTotalInFeet, "foot", split[1], split[2] };
                            split = newInput;
                        }

                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Adds degree prefixes to degree units for shorthand notation. E.g. '10 c in fahrenheit' becomes '10 °c in DegreeFahrenheit'.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1501:Statement should not be on a single line", Justification = "<挂起>")]
        public static void DegreePrefixer(ref string[] split)
        {
            switch (split[1].ToLower(CultureInfo.CurrentCulture))
            {
                case "celsius":
                    split[1] = "DegreeCelsius";
                    break;

                case "fahrenheit":
                    split[1] = "DegreeFahrenheit";
                    break;

                case "c":
                    split[1] = "°c";
                    break;

                case "f":
                    split[1] = "°f";
                    break;

                default:
                    break;
            }

            switch (split[3].ToLower(CultureInfo.CurrentCulture))
            {
                case "celsius":
                    split[3] = "DegreeCelsius";
                    break;

                case "fahrenheit":
                    split[3] = "DegreeFahrenheit";
                    break;

                case "c":
                    split[3] = "°c";
                    break;

                case "f":
                    split[3] = "°f";
                    break;

                default:
                    break;
            }

            // [Chinese Translator]
            // 前面的单位
            // Acceleration 加速度    【自由，无互相冲突】（其他单位由长度、时间合成）
            split[1] = split[1].Replace("标准重力", "g");

            // Energy 能量    在时间前（马力时 冲突 时），在质量前（磅力 冲突）
            split[1] = split[1].Replace("焦耳", "J").Replace("焦", "J").Replace("卡路里", "cal").Replace("卡", "cal").Replace("英国热力单位", "BTU").Replace("英热", "BTU").Replace("电子伏特", "eV").Replace("磅力", "ft·lb").Replace("尔格", "erg").Replace("马力时", "hp·h");

            // Power 功率    【自由，无互相冲突】
            split[1] = split[1].Replace("瓦特", "W").Replace("瓦", "W").Replace("机械马力", "hp(I)").Replace("公制马力", "hp(M)").Replace("电气马力", "hp(E)").Replace("锅炉马力", "hp(S)").Replace("液压马力", "hp(H)").Replace("马力", "hp(M)").Replace("匹", "hp(M)");

            // Angle 角度    【自由，无互相冲突】
            if (split[1] == "度") { split[1] = "deg"; }
            if (split[1] == "分") { split[1] = "min"; }
            if (split[1] == "秒") { split[1] = "sec"; }
            split[1] = split[1].Replace("弧度", "rad").Replace("百分度", "gradian").Replace("圈", "r").Replace("转", "r");

            // Duration 时间    在角度后（分秒 分类讨论）
            split[1] = split[1].Replace("年", "yr").Replace("月", "mo").Replace("星期", "week").Replace("周", "week").Replace("日", "d").Replace("天", "d").Replace("小时", "h").Replace("时", "h").Replace("分钟", "min").Replace("分", "min").Replace("秒钟", "s").Replace("秒", "s");

            // Temperature 温度    在体积前（标 修补器 冲突 温标），在角度后（度 冲突 度 修补器）
            if (split[1].Contains("温标")) { split[1] = "°" + split[1].Replace("温标", string.Empty); }
            if (split[1].Contains("温度")) { split[1] = "°" + split[1].Replace("温度", string.Empty); }
            if (split[1].Contains('度')) { split[1] = "°" + split[1].Replace("度", string.Empty); }
            split[1] = split[1].Replace("开尔文", "K").Replace("开", "K").Replace("°K", "K").Replace("摄尔修斯", "C").Replace("摄氏", "C").Replace("℃", "°C").Replace("德利尔", "De").Replace("德氏", "De").Replace("华伦海特", "F").Replace("华氏", "F").Replace("℉", "°F").Replace("牛顿", "N").Replace("牛", "N").Replace("兰金", "R").Replace("兰氏", "R").Replace("列奥缪尔", "Ré").Replace("列氏", "Ré").Replace("罗默", "Rø").Replace("罗氏", "Rø").Replace("氏", string.Empty).Replace("°太阳", "T⊙");

            // Pressure 压强    在能量后（磅力 修补器 依赖），在能量、功率后（力 冲突 马力、马力时，帕斯卡 修补器 依赖 卡），在体积前（标准大气压 冲突 标 修补器）（质量 千克公斤吨+力 合成）（长度 毫米+汞柱 合成）（温度 牛+每+米+方 合成）
            split[1] = split[1].Replace("帕斯cal", "Pa").Replace("帕", "Pa").Replace("标准大气压", "atm").Replace("工程大气压", "at").Replace("大气压", "atm").Replace("巴", "bar").Replace("千ft·lb每", "kipf/").Replace("ft·lb每", "lb/").Replace("力", "f").Replace("达因", "dyn").Replace("托", "torr").Replace("汞柱", "Hg").Replace("英寸水柱", "wc");
            if (split[1].Contains("扬程")) { split[1] = split[1].Replace("扬程", string.Empty) + " of head"; }

            // Volume 体积    在面积、长度前（英亩英尺 板英尺 冲突）
            split[1] = split[1].Replace("公升", "L").Replace("立升", "L").Replace("升", "L").Replace("勺", "匙").Replace("式", "制").Replace("标", "制").Replace("英液加仑", "ImperialGallon").Replace("英液盎司", "ImperialOunce").Replace("美液加仑", "UsGallon").Replace("美液盎司", "UsOunce").Replace("美制汤匙", "UsTablespoon").Replace("澳制汤匙", "AuTablespoon").Replace("英制汤匙", "UkTablespoon").Replace("公制茶匙", "MetricTeaspoon").Replace("美制茶匙", "UsTeaspoon").Replace("公制杯", "MetricCup").Replace("美制市杯", "UsCustomaryCup").Replace("美制法杯", "UsLegalCup").Replace("桶油当量", "bbl").Replace("美制酒桶", "UsBeerBarrel").Replace("英制酒桶", "ImperialBeerBarrel").Replace("美制夸脱", "UsQuart").Replace("美制品脱", "UsPint").Replace("英亩英尺", "ac-ft").Replace("英制品脱", "pt").Replace("品脱", "pt").Replace("板英尺", "bf");

            // Area 面积    在长度前（平方海里 冲突）（其他单位由长度合成）
            split[1] = split[1].Replace("英亩", "ac").Replace("公顷", "ha").Replace("平方海里", "nmi²");

            // Length 长度    在时间后（天文单位 修补器 依赖 天）
            split[1] = split[1].Replace("米", "m").Replace("公里", "km").Replace("公尺", "m").Replace("公寸", "dm").Replace("公分", "cm").Replace("公厘", "mm").Replace("英里", "mi").Replace("码", "yd").Replace("英尺", "ft").Replace("英寸", "in").Replace("密耳", "mil").Replace("海里", "NM").Replace("英寻", "fathom").Replace("缆索", "shackle").Replace("缆", "shackle").Replace("缇", "twip").Replace("掌", "hand").Replace("d文单位", "au").Replace("派卡", "pc").Replace("光年", "ly").Replace("太阳半径", "R⊙").Replace("链", "ch");

            // Mass 质量    自由
            split[1] = split[1].Replace("克", "g").Replace("公斤", "kg").Replace("短吨", "short tn").Replace("长吨", "long tn").Replace("吨", "t").Replace("磅", "lb").Replace("盎司", "oz").Replace("斯勒格", "slug").Replace("石", "st").Replace("格令", "gr").Replace("太阳质量", "M⊙").Replace("地球质量", "em");

            // Information 数据    在速度前（节 冲突 字节），在能量、长度后（xx单位 冲突 位）
            split[1] = split[1].Replace("字节", "B").Replace("位", "b").Replace("比特", "b");

            // Speed 速度    在长度、时间后（修补器 处理依赖），在数据后（节 冲突 字节）
            split[1] = split[1].Replace("节", "kn").Replace("mi每h", "mph").Replace("mi/h", "mph");

            // 通用    最后
            if (split[1].Contains("平方")) { split[1] = split[1].Replace("平方", string.Empty) + "²"; }
            if (split[1].Contains("二次方")) { split[1] = split[1].Replace("二次方", string.Empty) + "²"; }
            if (split[1].Contains("立方")) { split[1] = split[1].Replace("立方", string.Empty) + "³"; }
            if (split[1].Contains("三次方")) { split[1] = split[1].Replace("三次方", string.Empty) + "³"; }
            if (split[1].Contains('方')) { split[1] = split[1].Replace("方", string.Empty) + "²"; }
            split[1] = split[1].Replace("艾", "E").Replace("拍", "P").Replace("太", "T").Replace("吉", "G").Replace("兆", "M").Replace("千", "k").Replace("百", "h").Replace("分", "d").Replace("厘", "c").Replace("毫", "m").Replace("微", "μ").Replace("纳", "n");
            split[1] = split[1].Replace("的", string.Empty).Replace("个", string.Empty).Replace("每", "/");

            // 后面的单位
            // Acceleration 加速度    【自由，无互相冲突】（其他单位由长度、时间合成）
            split[3] = split[3].Replace("标准重力", "g");

            // Energy 能量    在时间前（马力时 冲突 时），在质量前（磅力 冲突）
            split[3] = split[3].Replace("焦耳", "J").Replace("焦", "J").Replace("卡路里", "cal").Replace("卡", "cal").Replace("英国热力单位", "BTU").Replace("英热", "BTU").Replace("电子伏特", "eV").Replace("磅力", "ft·lb").Replace("尔格", "erg").Replace("马力时", "hp·h");

            // Power 功率    【自由，无互相冲突】
            split[3] = split[3].Replace("瓦特", "W").Replace("瓦", "W").Replace("机械马力", "hp(I)").Replace("公制马力", "hp(M)").Replace("电气马力", "hp(E)").Replace("锅炉马力", "hp(S)").Replace("液压马力", "hp(H)").Replace("马力", "hp(M)").Replace("匹", "hp(M)");

            // Angle 角度    【自由，无互相冲突】
            if (split[3] == "度") { split[3] = "deg"; }
            if (split[3] == "分") { split[3] = "min"; }
            if (split[3] == "秒") { split[3] = "sec"; }
            split[3] = split[3].Replace("弧度", "rad").Replace("百分度", "gradian").Replace("圈", "r").Replace("转", "r");

            // Duration 时间    在角度后（分秒 分类讨论）
            split[3] = split[3].Replace("年", "yr").Replace("月", "mo").Replace("星期", "week").Replace("周", "week").Replace("日", "d").Replace("天", "d").Replace("小时", "h").Replace("时", "h").Replace("分钟", "min").Replace("分", "min").Replace("秒钟", "s").Replace("秒", "s");

            // Temperature 温度    在体积前（标 修补器 冲突 温标），在角度后（度 冲突 度 修补器）
            if (split[3].Contains("温标")) { split[3] = "°" + split[3].Replace("温标", string.Empty); }
            if (split[3].Contains("温度")) { split[3] = "°" + split[3].Replace("温度", string.Empty); }
            if (split[3].Contains('度')) { split[3] = "°" + split[3].Replace("度", string.Empty); }
            split[3] = split[3].Replace("开尔文", "K").Replace("开", "K").Replace("°K", "K").Replace("摄尔修斯", "C").Replace("摄氏", "C").Replace("℃", "°C").Replace("德利尔", "De").Replace("德氏", "De").Replace("华伦海特", "F").Replace("华氏", "F").Replace("℉", "°F").Replace("牛顿", "N").Replace("牛", "N").Replace("兰金", "R").Replace("兰氏", "R").Replace("列奥缪尔", "Ré").Replace("列氏", "Ré").Replace("罗默", "Rø").Replace("罗氏", "Rø").Replace("氏", string.Empty).Replace("°太阳", "T⊙");

            // Pressure 压强    在能量后（磅力 修补器 依赖），在能量、功率后（力 冲突 马力、马力时，帕斯卡 修补器 依赖 卡），在体积前（标准大气压 冲突 标 修补器）（质量 千克公斤吨+力 合成）（长度 毫米+汞柱 合成）（温度 牛+每+米+方 合成）
            split[3] = split[3].Replace("帕斯cal", "Pa").Replace("帕", "Pa").Replace("标准大气压", "atm").Replace("工程大气压", "at").Replace("大气压", "atm").Replace("巴", "bar").Replace("千ft·lb每", "kipf/").Replace("ft·lb每", "lb/").Replace("力", "f").Replace("达因", "dyn").Replace("托", "torr").Replace("汞柱", "Hg").Replace("英寸水柱", "wc");
            if (split[3].Contains("扬程")) { split[3] = split[3].Replace("扬程", string.Empty) + " of head"; }

            // Volume 体积    在面积、长度前（英亩英尺 板英尺 冲突）
            split[3] = split[3].Replace("公升", "L").Replace("立升", "L").Replace("升", "L").Replace("勺", "匙").Replace("式", "制").Replace("标", "制").Replace("英液加仑", "ImperialGallon").Replace("英液盎司", "ImperialOunce").Replace("美液加仑", "UsGallon").Replace("美液盎司", "UsOunce").Replace("美制汤匙", "UsTablespoon").Replace("澳制汤匙", "AuTablespoon").Replace("英制汤匙", "UkTablespoon").Replace("公制茶匙", "MetricTeaspoon").Replace("美制茶匙", "UsTeaspoon").Replace("公制杯", "MetricCup").Replace("美制市杯", "UsCustomaryCup").Replace("美制法杯", "UsLegalCup").Replace("桶油当量", "bbl").Replace("美制酒桶", "UsBeerBarrel").Replace("英制酒桶", "ImperialBeerBarrel").Replace("美制夸脱", "UsQuart").Replace("美制品脱", "UsPint").Replace("英亩英尺", "ac-ft").Replace("英制品脱", "pt").Replace("品脱", "pt").Replace("板英尺", "bf");

            // Area 面积    在长度前（平方海里 冲突）（其他单位由长度合成）
            split[3] = split[3].Replace("英亩", "ac").Replace("公顷", "ha").Replace("平方海里", "nmi²");

            // Length 长度    在时间后（天文单位 修补器 依赖 天）
            split[3] = split[3].Replace("米", "m").Replace("公里", "km").Replace("公尺", "m").Replace("公寸", "dm").Replace("公分", "cm").Replace("公厘", "mm").Replace("英里", "mi").Replace("码", "yd").Replace("英尺", "ft").Replace("英寸", "in").Replace("密耳", "mil").Replace("海里", "NM").Replace("英寻", "fathom").Replace("缆索", "shackle").Replace("缆", "shackle").Replace("缇", "twip").Replace("掌", "hand").Replace("d文单位", "au").Replace("派卡", "pc").Replace("光年", "ly").Replace("太阳半径", "R⊙").Replace("链", "ch");

            // Mass 质量    自由
            split[3] = split[3].Replace("克", "g").Replace("公斤", "kg").Replace("短吨", "short tn").Replace("长吨", "long tn").Replace("吨", "t").Replace("磅", "lb").Replace("盎司", "oz").Replace("斯勒格", "slug").Replace("石", "st").Replace("格令", "gr").Replace("太阳质量", "M⊙").Replace("地球质量", "em");

            // Information 数据    在速度前（节 冲突 字节），在能量、长度后（xx单位 冲突 位）
            split[3] = split[3].Replace("字节", "B").Replace("位", "b").Replace("比特", "b");

            // Speed 速度    在长度、时间后（修补器 处理依赖），在数据后（节 冲突 字节）
            split[3] = split[3].Replace("节", "kn").Replace("mi每h", "mph").Replace("mi/h", "mph");

            // 通用    最后
            if (split[3].Contains("平方")) { split[3] = split[3].Replace("平方", string.Empty) + "²"; }
            if (split[3].Contains("二次方")) { split[3] = split[3].Replace("二次方", string.Empty) + "²"; }
            if (split[3].Contains("立方")) { split[3] = split[3].Replace("立方", string.Empty) + "³"; }
            if (split[3].Contains("三次方")) { split[3] = split[3].Replace("三次方", string.Empty) + "³"; }
            if (split[3].Contains('方')) { split[3] = split[3].Replace("方", string.Empty) + "²"; }
            split[3] = split[3].Replace("艾", "E").Replace("拍", "P").Replace("太", "T").Replace("吉", "G").Replace("兆", "M").Replace("千", "k").Replace("百", "h").Replace("分", "d").Replace("厘", "c").Replace("毫", "m").Replace("微", "μ").Replace("纳", "n");
            split[3] = split[3].Replace("的", string.Empty).Replace("个", string.Empty).Replace("每", "/");

            // [For Debug]

            // using System.Windows;
            // System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo("msg.exe");
            // info.Arguments = "LittleFox /time:1 " + split[1] + " -> " + split[3];
            // info.CreateNoWindow = true;
            // System.Diagnostics.Process proc;
            // proc = System.Diagnostics.Process.Start(info);
        }

        /// <summary>
        /// The plural form "feet" is not recognized by UniteNets. Replace it with "ft".
        /// </summary>
        public static void FeetToFt(ref string[] split)
        {
            if (split[1].ToLowerInvariant() == "feet")
            {
                split[1] = "ft";
            }

            if (split[3].ToLowerInvariant() == "feet")
            {
                split[3] = "ft";
            }
        }

        /// <summary>
        /// Converts spelling "metre" to "meter", also for centimetre and other variants
        /// </summary>
        public static void MetreToMeter(ref string[] split)
        {
            split[1] = split[1].Replace("metre", "meter", System.StringComparison.CurrentCultureIgnoreCase);
            split[3] = split[3].Replace("metre", "meter", System.StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Choose "UsGallon" or "ImperialGallon" according to current culture when the input contains "gal" or "gallon".
        /// </summary>
        public static void GallonHandler(ref string[] split, CultureInfo culture)
        {
            HashSet<string> britishCultureNames = new HashSet<string>() { "en-AI", "en-VG", "en-GB", "en-KY", "en-MS", "en-AG", "en-DM", "en-GD", "en-KN", "en-LC", "en-VC", "en-IE", "en-GY", "en-AE" };
            if (split[1].ToLowerInvariant() == "gal" || split[1].ToLowerInvariant() == "gallon")
            {
                if (britishCultureNames.Contains(culture.Name))
                {
                    split[1] = "ImperialGallon";
                }
                else
                {
                    split[1] = "UsGallon";
                }
            }

            if (split[3].ToLowerInvariant() == "gal" || split[3].ToLowerInvariant() == "gallon")
            {
                if (britishCultureNames.Contains(culture.Name))
                {
                    split[3] = "ImperialGallon";
                }
                else
                {
                    split[3] = "UsGallon";
                }
            }
        }

        public static ConvertModel Parse(Query query)
        {
            string[] split = query.Search.Split(' ');

            InputInterpreter.ShorthandFeetInchHandler(ref split, CultureInfo.CurrentCulture);
            InputInterpreter.InputSpaceInserter(ref split);

            if (split.Length != 4)
            {
                // deny any other queries than:
                // 10 ft in cm
                // 10 ft to cm
                return null;
            }

            InputInterpreter.DegreePrefixer(ref split);
            InputInterpreter.MetreToMeter(ref split);
            InputInterpreter.FeetToFt(ref split);
            InputInterpreter.GallonHandler(ref split, CultureInfo.CurrentCulture);
            if (!double.TryParse(split[0], out double value))
            {
                return null;
            }

            return new ConvertModel()
            {
                Value = value,
                FromUnit = split[1],
                ToUnit = split[3],
            };
        }
    }
}
