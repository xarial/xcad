﻿using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.SolidWorks.Graphics;

namespace Xarial.XCad.SolidWorks.Services
{
    public interface ITriadHandlerProvider
    {
        SwTriadHandler CreateHandler(ISldWorks app);
    }
}