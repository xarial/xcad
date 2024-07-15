//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry.Evaluation
{
    /// <summary>
    /// Provides the tesselation data for the geometry
    /// </summary>
    public interface IXTessellation : IEvaluation
    {
        /// <summary>
        /// Positions array of the tesselation
        /// </summary>
        IEnumerable<Point> Positions { get; }

        /// <summary>
        /// Indices of triangs in relation to <see cref="Positions"/> and <see cref="Normals"/>
        /// </summary>
        IEnumerable<int> TriangleIndices { get; }

        /// <summary>
        /// Normals array of the tesselation
        /// </summary>
        IEnumerable<Vector> Normals { get; }
    }

    /// <summary>
    /// Additional methods of <see cref="IXTessellation"/>
    /// </summary>
    public static class XTessellationExtension 
    {
        /// <summary>
        /// Enumerates triangulation of the tesselation
        /// </summary>
        /// <param name="tess">Tesselation</param>
        /// <returns>Triangles</returns>
        public static IEnumerable<TesselationTriangle> EnumerateTriangles(this IXTessellation tess) 
        {
            var indicesEnumer = tess.TriangleIndices.GetEnumerator();

            var posEnumer = tess.Positions.GetEnumerator();
            var normEnumer = tess.Normals.GetEnumerator();

            var positionsCache = new List<Point>();
            var normCache = new List<Vector>();

            T GetValue<T>(IEnumerator<T> enumer, List<T> cache, int index)
            {
                var lastIndex = cache.Count;

                if (lastIndex > index)
                {
                    return cache[index];
                }
                else 
                {
                    while (enumer.MoveNext()) 
                    {
                        var val = enumer.Current;
                        cache.Add(val);

                        if (lastIndex == index)
                        {
                            return val;
                        }
                        else 
                        {
                            lastIndex++;
                        }
                    }

                    throw new Exception("Failed to find point at index");
                }
            }

            while (indicesEnumer.MoveNext()) 
            {
                var firstInd = indicesEnumer.Current;
                if (indicesEnumer.MoveNext())
                {
                    var secondInd = indicesEnumer.Current;

                    if (indicesEnumer.MoveNext())
                    {
                        var thirdInd = indicesEnumer.Current;

                        yield return new TesselationTriangle(GetValue(normEnumer, normCache, firstInd),
                            GetValue(posEnumer, positionsCache, firstInd),
                            GetValue(posEnumer, positionsCache, secondInd),
                            GetValue(posEnumer, positionsCache, thirdInd));
                    }
                    else
                    {
                        throw new Exception("Invalid indices");
                    }
                }
                else 
                {
                    throw new Exception("Invalid indices");
                }
            }
        }
    }

    /// <summary>
    /// Tesselation specific to the assembly
    /// </summary>
    public interface IXAssemblyTessellation : IXTessellation, IAssemblyEvaluation
    {
    }

    /// <summary>
    /// Face-specific tesselation
    /// </summary>
    public interface IXFaceTesselation : IXTessellation 
    {
        /// <summary>
        /// Faces to get tesselation for
        /// </summary>
        new IXFace[] Scope { get; set;}
    }
}
