//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Media;
using Xarial.XCad.Annotations;
using Xarial.XCad.Annotations.Delegates;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Annotations.EventHandlers;
using Xarial.XCad.SolidWorks.Annotations.Exceptions;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Annotations
{
    /// <summary>
    /// SOLIDWORKS specific dimension
    /// </summary>
    public interface ISwDimension : IXDimension, IDisposable, ISwSelObject, ISwAnnotation
    {
        /// <summary>
        /// Pointer to dimension
        /// </summary>
        IDimension Dimension { get; }

        /// <summary>
        /// Pointer to display dimension
        /// </summary>
        IDisplayDimension DisplayDimension { get; }
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwDimension : SwAnnotation, ISwDimension
    {
        internal static SwDimension New(IDisplayDimension dispDim, SwDocument doc, SwApplication app)
        {
            if (doc is IXDrawing)
            {
                return SwDrawingDimension.New(dispDim, (SwDrawing)doc, app);
            }
            else
            {
                return new SwDimension(dispDim, doc, app);
            }
        }

        private IDimension m_Dimension;

        private double? m_CachedValue;

        private SwDimensionChangeEventsHandler m_ValueChangedHandler;

        public event DimensionValueChangedDelegate ValueChanged
        {
            add => m_ValueChangedHandler.Attach(value);
            remove => m_ValueChangedHandler.Detach(value);
        }

        public IDimension Dimension => m_Dimension ?? (m_Dimension = DisplayDimension?.GetDimension2(0));
        public IDisplayDimension DisplayDimension { get; private set; }

        public string Name 
        {
            get 
            {
                var fullName = Dimension.FullName;
                var nameParts = fullName.Split('@');

                return $"{nameParts[0]}@{nameParts[1]}";
            }
        }

        public virtual double Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        public override object Dispatch => DisplayDimension;

        protected Context m_Context;

        protected SwDimension(IDisplayDimension dispDim, SwDocument doc, SwApplication app)
            : base(dispDim.IGetAnnotation(), doc, app)
        {
            if (doc == null) 
            {
                throw new ArgumentNullException(nameof(doc));
            }

            m_Context = new Context(doc);
            
            DisplayDimension = dispDim;

            m_ValueChangedHandler = new SwDimensionChangeEventsHandler(this, doc);
        }

        internal void SetContext(Context context)
        {
            ValidateContext(context);

            m_Context = context;
        }

        internal void CommitCachedValue() 
        {
            if (m_CachedValue.HasValue) 
            {
                SetValue(m_CachedValue.Value);
            }
        }

        private void ValidateContext(Context context)
        {
            //should not throw an exception
            GetConfigurationNameFromContext(context);
        }

        protected double GetValue()
        {
            if (m_Context?.Owner.IsCommitted != false)
            {
                if (!IsInContextDimension() && !(m_Context.Owner is IXDrawing))
                {
                    string[] confs;
                    swInConfigurationOpts_e opts;

                    var confName = GetConfigurationNameFromContext(m_Context);

                    if (!string.IsNullOrEmpty(confName))
                    {
                        opts = swInConfigurationOpts_e.swSpecifyConfiguration;
                        confs = new string[] { confName };
                    }
                    else
                    {
                        opts = swInConfigurationOpts_e.swThisConfiguration;
                        confs = null;
                    }

                    return ((double[])Dimension.GetSystemValue3((int)opts, confs))[0];
                }
                else 
                {
                    //NOTE: dimensions in the drawings or in the context of the assembly (e.g. selected from the component)
                    //cannot get the value of the current component's configuration and its owner is returned in the reference document's context
                    //however obsolete method returns the correct value of the current context
                    return Dimension.SystemValue;
                }
            }
            else 
            {
                if (m_CachedValue.HasValue)
                {
                    return m_CachedValue.Value;
                }
                else 
                {
                    return double.NaN;
                }
            }
        }

        private bool IsInContextDimension() 
        {
            if (m_Context?.Owner is ISwAssembly) 
            {
                var annOwner = Annotation.Owner;

                if (annOwner is IPartDoc || annOwner is IAssemblyDoc) 
                {
                    return OwnerApplication.Sw.IsSame(((ISwDocument)m_Context.Owner).Model, annOwner) != (int)swObjectEquality.swObjectSame;
                }
            }

            return false;
        }

        protected void SetValue(double val)
        {
            if (m_Context?.Owner.IsCommitted != false)
            {
                if (Dimension.DrivenState != (int)swDimensionDrivenState_e.swDimensionDriven)
                {
                    string[] confs;
                    swSetValueInConfiguration_e opts;

                    var confName = GetConfigurationNameFromContext(m_Context);

                    if (!string.IsNullOrEmpty(confName))
                    {
                        opts = swSetValueInConfiguration_e.swSetValue_InSpecificConfigurations;
                        confs = new string[] { confName };
                    }
                    else
                    {
                        opts = swSetValueInConfiguration_e.swSetValue_UseCurrentSetting;
                        confs = null;
                    }

                    var res = (swSetValueReturnStatus_e)Dimension.SetSystemValue3(val, (int)opts, confs);

                    if (res != swSetValueReturnStatus_e.swSetValue_Successful)
                    {
                        throw new Exception($"Failed to change the dimension value: {res}");
                    }
                }
                else 
                {
                    throw new NotEditableDrivenDimensionException();
                }
            }
            else 
            {
                m_CachedValue = val;
            }
        }

        private string GetConfigurationNameFromContext(Context context)
        {
            if (context != null)
            {
                switch (context.Owner)
                {
                    case ISwDocument _:
                        return "";

                    case ISwComponent comp:
                        return comp.ReferencedConfiguration.Name;

                    case ISwConfiguration conf:
                        return conf.Name;

                    default:
                        throw new Exception("Invalid context of the dimension");
                }
            }
            else
            {
                throw new NullReferenceException("Context is not specified");
            }
        }

        public void Dispose()
        {
            m_ValueChangedHandler.Dispose();

            Dispose(true);

            //NOTE: releasing the pointers as unreleased pointer might cause crash
            if (m_Dimension != null && Marshal.IsComObject(m_Dimension))
            {
                Marshal.ReleaseComObject(m_Dimension);
                m_Dimension = null;
            }

            if (DisplayDimension != null && Marshal.IsComObject(DisplayDimension))
            {
                Marshal.ReleaseComObject(DisplayDimension);
                DisplayDimension = null;
            }

            GC.Collect();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        internal override void Select(bool append, ISelectData selData)
        {
            if (!DisplayDimension.IGetAnnotation().Select3(append, (SelectData)selData))
            {
                throw new Exception("Failed to select dimension");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }

    internal class SwDrawingDimension : SwDimension, IXDrawingDimension
    {
        internal static SwDrawingDimension New(IDisplayDimension dispDim, SwDrawing drw, SwApplication app)
            => new SwDrawingDimension(dispDim, drw, app);

        public IXObject Owner
        {
            get => m_DrwAnnWrapper.Owner;
            set => m_DrwAnnWrapper.Owner = value;
        }

        private readonly SwDrawingAnnotationWrapper m_DrwAnnWrapper;

        private SwDrawingDimension(IDisplayDimension dispDim, SwDrawing drw, SwApplication app) : base(dispDim, drw, app)
        {
            m_DrwAnnWrapper = new SwDrawingAnnotationWrapper(this);
        }
    }

    /// <summary>
    /// This is a specific dimension related to a macro feature
    /// </summary>
    internal class SwMacroFeatureDimension : SwDimension
    {
        internal SwMacroFeatureDimension(IDisplayDimension dispDim, SwDocument doc, SwApplication app) : base(dispDim, doc, app)
        {
        }

        /// <summary>
        /// Macro feature dimensions are managing their values in the different way. The dimension will always be attached to the correct configuration
        /// Attempt to read the dimension value from the specific configuration with GetSystemValue3 method may result in 0 value for the multi-configuration part
        /// </summary>
        public override double Value
        {
            get => Dimension.SystemValue;
            set => Dimension.SystemValue = value; 
        }
    }
}