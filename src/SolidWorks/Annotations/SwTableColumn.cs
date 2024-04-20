//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Diagnostics;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.Toolkit.Data;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Annotations
{
    [DebuggerDisplay("{" + nameof(Title) + "} [{" + nameof(Index) + "}]")]
    internal class SwTableColumn : TableElement, IXTableColumn
    {
        public bool Visible
        {
            get
            {
                CheckDeleted();
                return !m_Table.TableAnnotation.ColumnHidden[Index];
            }
            set
            {
                CheckDeleted();
                m_Table.TableAnnotation.ColumnHidden[Index] = !value;
            }
        }

        public string Title 
        {
            get
            {
                CheckDeleted();

                if (m_Table.OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2018))
                {
                    return m_Table.TableAnnotation.GetColumnTitle2(Index, true);
                }
                else 
                {
                    return m_Table.TableAnnotation.GetColumnTitle(Index);
                }
            }
            set
            {
                CheckDeleted();

                if (m_Table.OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2018))
                {
                    m_Table.TableAnnotation.SetColumnTitle2(Index, value, true);
                }
                else
                {
                    m_Table.TableAnnotation.SetColumnTitle(Index, value);
                }
            }
        }

        private readonly SwTable m_Table;

        internal SwTableColumn(SwTable table, int? index, ChangeTracker changeTracker) : base(index, changeTracker)
        {
            m_Table = table;
        }

        protected override void Move(int to)
        {
            m_ChangeTracker.Move(Index, to);
            throw new NotImplementedException();
        }

        protected override void CreateElement(int index, CancellationToken cancellationToken)
        {
            m_ChangeTracker.Insert(index);
            throw new NotImplementedException();
        }
    }
}
