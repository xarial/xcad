using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Exceptions
{
    public class EntityNotFoundException : KeyNotFoundException
    {
        public EntityNotFoundException(string name) : base($"Entity '{name}' is not found in the repository") 
        {
        }
    }
}
