//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Linq;
using System.Runtime.InteropServices;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;

namespace SwAddInExample
{
    public class OglRectangeRenderer : IXCustomGraphicsRenderer
    {
        [DllImport("opengl32")]
        public static extern void glBegin(uint mode);

        [DllImport("opengl32")]
        public static extern void glEnd();

        [DllImport("opengl32")]
        public static extern void glVertex3d(double x, double y, double z);

        [DllImport("opengl32.dll")]
        public static extern void glDisable(uint cap);

        [DllImport("opengl32.dll")]
        public static extern void glColor4f(float R, float G, float B, float A);

        [DllImport("opengl32.dll")]
        public static extern void glEnable(uint cap);

        public const uint GL_TRIANGLE_FAN = 0x0006;
        public const uint GL_LIGHTING = 0x0B50;
        public const int GL_BLEND = 0x0BE2;

        private readonly Point[] m_Vertices;
        private readonly System.Drawing.Color m_Color;

        public OglRectangeRenderer(Rect2D rect, System.Drawing.Color color) 
        {
            m_Color = color;

            m_Vertices = new Point[]
            {
                rect.GetLeftTop(),
                rect.GetRightTop(),
                rect.GetRightBottom(),
                rect.GetLeftBottom()
            };
        }

        public void Render()
        {
            glDisable(GL_LIGHTING);
            glEnable(GL_BLEND);

            glBegin(GL_TRIANGLE_FAN);
            glColor4f(m_Color.R / 255f, m_Color.G / 255f, m_Color.B / 255f, m_Color.A / 255f);
            
            foreach (var vertex in m_Vertices)
            {
                glVertex3d(vertex.X, vertex.Y, vertex.Z);
            }

            glEnd();
        }

        public void Dispose()
        {
        }
    }
}
