using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Delegates;
using Xarial.XCad.Services;

namespace Xarial.XCad.SwDocumentManager.Data
{
    public interface ISwDmCustomProperty : IXProperty
    {
    }

    internal abstract class SwDmCustomProperty : ISwDmCustomProperty
    {
        public event PropertyValueChangedDelegate ValueChanged;

        private string m_Name;
        private object m_TempValue;

        public string Name 
        {
            get => m_Name;
            set => RenameProperty(m_Name, value);
        }

        public object Value 
        {
            get 
            {
                if (IsCommitted)
                {
                    return ReadValue();
                }
                else 
                {
                    return m_TempValue;
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    SetValue(value);
                    ValueChanged?.Invoke(this, value);
                }
                else 
                {
                    m_TempValue = value;
                }
            }
        }

        public bool IsCommitted { get; set; }

        internal SwDmCustomProperty(string name, bool isCreated) 
        {
            m_Name = name;
            IsCommitted = isCreated;
        }

        public void Commit(CancellationToken cancellationToken)
        {
            AddValue(Value);
            IsCommitted = true;
        }

        private void RenameProperty(string from, string to) 
        {
            m_Name = to;

            if (IsCommitted) 
            {
                throw new NotImplementedException();
            }
        }

        protected SwDmCustomInfoType GetPropertyType(object value)
        {
            SwDmCustomInfoType type = SwDmCustomInfoType.swDmCustomInfoUnknown;

            if (value is string)
            {
                type = SwDmCustomInfoType.swDmCustomInfoText;
            }
            else if (value is bool)
            {
                type = SwDmCustomInfoType.swDmCustomInfoYesOrNo;
            }
            else if (value is int || value is double)
            {
                type = SwDmCustomInfoType.swDmCustomInfoNumber;
            }
            else if (value is DateTime)
            {
                type = SwDmCustomInfoType.swDmCustomInfoDate;
            }

            return type;
        }

        internal abstract void Delete();
        protected abstract object ReadValue();
        protected abstract void AddValue(object value);
        protected abstract void SetValue(object value);
    }
}
