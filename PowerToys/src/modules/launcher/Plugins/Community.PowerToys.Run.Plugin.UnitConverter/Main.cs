﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using ManagedCommon;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.UnitConverter
{
    public class Main : IPlugin, IPluginI18n, IContextMenu, IDisposable
    {
        public string Name => Properties.Resources.plugin_name;

        public string Description => Properties.Resources.plugin_description;

        private PluginInitContext _context;
        private static string _icon_path;
        private bool _disposed;

        public void Init(PluginInitContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(paramName: nameof(context));
            }

            _context = context;
            _context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(_context.API.GetCurrentTheme());
        }

        public List<Result> Query(Query query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(paramName: nameof(query));
            }

            // Parse
            ConvertModel convertModel = InputInterpreter.Parse(query);
            if (convertModel == null)
            {
                return new List<Result>();
            }

            // Convert
            return UnitHandler.Convert(convertModel)
                .Select(x => GetResult(x))
                .ToList();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1501:Statement should not be on a single line", Justification = "<挂起>")]
        private Result GetResult(UnitConversionResult result)
        {
            string hack = string.Format("{0}", result.QuantityType);
            if (hack == "Acceleration") { hack = "加速度"; }
            else if (hack == "Angle") { hack = "角度"; }
            else if (hack == "Area") { hack = "面积"; }
            else if (hack == "Duration") { hack = "时间"; }
            else if (hack == "Energy") { hack = "能量"; }
            else if (hack == "Information") { hack = "数据"; }
            else if (hack == "Length") { hack = "长度"; }
            else if (hack == "Mass") { hack = "质量"; }
            else if (hack == "Power") { hack = "功率"; }
            else if (hack == "Pressure") { hack = "压强"; }
            else if (hack == "Speed") { hack = "速度"; }
            else if (hack == "Temperature") { hack = "温度"; }
            else if (hack == "Volume") { hack = "体积"; }
            return new Result
            {
                ContextData = result,
                Title = string.Format("{0} {1}", result.ConvertedValue, result.UnitName),
                IcoPath = _icon_path,
                Score = 300,
                SubTitle = string.Format(Properties.Resources.copy_to_clipboard, hack),
                Action = c =>
                {
                    var ret = false;
                    var thread = new Thread(() =>
                    {
                        try
                        {
                            Clipboard.SetText(result.ConvertedValue.ToString());
                            ret = true;
                        }
                        catch (ExternalException)
                        {
                            MessageBox.Show(Properties.Resources.copy_failed);
                        }
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    thread.Join();
                    return ret;
                },
            };
        }

        private ContextMenuResult CreateContextMenuEntry(UnitConversionResult result)
        {
            return new ContextMenuResult
            {
                PluginName = Name,
                Title = Properties.Resources.context_menu_copy,
                Glyph = "\xE8C8",
                FontFamily = "Segoe MDL2 Assets",
                AcceleratorKey = Key.Enter,
                Action = _ =>
                {
                    bool ret = false;
                    var thread = new Thread(() =>
                    {
                        try
                        {
                            Clipboard.SetText(result.ConvertedValue.ToString());
                            ret = true;
                        }
                        catch (ExternalException)
                        {
                            MessageBox.Show(Properties.Resources.copy_failed);
                        }
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    thread.Join();
                    return ret;
                },
            };
        }

        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (!(selectedResult?.ContextData is UnitConversionResult))
            {
                return new List<ContextMenuResult>();
            }

            List<ContextMenuResult> contextResults = new List<ContextMenuResult>();
            UnitConversionResult result = selectedResult.ContextData as UnitConversionResult;
            contextResults.Add(CreateContextMenuEntry(result));

            return contextResults;
        }

        public string GetTranslatedPluginTitle()
        {
            return Properties.Resources.plugin_name;
        }

        public string GetTranslatedPluginDescription()
        {
            return Properties.Resources.plugin_description;
        }

        private void OnThemeChanged(Theme currentTheme, Theme newTheme)
        {
            UpdateIconPath(newTheme);
        }

        private static void UpdateIconPath(Theme theme)
        {
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                _icon_path = "Images/unitconverter.light.png";
            }
            else
            {
                _icon_path = "Images/unitconverter.dark.png";
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_context != null && _context.API != null)
                    {
                        _context.API.ThemeChanged -= OnThemeChanged;
                    }

                    _disposed = true;
                }
            }
        }
    }
}
