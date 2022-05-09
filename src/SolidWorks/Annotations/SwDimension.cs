//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Runtime.InteropServices;
using Xarial.XCad.Annotations;
using Xarial.XCad.Annotations.Delegates;
using Xarial.XCad.SolidWorks.Annotations.EventHandlers;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Annotations
{
    public interface ISwDimension : IXDimension, IDisposable, ISwSelObject, ISwAnnotation
    {
        IDimension Dimension { get; }
        IDisplayDimension DisplayDimension { get; }
    }

    internal class SwDimension : SwAnnotation, ISwDimension
    {
        private IDimension m_Dimension;

        private SwDimensionChangeEventsHandler m_ValueChangedHandler;

        public event DimensionValueChangedDelegate ValueChanged
        {
            add
            {
                m_ValueChangedHandler.Attach(value);
            }
            remove
            {
                m_ValueChangedHandler.Detach(value);
            }
        }

        public IDimension Dimension => m_Dimension ?? (m_Dimension = DisplayDimension.GetDimension2(0));
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

        internal SwDimension(IDisplayDimension dispDim, ISwDocument doc, ISwApplication app)
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

        private void ValidateContext(Context context) => ParseContext(context, out _);

        protected double GetValue()
        {
            var val = double.NaN;

            ProcessDimension((opts, confs) =>
            {
                if (opts == swInConfigurationOpts_e.swAllConfiguration) 
                {
                    opts = swInConfigurationOpts_e.swSpecifyConfiguration;
                }

                val = ((double[])Dimension.GetSystemValue3((int)opts, confs))[0];
            });

            return val;
        }

        protected void SetValue(double val)
        {
            ProcessDimension((opts, confs) =>
            {
                if (opts == swInConfigurationOpts_e.swAllConfiguration) 
                {
                    confs = null;
                }

                Dimension.SetSystemValue3(val, (int)opts, confs);
            });
        }

        private void ProcessDimension(Action<swInConfigurationOpts_e, string[]> action) 
        {
            swInConfigurationOpts_e opts;
            string[] confs;

            if (m_Context != null)
            {
                opts = ParseContext(m_Context, out string confName);

                confs = new string[] { confName };
            }
            else
            {
                opts = swInConfigurationOpts_e.swThisConfiguration;
                confs = null;
            }

            action.Invoke(opts, confs);
        }

        private swInConfigurationOpts_e ParseContext(Context context, out string confName)
        {
            if (context == null) 
            {
                throw new NullReferenceException("Context is not specified");
            }

            switch (context.Owner)
            {
                case ISwDocument doc:
                    if (doc is ISwDocument3D)
                    {
                        confName = ((ISwDocument3D)doc).Configurations.Active.Name;
                    }
                    else
                    {
                        confName = "";
                    }
                    return swInConfigurationOpts_e.swAllConfiguration;

                case ISwComponent comp:
                    confName = comp.ReferencedConfiguration.Name;
                    return swInConfigurationOpts_e.swAllConfiguration;

                case ISwConfiguration conf:
                    confName = conf.Name;
                    return swInConfigurationOpts_e.swSpecifyConfiguration;

                default:
                    throw new Exception("Invalid context of the dimension");
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

    /// <summary>
    /// This is a specific dimension related to a macro feature
    /// </summary>
    internal class SwMacroFeatureDimension : SwDimension
    {
        internal SwMacroFeatureDimension(IDisplayDimension dispDim, ISwDocument doc, ISwApplication app) : base(dispDim, doc, app)
        {
        }

        /// <summary>
        /// Macro feature dimensions are managing thei rvalues in the different way. The dimension will always be attached to the correct configuration
        /// Attempt to read the dimension value from the specific configuration with GetSystemValue3 method may result in 0 value for the multi-part configuration
        /// </summary>
        public override double Value
        {
            get => Dimension.SystemValue;
            set => Dimension.SystemValue = value; 
        }
    }
}