//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Linq;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public class SwBody : SwSelObject, IXBody
    {
        public static SwBody operator -(SwBody firstBody, SwBody secondBody)
        {
            return (SwBody)firstBody.Substract(secondBody).First();
        }

        public static SwBody operator +(SwBody firstBody, SwBody secondBody)
        {
            return (SwBody)firstBody.Add(secondBody);
        }

        public virtual IBody2 Body { get; }

        public bool Visible
        {
            get => Body.Visible;
            set
            {
                Body.HideBody(!value);
            }
        }

        internal SwBody(IBody2 body) : base(null, body)
        {
            Body = body;
        }

        public IXBody Add(IXBody other)
        {
            return PerformOperation(other, swBodyOperationType_e.SWBODYADD).FirstOrDefault();
        }

        public IXBody[] Substract(IXBody other)
        {
            return PerformOperation(other, swBodyOperationType_e.SWBODYCUT);
        }

        public IXBody[] Common(IXBody other)
        {
            return PerformOperation(other, swBodyOperationType_e.SWBODYINTERSECT);
        }

        public SwTempBody ToTempBody()
        {
            return FromDispatch<SwTempBody>(Body.ICopy());
        }

        private IXBody[] PerformOperation(IXBody other, swBodyOperationType_e op)
        {
            if (other is SwBody)
            {
                var thisBody = Body;
                var otherBody = (other as SwBody).Body;

                int errs;
                var res = thisBody.Operations2((int)op, otherBody, out errs) as object[];

                if (errs != (int)swBodyOperationError_e.swBodyOperationNoError)
                {
                    throw new Exception($"Body boolean operation failed: {(swBodyOperationError_e)errs}");
                }

                if (res?.Any() == true)
                {
                    return res.Select(b => FromDispatch<SwBody>(b as IBody2)).ToArray();
                }
                else
                {
                    return new IXBody[0];
                }
            }
            else
            {
                throw new InvalidCastException();
            }
        }

        public override void Select(bool append)
        {
            if (!Body.Select2(append, null))
            {
                throw new Exception("Failed to select body");
            }
        }
    }
}