//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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
                    return ReadValue(out _);
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

        public string Expression
        {
            get
            {
                if (IsCommitted)
                {
                    ReadValue(out string exp);
                    return exp;
                }
                else
                {
                    return m_TempValue as string;
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

        protected object ReadValue(out string expression)
        {
            var val = ReadRawValue(out SwDmCustomInfoType type, out expression);

            object resVal;

            switch (type)
            {
                case SwDmCustomInfoType.swDmCustomInfoText:
                    resVal = val;
                    break;

                case SwDmCustomInfoType.swDmCustomInfoYesOrNo:
                    switch (val.ToLower())
                    {
                        case "yes":
                            resVal = true;
                            break;

                        case "no":
                            resVal = false;
                            break;

                        default:
                            if (bool.TryParse(val, out var boolVal))
                            {
                                resVal = boolVal;
                            }
                            else
                            {
                                resVal = val;
                            }
                            break;
                    }
                    break;

                case SwDmCustomInfoType.swDmCustomInfoNumber:
                    resVal = double.Parse(val);
                    break;

                case SwDmCustomInfoType.swDmCustomInfoDate:
                    resVal = DateTime.Parse(val);
                    break;

                default:
                    resVal = val;
                    break;
            }

            if (string.IsNullOrEmpty(expression))
            {
                expression = val;
            }

            return resVal;
        }

        internal abstract void Delete();
        protected abstract string ReadRawValue(out SwDmCustomInfoType type, out string linkedTo);
        protected abstract void AddValue(object value);
        protected abstract void SetValue(object value);
    }
}
