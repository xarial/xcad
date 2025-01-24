//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Xarial.XCad.Toolkit
{
    /// <summary>
    /// Represents identifier based on the simple types
    /// </summary>
    public class XIdentifier : IXIdentifier
    {
        private enum XIdentifierType_e
        {
            DateTime,
            Long,
            Int,
            Byte,
            String,
            ByteArr,
            IntArr
        }

        private readonly object m_Object;
        private readonly DateTime m_UtcDate;
        private readonly long m_Long;
        private readonly int m_Int;
        private readonly byte m_Byte;
        private readonly string m_String;
        private readonly byte[] m_ByteArr;
        private readonly int[] m_IntArr;

        private readonly XIdentifierType_e m_Type;

        /// <inheritdoc/>
        public byte[] Thumbprint
        {
            get
            {
                switch (m_Type)
                {
                    case XIdentifierType_e.DateTime:
                        return BitConverter.GetBytes(m_UtcDate.Ticks);
                    case XIdentifierType_e.Long:
                        return BitConverter.GetBytes(m_Long);
                    case XIdentifierType_e.Int:
                        return BitConverter.GetBytes(m_Int);
                    case XIdentifierType_e.Byte:
                        return new byte[] { m_Byte };
                    case XIdentifierType_e.String:
                        return Encoding.UTF8.GetBytes(m_String);
                    case XIdentifierType_e.ByteArr:
                        return m_ByteArr;
                    case XIdentifierType_e.IntArr:
                        {
                            using (var memStr = new MemoryStream())
                            {
                                if (m_IntArr != null)
                                {
                                    using (var writer = new BinaryWriter(memStr))
                                    {
                                        foreach (var val in m_IntArr)
                                        {
                                            writer.Write(val);
                                        }
                                    }
                                }

                                memStr.Seek(0, SeekOrigin.Begin);

                                return memStr.ToArray();
                            }
                        }
                    default:
                        throw new NotSupportedException();
                }
            }

        }

        /// <summary>
        /// Constructor of date-based identifier
        /// </summary>
        /// <param name="id">Date value</param>
        public XIdentifier(DateTime id)
        {
            m_Type = XIdentifierType_e.DateTime;
            m_UtcDate = id.ToUniversalTime();
        }

        /// <summary>
        /// Constructor of long-based identifier
        /// </summary>
        /// <param name="id">Long value</param>
        public XIdentifier(long id)
        {
            m_Type = XIdentifierType_e.Long;
            m_Long = id;
        }

        /// <summary>
        /// Constructor of integer-based identifier
        /// </summary>
        /// <param name="id">Integer value</param>
        public XIdentifier(int id)
        {
            m_Type = XIdentifierType_e.Int;
            m_Int = id;
        }

        /// <summary>
        /// Constructor of byte-based identifier
        /// </summary>
        /// <param name="id">Byte value</param>
        public XIdentifier(byte id)
        {
            m_Type = XIdentifierType_e.Byte;
            m_Byte = id;
        }

        /// <summary>
        /// Constructor of string-based identifier
        /// </summary>
        /// <param name="id">String value</param>
        public XIdentifier(string id)
        {
            m_Type = XIdentifierType_e.String;
            m_String = id;
        }

        /// <summary>
        /// Constructor of byte array-based identifier
        /// </summary>
        /// <param name="id">Byte array value</param>
        public XIdentifier(byte[] id)
        {
            m_Type = XIdentifierType_e.ByteArr;
            m_ByteArr = id;
        }

        /// <summary>
        /// Constructor of byte array-based identifier
        /// </summary>
        /// <param name="id">Byte array value</param>
        public XIdentifier(int[] id)
        {
            m_Type = XIdentifierType_e.IntArr;
            m_IntArr = id;
        }

        /// <inheritdoc/>
        public bool Equals(IXIdentifier other)
        {
            if (other is XIdentifier)
            {
                var otherId = (XIdentifier)other;

                if (otherId.m_Type == m_Type)
                {
                    switch (m_Type)
                    {
                        case XIdentifierType_e.DateTime:
                            return m_UtcDate.Ticks == otherId.m_UtcDate.Ticks;
                        case XIdentifierType_e.Long:
                            return m_Long == otherId.m_Long;
                        case XIdentifierType_e.Int:
                            return m_Int == otherId.m_Int;
                        case XIdentifierType_e.Byte:
                            return m_Byte == otherId.m_Byte;
                        case XIdentifierType_e.String:
                            return m_String == otherId.m_String;
                        case XIdentifierType_e.ByteArr:
                            return (m_ByteArr ?? Enumerable.Empty<byte>()).SequenceEqual(otherId?.m_ByteArr ?? Enumerable.Empty<byte>());
                        case XIdentifierType_e.IntArr:
                            return (m_IntArr ?? Enumerable.Empty<int>()).SequenceEqual(otherId?.m_IntArr ?? Enumerable.Empty<int>());
                        default:
                            throw new NotSupportedException();
                    }
                }
            }

            return false;
        }
    }
}
