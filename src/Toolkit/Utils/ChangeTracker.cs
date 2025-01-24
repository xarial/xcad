//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Toolkit.Utils
{
    /// <summary>
    /// Helper class to track changes in the tables
    /// </summary>
    public class ChangeTracker
    {
        public event Action<int, int> Moved;
        public event Action<int> Inserted;
        public event Action<int> Deleted;

        public void Move(int from, int to)
            => Moved?.Invoke(from, to);

        public void Insert(int to)
            => Inserted?.Invoke(to);

        public void Delete(int from)
            => Deleted?.Invoke(from);

        public void HandleMoved(int from, int to, ref int index)
        {
            if (index == from)
            {
                index = to;
            }
            else if (from > index && to <= index)
            {
                index++;
            }
            else if(from < index && to >= index) 
            {
                index--;
            }
        }

        public void HandleInserted(int to, ref int index)
        {
            if (index >= to)
            {
                index++;
            }
        }

        public void HandleDeleted(int from, ref int index)
        {
            if (index == from)
            {
                throw new Exception("Element is deleted");
            }
            else if (index > from)
            {
                index--;
            }
        }
    }
}
