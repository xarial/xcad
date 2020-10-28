//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documents;
using Xarial.XCad.Extensions;
using Xarial.XCad.Extensions.Attributes;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Features.CustomFeature.Delegates;
using Xarial.XCad.SolidWorks.Base;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature;
using Xarial.XCad.SolidWorks.Features.CustomFeature.Toolkit;
using Xarial.XCad.SolidWorks.UI;
using Xarial.XCad.SolidWorks.UI.Commands;
using Xarial.XCad.SolidWorks.UI.Commands.Exceptions;
using Xarial.XCad.SolidWorks.UI.Commands.Toolkit.Structures;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit;
using Xarial.XCad.UI;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.TaskPane;
using Xarial.XCad.Utils.Diagnostics;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks
{
    /// <inheritdoc/>
    [ComVisible(true)]
    public abstract class SwAddInEx : IXExtension, ISwAddin, IXServiceConsumer, IDisposable
    {
        #region Registration

        private static RegistrationHelper m_RegHelper;

        /// <summary>
        /// COM Registration entry function
        /// </summary>
        /// <param name="t">Type</param>
        [ComRegisterFunction]
        public static void RegisterFunction(Type t)
        {
            if (t.TryGetAttribute<SkipRegistrationAttribute>()?.Skip != true)
            {
                GetRegistrationHelper(t).Register(t);
            }
        }

        /// <summary>
        /// COM Unregistration entry function
        /// </summary>
        /// <param name="t">Type</param>
        [ComUnregisterFunction]
        public static void UnregisterFunction(Type t)
        {
            if (t.TryGetAttribute<SkipRegistrationAttribute>()?.Skip != true)
            {
                GetRegistrationHelper(t).Unregister(t);
            }
        }

        private static RegistrationHelper GetRegistrationHelper(Type moduleType)
        {
            return m_RegHelper ?? (m_RegHelper = new RegistrationHelper(new TraceLogger(moduleType.FullName)));
        }

        #endregion Registration

        IXApplication IXExtension.Application => Application;
        IXCommandManager IXExtension.CommandManager => CommandManager;
        IXCustomPanel<TControl> IXExtension.CreateDocumentTab<TControl>(IXDocument doc)
            => CreateDocumentTab<TControl>((SwDocument)doc);
        IXPopupWindow<TWindow> IXExtension.CreatePopupWindow<TWindow>()
            => CreatePopupWindow<TWindow>();
        IXTaskPane<TControl> IXExtension.CreateTaskPane<TControl>(TaskPaneSpec spec)
            => CreateTaskPane<TControl>(spec);
        IXCustomPanel<TControl> IXExtension.CreateFeatureManagerTab<TControl>(IXDocument doc) 
            => CreateFeatureManagerTab<TControl>((SwDocument)doc);

        public SwApplication Application { get; private set; }

        public SwCommandManager CommandManager { get; private set; }

        /// <summary>
        /// Add-ins cookie (id)
        /// </summary>
        protected int AddInId { get; private set; }

        public IXLogger Logger { get; }

        private readonly List<IDisposable> m_Disposables;

        private readonly IServiceProvider m_SvcProvider;

        public SwAddInEx()
        {
            var addInType = this.GetType();
            var title = GetRegistrationHelper(addInType).GetTitle(addInType);

            var svcColl = new ServiceCollection();

            svcColl.AddOrReplace<IXLogger>(() => new TraceLogger($"XCad.AddIn.{title}"));

            ConfigureServices(svcColl);

            m_SvcProvider = svcColl.CreateProvider();

            Logger = m_SvcProvider.GetService<IXLogger>();
            
            m_Disposables = new List<IDisposable>();
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ConnectToSW(object ThisSW, int cookie)
        {
            Logger.Log("Loading add-in");

            try
            {
                var app = ThisSW as ISldWorks;
                AddInId = cookie;

                if (app.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2015))
                {
                    app.SetAddinCallbackInfo2(0, this, AddInId);
                }
                else 
                {
                    app.SetAddinCallbackInfo(0, this, AddInId);
                }

                Application = new SwApplication(app, Logger);

                SwMacroFeatureDefinition.Application = Application;

                CommandManager = new SwCommandManager(Application, AddInId, Logger, this.GetType().GUID);

                OnConnect();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool DisconnectFromSW()
        {
            Logger.Log("Unloading add-in");

            try
            {
                OnDisconnect();
                Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Command click callback
        /// </summary>
        /// <param name="cmdId">Command tag</param>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnCommandClick(string cmdId)
        {
            CommandManager.HandleCommandClick(cmdId);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int OnCommandEnable(string cmdId)
        {
            return CommandManager.HandleCommandEnable(cmdId);
        }

        public virtual void OnConnect()
        {
        }

        public virtual void OnDisconnect()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CommandManager.Dispose();
                Application.Documents.Dispose();
                Application.Dispose();

                foreach (var dispCtrl in m_Disposables) 
                {
                    dispCtrl.Dispose();
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        IXPropertyPage<TData> IXExtension.CreatePage<TData>()
        {
            return CreatePropertyManagerPage<TData>(typeof(TData));
        }

        public SwPropertyManagerPage<TData> CreatePage<TData>()
        {
            return CreatePropertyManagerPage<TData>(typeof(TData));
        }

        public SwPropertyManagerPage<TData> CreatePage<TData, THandler>()
            where THandler : SwPropertyManagerPageHandler, new()
        {
            return CreatePropertyManagerPage<TData>(typeof(THandler));
        }

        private SwPropertyManagerPage<TData> CreatePropertyManagerPage<TData>(Type handlerType)
        {
            var page = new SwPropertyManagerPage<TData>(Application, Logger, handlerType);
            m_Disposables.Add(page);
            return page;
        }

        public SwModelViewTab<TControl> CreateDocumentTab<TControl>(Documents.SwDocument doc)
        {
            var mdlViewMgr = doc.Model.ModelViewManager;

            return CustomControlHelper.HostControl<TControl, SwModelViewTab<TControl>>(
                (c, h, t, _) =>
                {
                    if (mdlViewMgr.DisplayWindowFromHandlex64(t, h.Handle.ToInt64(), true))
                    {
                        return new SwModelViewTab<TControl>(c, t, mdlViewMgr, doc);
                    }
                    else
                    {
                        throw new NetControlHostException(h.Handle);
                    }
                },
                (p, t, _) =>
                {
                    var ctrl = (TControl)mdlViewMgr.AddControl3(t, p, "", true);
                    
                    if (ctrl == null)
                    {
                        throw new ComControlHostException(p);
                    }

                    return new SwModelViewTab<TControl>(ctrl, t, mdlViewMgr, doc);
                });
        }

        public SwPopupWindow<TWindow> CreatePopupWindow<TWindow>() 
        {
            var parent = (IntPtr)Application.Sw.IFrameObject().GetHWnd();

            if (typeof(System.Windows.Window).IsAssignableFrom(typeof(TWindow)))
            {
                return new SwPopupWpfWindow<TWindow>((TWindow)Activator.CreateInstance(typeof(TWindow)), parent);
            }
            else if (typeof(System.Windows.Forms.Form).IsAssignableFrom(typeof(TWindow)))
            {
                return new SwPopupWinForm<TWindow>((TWindow)Activator.CreateInstance(typeof(TWindow)), parent);
            }
            else
            {
                throw new NotSupportedException($"Only {typeof(System.Windows.Forms.Form).FullName} or {typeof(System.Windows.Window).FullName} are supported");
            }
        }

        public SwTaskPane<TControl> CreateTaskPane<TControl>() => CreateTaskPane<TControl>(new TaskPaneSpec());

        public SwTaskPane<TControl> CreateTaskPane<TControl>(TaskPaneSpec spec) 
        {
            if (spec == null)
            {
                spec = new TaskPaneSpec();
            }

            ITaskpaneView CreateTaskPaneView(IconsConverter iconConv, Image icon, string title) 
            {
                if (icon == null) 
                {
                    if (spec.Icon != null)
                    {
                        icon = IconsConverter.FromXImage(spec.Icon);
                    }
                }

                if (string.IsNullOrEmpty(title)) 
                {
                    if (spec != null)
                    {
                        title = spec.Title;
                    }
                }
                
                if (Application.Sw.SupportsHighResIcons(CompatibilityUtils.HighResIconsScope_e.TaskPane))
                {
                    string[] taskPaneIconImages = null;

                    if (icon != null)
                    {
                        taskPaneIconImages = iconConv.ConvertIcon(new TaskPaneHighResIcon(icon));
                    }

                    return Application.Sw.CreateTaskpaneView3(taskPaneIconImages, title);
                }
                else
                {
                    var taskPaneIconImage = "";

                    if (icon != null)
                    {
                        taskPaneIconImage = iconConv.ConvertIcon(new TaskPaneIcon(icon)).First();
                    }

                    return Application.Sw.CreateTaskpaneView2(taskPaneIconImage, title);
                }
            }

            using (var iconConv = new IconsConverter())
            {
                var taskPane = CustomControlHelper.HostControl<TControl, SwTaskPane<TControl>>(
                    (c, h, t, i) =>
                    {
                        var v = CreateTaskPaneView(iconConv, i, t);
                        
                        if (!v.DisplayWindowFromHandle(h.Handle.ToInt32()))
                        {
                            throw new NetControlHostException(h.Handle);
                        }

                        return new SwTaskPane<TControl>(Application.Sw, v, c, spec);
                    },
                    (p, t, i) =>
                    {
                        var v = CreateTaskPaneView(iconConv, i, t);
                        var ctrl = (TControl)v.AddControl(p, "");

                        if (ctrl == null)
                        {
                            throw new ComControlHostException(p);
                        }

                        return new SwTaskPane<TControl>(Application.Sw, v, ctrl, spec);
                    });

                m_Disposables.Add(taskPane);

                return taskPane;
            }
        }

        public SwFeatureMgrTab<TControl> CreateFeatureManagerTab<TControl>(SwDocument doc) 
        {
            var mdlViewMgr = doc.Model.ModelViewManager;

            using (var iconsConv = new IconsConverter())
            {
                return CustomControlHelper.HostControl<TControl, SwFeatureMgrTab<TControl>>(
                    (c, h, t, i) =>
                    {
                        var imgPath = iconsConv.ConvertIcon(new FeatMgrViewIcon(i)).First();

                        var featMgr = mdlViewMgr.CreateFeatureMgrWindowFromHandlex64(
                            imgPath, h.Handle.ToInt64(), t, (int)swFeatMgrPane_e.swFeatMgrPaneBottom) as IFeatMgrView;

                        if (featMgr != null)
                        {
                            return new SwFeatureMgrTab<TControl>(c, featMgr, doc);
                        }
                        else
                        {
                            throw new NetControlHostException(h.Handle);
                        }
                    },
                    (p, t, i) =>
                    {
                        var imgPath = iconsConv.ConvertIcon(new FeatMgrViewIcon(i)).First();

                        var featMgr = mdlViewMgr.CreateFeatureMgrControl3(imgPath, p, "", t,
                            (int)swFeatMgrPane_e.swFeatMgrPaneBottom) as IFeatMgrView;

                        TControl ctrl = default;

                        if (featMgr != null)
                        {
                            ctrl = (TControl)featMgr.GetControl();
                        }

                        if (ctrl == null)
                        {
                            throw new ComControlHostException(p);
                        }

                        return new SwFeatureMgrTab<TControl>(ctrl, featMgr, doc);
                    });
            }
        }

        public virtual void ConfigureServices(IXServiceCollection collection)
        {
        }
    }

    public static class SwAddInExExtension 
    {
        public static SwModelViewTab<TControl> CreateDocumentTabWinForm<TControl>(this SwAddInEx addIn, SwDocument doc)
            where TControl : System.Windows.Forms.Control => addIn.CreateDocumentTab<TControl>(doc);

        public static SwModelViewTab<TControl> CreateDocumentTabWpf<TControl>(this SwAddInEx addIn, SwDocument doc)
            where TControl : System.Windows.UIElement => addIn.CreateDocumentTab<TControl>(doc);

        public static SwPopupWpfWindow<TWindow> CreatePopupWpfWindow<TWindow>(this SwAddInEx addIn)
            where TWindow : System.Windows.Window => (SwPopupWpfWindow<TWindow>)addIn.CreatePopupWindow<TWindow>();

        public static SwPopupWinForm<TWindow> CreatePopupWinForm<TWindow>(this SwAddInEx addIn)
            where TWindow : System.Windows.Forms.Form => (SwPopupWinForm<TWindow>)addIn.CreatePopupWindow<TWindow>();

        public static SwTaskPane<TControl> CreateTaskPaneWinForm<TControl>(this SwAddInEx addIn, TaskPaneSpec spec = null)
            where TControl : System.Windows.Forms.Control => addIn.CreateTaskPane<TControl>(spec);

        public static SwTaskPane<TControl> CreateTaskPaneWpf<TControl>(this SwAddInEx addIn, TaskPaneSpec spec = null)
            where TControl : System.Windows.UIElement => addIn.CreateTaskPane<TControl>(spec);

        public static IXEnumTaskPane<TControl, TEnum> CreateTaskPaneWinForm<TControl, TEnum>(this SwAddInEx addIn)
            where TControl : System.Windows.Forms.Control
            where TEnum : Enum => addIn.CreateTaskPane<TControl, TEnum>();

        public static IXEnumTaskPane<TControl, TEnum> CreateTaskPaneWpf<TControl, TEnum>(this SwAddInEx addIn)
            where TControl : System.Windows.UIElement
            where TEnum : Enum => addIn.CreateTaskPane<TControl, TEnum>();

        public static SwFeatureMgrTab<TControl> CreateFeatureManagerTabWpf<TControl>(this SwAddInEx addIn, SwDocument doc)
            where TControl : System.Windows.UIElement => addIn.CreateFeatureManagerTab<TControl>(doc);

        public static SwFeatureMgrTab<TControl> CreateFeatureManagerTabWinForm<TControl>(this SwAddInEx addIn, SwDocument doc)
            where TControl : System.Windows.Forms.Control => addIn.CreateFeatureManagerTab<TControl>(doc);
    }
}