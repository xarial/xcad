//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Microsoft.Win32;
using System;
using System.ComponentModel;
using Xarial.XCad.Base;
using Xarial.XCad.Reflection;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal class RegistrationHelper
    {
        private const string ADDIN_REG_KEY_TEMPLATE = @"SOFTWARE\SolidWorks\Addins\{{{0}}}";
        private const string ADDIN_STARTUP_REG_KEY_TEMPLATE = @"Software\SolidWorks\AddInsStartup\{{{0}}}";
        private const string DESCRIPTION_REG_KEY_NAME = "Description";
        private const string TITLE_REG_KEY_NAME = "Title";

        private readonly IXLogger m_Logger;

        internal RegistrationHelper(IXLogger logger)
        {
            m_Logger = logger;
        }

        internal bool Register(Type type)
        {
            try
            {
                m_Logger.Log($"Registering add-in");

                RegisterAddIn(type);

                return true;
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                return false;
            }
        }

        internal bool Unregister(Type type)
        {
            try
            {
                m_Logger.Log($"Unregistering add-in");

                UnregisterAddIn(type);

                return true;
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                return false;
            }
        }

        internal string GetTitle(Type type) 
        {
            string title = "";

            type.TryGetAttribute<DisplayNameAttribute>(a => title = a.DisplayName);

            if (string.IsNullOrEmpty(title))
            {
                title = type.Name;
            }

            return title;
        }

        private void RegisterAddIn(Type type)
        {
            string desc = "";
            bool loadAtStartup = true;

            type.TryGetAttribute<DescriptionAttribute>(a => desc = a.Description);

            var title = GetTitle(type);

            var addInKey = Registry.LocalMachine.CreateSubKey(
                string.Format(ADDIN_REG_KEY_TEMPLATE, type.GUID));
            addInKey.SetValue(null, 0);

            m_Logger.Log($"Created HKLM\\{addInKey}");

            addInKey.SetValue(DESCRIPTION_REG_KEY_NAME, desc);
            addInKey.SetValue(TITLE_REG_KEY_NAME, title);

            var addInStartupKey = Registry.CurrentUser.CreateSubKey(
                string.Format(ADDIN_STARTUP_REG_KEY_TEMPLATE, type.GUID));
            addInStartupKey.SetValue(null, Convert.ToInt32(loadAtStartup), RegistryValueKind.DWord);

            m_Logger.Log($"Created HKCU\\{addInStartupKey}");
        }

        private void UnregisterAddIn(Type type)
        {
            var addInKey = string.Format(ADDIN_REG_KEY_TEMPLATE, type.GUID);
            var addInStartupKey = string.Format(ADDIN_STARTUP_REG_KEY_TEMPLATE, type.GUID);

            if (Registry.LocalMachine.OpenSubKey(addInKey, false) != null)
            {
                Registry.LocalMachine.DeleteSubKey(addInKey);
                m_Logger.Log($"Deleting: HKLM\\{addInKey}");
            }

            if (Registry.CurrentUser.OpenSubKey(addInStartupKey, false) != null)
            {
                Registry.CurrentUser.DeleteSubKey(addInStartupKey);
                m_Logger.Log($"Deleting: HKCU\\{addInStartupKey}");
            }
        }
    }
}