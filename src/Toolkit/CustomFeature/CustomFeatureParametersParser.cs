//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Features;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Features.CustomFeature.Attributes;
using Xarial.XCad.Features.CustomFeature.Enums;
using Xarial.XCad.Features.CustomFeature.Services;
using Xarial.XCad.Features.CustomFeature.Structures;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Reflection;
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.Utils.CustomFeature
{
    /// <summary>
    /// Helper utility allowsing to parse and convert parameters of the custom feature to the class
    /// </summary>
    public class CustomFeatureParametersParser
    {
        private class DimensionParamData
        {
            internal CustomFeatureDimensionType_e Type { get; private set; }
            internal double Value { get; private set; }

            internal DimensionParamData(CustomFeatureDimensionType_e type, double val)
            {
                Type = type;
                Value = val;
            }
        }

        private class PropertyObject<TObject>
        {
            internal TObject Object { get; private set; }
            internal string PropertyName { get; private set; }

            internal PropertyObject(string prpName, TObject obj)
            {
                PropertyName = prpName;
                Object = obj;
            }
        }

        /// <summary>
        /// Name of the attribute which is holding version of dimensions
        /// </summary>
        public const string VERSION_DIMENSIONS_NAME = "__dimsVersion";

        /// <summary>
        /// Name of the attribute which is holding version of parameters
        /// </summary>
        public const string VERSION_PARAMETERS_NAME = "__paramsVersion";

        /// <summary>
        /// Used to mark the index of the object in the list as not set (e.g. null)
        /// </summary>
        private const int LIST_INDEX_NOT_SET = -1;

        private readonly FaultObjectFactory m_FaultObjectFactory;

        /// <summary>
        /// Constructor
        /// </summary>
        public CustomFeatureParametersParser() 
        {
            m_FaultObjectFactory = new FaultObjectFactory();
        }

        /// <summary>
        /// Reads the parameters from the feature definition
        /// </summary>
        public object BuildParameters(Type paramsType,
            ref Dictionary<string, object> featPrps,
            ref IXDimension[] featDims, ref IXBody[] featEditBodies,
            ref CustomFeatureSelectionInfo[] featSels, out string[] dispDimParams)
        {
            var parameters = new Dictionary<string, object>();

            if (featPrps?.Any() == true)
            {
                foreach (var featRawParam in featPrps)
                {
                    var paramName = featRawParam.Key;

                    if (paramName != VERSION_PARAMETERS_NAME && paramName != VERSION_DIMENSIONS_NAME)
                    {
                        var paramVal = featRawParam.Value;
                        parameters.Add(paramName, paramVal);
                    }
                }
            }

            var resParams = Activator.CreateInstance(paramsType);

            var dispDimParamsMap = new SortedDictionary<int, string>();

            var featDimsLocal = featDims ?? new IXDimension[0];
            var featEditBodiesLocal = featEditBodies ?? new IXBody[0];
            var featSelsLocal = featSels ?? new CustomFeatureSelectionInfo[0];

            TraverseParametersDefinition(resParams,
                (obj, prp) =>
                {
                    AssignObjectsToProperty(obj, featSelsLocal.Select(s => s.Selection).ToArray(), prp, parameters);
                },
                (dimType, obj, prp) =>
                {
                    var dimIndices = GetObjectIndices(prp, parameters);

                    if (dimIndices.Length != 1)
                    {
                        throw new InvalidOperationException(
                            "It could only be one index associated with dimension");
                    }

                    var dimInd = dimIndices.First();

                    if (featDimsLocal.Length > dimInd)
                    {
                        var dispDim = featDimsLocal[dimInd];

                        var val = dispDim.Value;

                        if (!double.IsNaN(val))
                        {
                            prp.SetValue(obj, val, null);
                        }

                        dispDimParamsMap.Add(dimInd, prp.Name);
                    }
                    else
                    {
                        throw new IndexOutOfRangeException(
                            $"Dimension at index {dimInd} is not present in the macro feature");
                    }
                },
                (obj, prp) =>
                {
                    AssignObjectsToProperty(obj, featEditBodiesLocal, prp, parameters);
                },
                (obj, prp) =>
                {
                    if (TryGetParameterValue(parameters, prp.Name, out object paramVal))
                    {
                        object val = null;

                        if (paramVal != null)
                        {
                            if (prp.PropertyType.IsEnum)
                            {
                                val = Enum.Parse(prp.PropertyType, paramVal.ToString());
                            }
                            else
                            {
                                val = Convert.ChangeType(paramVal, prp.PropertyType);
                            }
                        }

                        prp.SetValue(obj, val, null);
                    }
                });

            dispDimParams = dispDimParamsMap.Values.ToArray();

            return resParams;
        }

        /// <summary>
        /// Parses the custom feature data from the parameters structure
        /// </summary>
        public void Parse(object parameters,
            out CustomFeatureAttribute[] atts, out IXSelObject[] selection,
            out CustomFeatureDimensionType_e[] dimTypes, out double[] dimValues, out IXBody[] editBodies)
        {
            if (parameters == null) 
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var paramAttsList = new List<CustomFeatureAttribute>();

            var selectionList = new List<PropertyObject<IXSelObject>>();
            var dimsList = new List<PropertyObject<DimensionParamData>>();
            var editBodiesList = new List<PropertyObject<IXBody>>();

            TraverseParametersDefinition(parameters,
                (obj, prp) =>
                {
                    ReadObjectsValueFromProperty(obj, prp, selectionList);
                },
                (dimType, obj, prp) =>
                {
                    var val = Convert.ToDouble(prp.GetValue(obj, null));
                    dimsList.Add(new PropertyObject<DimensionParamData>(
                        prp.Name, new DimensionParamData(dimType, val)));
                },
                (obj, prp) =>
                {
                    ReadObjectsValueFromProperty(obj, prp, editBodiesList);
                },
                (obj, prp) =>
                {
                    var val = prp.GetValue(obj, null);

                    paramAttsList.Add(new CustomFeatureAttribute(prp.Name, prp.PropertyType, val));
                });

            parameters.GetType().TryGetAttribute<ParametersVersionAttribute>(a =>
            {
                var setVersionFunc = new Action<string, Version>((n, v) =>
                {
                    var versParamIndex = paramAttsList.FindIndex(l => l.Name == n);

                    if (versParamIndex == -1)
                    {
                        paramAttsList.Add(new CustomFeatureAttribute(n, typeof(string), v.ToString()));
                    }
                    else
                    {
                        var curParam = paramAttsList[versParamIndex];

                        paramAttsList[versParamIndex] = new CustomFeatureAttribute(curParam.Name, curParam.Type, v.ToString());
                    }
                });

                setVersionFunc.Invoke(VERSION_PARAMETERS_NAME, a.Version);
                setVersionFunc.Invoke(VERSION_DIMENSIONS_NAME, a.Version);
            });

            selection = AddParametersForObjects(selectionList, paramAttsList);
            var dimParams = AddParametersForObjects(dimsList, paramAttsList);
            editBodies = AddParametersForObjects(editBodiesList, paramAttsList);

            atts = paramAttsList.ToArray();

            if (dimParams != null)
            {
                dimTypes = dimParams.Select(d => d.Type).ToArray();
                dimValues = dimParams.Select(d => d.Value).ToArray();
            }
            else
            {
                dimTypes = null;
                dimValues = null;
            }
        }

        /// <summary>
        /// Converts the parameters using the assigned converters
        /// </summary>
        public void ConvertParameters(Type paramsType, IXDocument doc, IXCustomFeature feat, ref Dictionary<string, object> parameters,
            ref CustomFeatureSelectionInfo[] selection, ref IXDimension[] dispDims, ref IXBody[] editBodies)
        {
            var paramsVersion = new Version();
            var dimsVersion = new Version();

            if (parameters?.Any() == true)
            {
                foreach (var featRawParam in parameters)
                {
                    var paramName = featRawParam.Key;

                    var paramVal = featRawParam.Value;

                    if (paramName == VERSION_PARAMETERS_NAME)
                    {
                        paramsVersion = new Version(paramVal.ToString());
                    }
                    else if (paramName == VERSION_DIMENSIONS_NAME)
                    {
                        dimsVersion = new Version(paramVal.ToString());
                    }
                }
            }

            IParametersVersionConverter versConv = null;
            var curParamVersion = new Version();

            paramsType.TryGetAttribute<ParametersVersionAttribute>(a =>
            {
                versConv = a.VersionConverter;
                curParamVersion = a.Version;
            });

            if (curParamVersion != paramsVersion)
            {
                if (curParamVersion > paramsVersion)
                {
                    if (versConv != null)
                    {
                        if (versConv.ContainsKey(curParamVersion))
                        {
                            foreach (var conv in versConv.Where(
                                v => v.Key > paramsVersion && v.Key <= curParamVersion)
                                .OrderBy(v => v.Key))
                            {
                                conv.Value.Convert(doc, feat, ref parameters, ref selection, ref dispDims, ref editBodies);
                            }
                        }
                        else
                        {
                            throw new NullReferenceException($"{curParamVersion} version of parameters {paramsType.FullName} is not registered");
                        }
                    }
                    else
                    {
                        throw new NullReferenceException("Version converter is not set");
                    }
                }
                else
                {
                    throw new FutureVersionParametersException(paramsType, curParamVersion, paramsVersion);
                }
            }
        }

        /// <summary>
        /// Traverses the definiton of the parameters class with custom handler for each parameter group
        /// </summary>
        //TODO: implement the support for the nested types
        public void TraverseParametersDefinition(object parameters,
                    Action<object, PropertyInfo> selParamHandler,
                    Action<CustomFeatureDimensionType_e, object, PropertyInfo> dimParamHandler,
                    Action<object, PropertyInfo> editBodyHandler,
                    Action<object, PropertyInfo> dataParamHandler)
        {
            var paramsType = parameters.GetType();

            foreach (var prp in paramsType.GetProperties())
            {
                if (prp.TryGetAttribute<ParameterExcludeAttribute>() == null)
                {
                    var prpType = prp.PropertyType;

                    var dimAtt = prp.TryGetAttribute<ParameterDimensionAttribute>();
                    var editBodyAtt = prp.TryGetAttribute<ParameterEditBodyAttribute>();

                    if (dimAtt != null)
                    {
                        var dimType = dimAtt.DimensionType;
                        dimParamHandler.Invoke(dimType, parameters, prp);
                    }
                    else if (editBodyAtt != null)
                    {
                        editBodyHandler.Invoke(parameters, prp);
                    }
                    else if (typeof(IXSelObject).IsAssignableFrom(prpType)
                        || typeof(IEnumerable<IXSelObject>).IsAssignableFrom(prpType))
                    {
                        selParamHandler.Invoke(parameters, prp);
                    }
                    else
                    {
                        if (typeof(IConvertible).IsAssignableFrom(prpType))
                        {
                            dataParamHandler.Invoke(parameters, prp);
                        }
                        else
                        {
                            throw new NotSupportedException(
                                $"{prp.Name} is not supported as the parameter of macro feature. Currently only types implementing IConvertible are supported (e.g. primitive types, string, DateTime, decimal)");
                        }
                    }
                }
            }
        }

        private T[] AddParametersForObjects<T>(List<PropertyObject<T>> objects,
            List<CustomFeatureAttribute> paramList)
            where T : class
        {
            T[] retVal = null;

            if (objects != null && objects.Any())
            {
                var allObjects = objects.Select(o => o.Object)
                    .Distinct()
                    .Where(o => o != null).ToList();

                var paramsGroup = objects.GroupBy(o => o.PropertyName).ToDictionary(g => g.Key,
                    g =>
                    {
                        return string.Join(",", g.Select(
                            e =>
                            {
                                var index = allObjects.IndexOf(e.Object);
                                return index;
                            }).ToArray());
                    });

                paramList.AddRange(paramsGroup.Select(g => new CustomFeatureAttribute(g.Key, typeof(string), g.Value)));

                retVal = allObjects.ToArray();
            }

            return retVal;
        }

        private void AssignObjectsToProperty(object resParams, Array availableObjects,
            PropertyInfo prp, Dictionary<string, object> parameters)
        {
            var indices = GetObjectIndices(prp, parameters);

            if (indices?.Any() == true)
            {
                if (availableObjects == null)
                {
                    availableObjects = new object[0];
                }

                object val = null;

                if (indices.All(i => availableObjects.Length > i))
                {
                    if (typeof(IList).IsAssignableFrom(prp.PropertyType))
                    {
                        if (indices.Length == 1 && indices.First() == LIST_INDEX_NOT_SET)
                        {
                            val = null; //no entities in the list
                        }
                        else
                        {
                            //TODO: potential issues with IList as IEnumerable can come here as well and it will fail
                            var lst = (IList)prp.GetValue(resParams, null);

                            if (lst != null)
                            {
                                lst.Clear();
                            }
                            else
                            {
                                lst = (IList)Activator.CreateInstance(prp.PropertyType);
                            }

                            val = lst;

                            foreach (var obj in indices.Select(i =>
                            {
                                object elem;

                                if (i == LIST_INDEX_NOT_SET)
                                {
                                    elem = null;
                                }
                                else
                                {
                                    elem = availableObjects.GetValue(i);

                                    if (elem == null)
                                    {
                                        var elemType = prp.PropertyType.GetArgumentsOfGenericType(typeof(IList<>))[0];

                                        elem = m_FaultObjectFactory.CreateFaultObject(elemType);
                                    }
                                }

                                return elem;
                            }))
                            {
                                lst.Add(obj);
                            }
                        }
                    }
                    else
                    {
                        if (indices.Length > 1)
                        {
                            throw new InvalidOperationException($"Multiple selection indices at {prp.Name} could only be associated with the List");
                        }

                        var index = indices.First();

                        if (index == LIST_INDEX_NOT_SET)
                        {
                            val = null;
                        }
                        else
                        {
                            val = availableObjects.GetValue(index);

                            if (val == null)
                            {
                                val = m_FaultObjectFactory.CreateFaultObject(prp.PropertyType);
                            }
                        }
                    }
                }
                else
                {
                    throw new IndexOutOfRangeException(
                        $"Some of the referenced entity indices ({string.Join(", ", indices)}) are out of range for '{prp.Name}' (max size: {availableObjects.Length})");
                }

                prp.SetValue(resParams, val, null);
            }
            else
            {
                throw new NullReferenceException($"Indices are not set for {prp.PropertyType.Name}");
            }
        }

        private int[] GetObjectIndices(PropertyInfo prp, Dictionary<string, object> parameters)
        {
            int[] indices = null;

            object indValues;

            if (parameters.TryGetValue(prp.Name, out indValues))
            {
                indices = indValues.ToString().Split(',').Select(i => int.Parse(i)).ToArray();
            }

            return indices;
        }

        private bool TryGetParameterValue(Dictionary<string, object> parameters, string name, out object value)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            return parameters.TryGetValue(name, out value);
        }

        private void ReadObjectsValueFromProperty<T>(object parameters,
                    PropertyInfo prp, List<PropertyObject<T>> list)
                    where T : class
        {
            var val = prp.GetValue(parameters, null);

            if (val is IList)
            {
                if ((val as IList).Count != 0)
                {
                    foreach (T lstElem in val as IList)
                    {
                        list.Add(new PropertyObject<T>(prp.Name, lstElem));
                    }
                }
                else
                {
                    list.Add(new PropertyObject<T>(prp.Name, null));
                }
            }
            else
            {
                list.Add(new PropertyObject<T>(prp.Name, val as T));
            }
        }
    }
}