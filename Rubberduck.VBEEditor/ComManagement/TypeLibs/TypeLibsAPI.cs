﻿using System;
using System.Collections.Generic;
using Rubberduck.VBEditor.ComManagement.TypeLibs;
using Rubberduck.VBEditor.ComManagement.TypeLibsSupport;
using Rubberduck.VBEditor.SafeComWrappers.Abstract;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Rubberduck.VBEditor.ComManagement.TypeLibsAPI
{
#if DEBUG
    // FOR DEBUGGING/DEVELOPMENT PURPOSES, ALLOW ACCESS TO SOME VBETypeLibsAPI FEATURES FROM VBA
    /* 
        VBA Usage example:

            With Application.VBE.Addins("Rubberduck.Extension").Object
                .ExecuteCode("ProjectName", "ModuleName", "ProcedureName")
            End With
    */
    [System.Runtime.InteropServices.ComVisible(true)]
    public class VBETypeLibsAPI_Object
    {
        IVBE _ide;
        public VBETypeLibsAPI_Object(IVBE ide) 
            => _ide = ide;

        public bool CompileProject(string projectName) 
            => VBETypeLibsAPI.CompileProject(_ide, projectName);
        public bool CompileComponent(string projectName, string componentName) 
            => VBETypeLibsAPI.CompileComponent(_ide, projectName, componentName);
        public object ExecuteCode(string projectName, string standardModuleName, string procName) 
            => VBETypeLibsAPI.ExecuteCode(_ide, projectName, standardModuleName, procName);
        public string GetProjectConditionalCompilationArgsRaw(string projectName)
            => VBETypeLibsAPI.GetProjectConditionalCompilationArgsRaw(_ide, projectName);
        public void SetProjectConditionalCompilationArgsRaw(string projectName, string newConditionalArgs)
            => VBETypeLibsAPI.SetProjectConditionalCompilationArgsRaw(_ide, projectName, newConditionalArgs);
        public bool DoesClassImplementInterface(string projectName, string className, string interfaceProgId) 
            => VBETypeLibsAPI.DoesClassImplementInterface(_ide, projectName, className, interfaceProgId);
        public string GetUserFormControlType(string projectName, string userFormName, string controlName)
            => VBETypeLibsAPI.GetUserFormControlType(_ide, projectName, userFormName, controlName);
        public string GetDocumentClassControlType(string projectName, string documentClassName, string controlName)
            => VBETypeLibsAPI.GetDocumentClassControlType(_ide, projectName, documentClassName, controlName);
        public DocClassType DetermineDocumentClassType(string projectName, string className)
            => VBETypeLibsAPI.DetermineDocumentClassType(_ide, projectName, className);
        public string DocumentAll() 
            => VBETypeLibsAPI.DocumentAll(_ide);
    }
#endif

    /// <summary>
    /// Top level API for accessing live type information from the VBE
    /// </summary>
    /// <remarks>
    /// This class provides a selection of static methods built on top the low-level wrappers [TypLibWrapper/TypeInfoWrapper],
    /// designed for easy access and to encapsulate proper disposal where necessary
    /// </remarks>
    public static class VBETypeLibsAPI
    {
        /// <summary>
        /// Compile an entire VBE project
        /// </summary>
        /// <param name="ide">Safe-com wrapper representing the VBE</param>
        /// <param name="projectName">The VBA project name</param>
        /// <returns>bool indicating success/failure</returns>
        public static bool CompileProject(IVBE ide, string projectName)
        {
            using (var typeLibs = new VBETypeLibsAccessor(ide))
            {
                return CompileProject(typeLibs.Get(projectName));
            }
        }

        /// <summary>
        /// Compile an entire VBA project
        /// </summary>
        /// <param name="project">Safe-com wrapper representing the VBA project</param>
        /// <returns>bool indicating success/failure.</returns>
        public static bool CompileProject(IVBProject project)
        {
            using (var typeLib = TypeLibWrapper.FromVBProject(project))
            {
                return CompileProject(typeLib);
            }
        }

        /// <summary>
        /// Compile an entire VBA project
        /// </summary>
        /// <param name="projectTypeLib">Low-level ITypeLib wrapper representing the VBA project</param>
        /// <returns>bool indicating success/failure</returns>
        public static bool CompileProject(TypeLibWrapper projectTypeLib)
        {
            return projectTypeLib.CompileProject();
        }

        /// <summary>
        /// Compile a single VBA component (e.g. module/class)
        /// </summary>
        /// <remarks>NOTE: This will only return success if ALL components that this component depends on also compile successfully</remarks>
        /// <param name="ide">Safe-com wrapper representing the VBE</param>
        /// <param name="projectName">The VBA project name</param>
        /// <param name="componentName">The name of the component (module/class etc) to compile</param>
        /// <returns>bool indicating success/failure.</returns>
        public static bool CompileComponent(IVBE ide, string projectName, string componentName)
        {
            using (var typeLibs = new VBETypeLibsAccessor(ide))
            {
                return CompileComponent(typeLibs.Get(projectName), componentName);
            }
        }

        /// <summary>
        /// Compile a single VBA component (e.g. module/class)
        /// </summary>
        /// <remarks>NOTE: This will only return success if ALL components that this component depends on also compile successfully</remarks>
        /// <param name="project">Safe-com wrapper representing the VBA project</param>
        /// <param name="componentName">The name of the component (module/class etc) to compile</param>
        /// <returns>bool indicating success/failure.</returns>
        public static bool CompileComponent(IVBProject project, string componentName)
        {
            using (var typeLib = TypeLibWrapper.FromVBProject(project))
            {
                return CompileComponent(typeLib, componentName);
            }
        }

        /// <summary>
        /// Compile a single VBA component (e.g. module/class)
        /// </summary>
        /// <remarks>NOTE: This will only return success if ALL components that this component depends on also compile successfully</remarks>
        /// <param name="projectTypeLib">Low-level ITypeLib wrapper representing the VBA project</param>
        /// <param name="componentName">The name of the component (module/class etc) to compile</param>
        /// <returns>bool indicating success/failure.</returns>
        public static bool CompileComponent(TypeLibWrapper projectTypeLib, string componentName)
        {
            return CompileComponent(projectTypeLib.TypeInfos.Get(componentName));
        }

        /// <summary>
        /// Compile a single VBA component (e.g. module/class)
        /// </summary>
        /// <remarks>NOTE: This will only return success if ALL components that this component depends on also compile successfully</remarks>
        /// <param name="component">Safe-com wrapper representing the VBA component to compile</param>
        /// <returns>bool indicating success/failure.</returns>
        public static bool CompileComponent(IVBComponent component)
        {
            return CompileComponent(component.ParentProject, component.Name);
        }

        /// <summary>
        /// Compile a single VBA component (e.g. module/class)
        /// </summary>
        /// <remarks>NOTE: This will only return success if ALL components that this component depends on also compile successfully</remarks>
        /// <param name="componentTypeInfo">Low-level ITypeInfo wrapper representing the VBA component to compile</param>
        /// <returns>bool indicating success/failure.</returns>
        public static bool CompileComponent(TypeInfoWrapper componentTypeInfo)
        {
            return componentTypeInfo.CompileComponent();
        }

        /// <summary>
        /// Execute a routine inside a standard VBA code module
        /// </summary>
        /// <remarks>the VBA return value returned here can be a COM object, but needs freeing with Marshal.ReleaseComObject to ensure deterministic behaviour.</remarks>
        /// <param name="ide">Safe-com wrapper representing the VBE</param>
        /// <param name="projectName">VBA Project name, as declared in the VBE</param>
        /// <param name="standardModuleName">Module name, as declared in the VBA project</param>
        /// <param name="procName">Procedure name, as declared in the VBA module</param>
        /// <param name="args">optional array of arguments to pass to the VBA routine</param>
        /// <returns>object representing the VBA return value, if one was provided, or null otherwise.</returns>
        public static object ExecuteCode(IVBE ide, string projectName, string standardModuleName, string procName, object[] args = null)
        {
            using (var typeLibs = new VBETypeLibsAccessor(ide))
            {
                return ExecuteCode(typeLibs.Get(projectName), standardModuleName, procName, args);
            }
        }

        /// <summary>
        /// Execute a routine inside a standard VBA code module
        /// </summary>
        /// <remarks>the VBA return value returned here can be a COM object, but needs freeing with Marshal.ReleaseComObject to ensure deterministic behaviour.</remarks>
        /// <param name="project">Safe-com wrapper representing the VBA project</param>
        /// <param name="standardModuleName">Module name, as declared in the VBA project</param>
        /// <param name="procName">Procedure name, as declared in the VBA module</param>
        /// <param name="args">optional array of arguments to pass to the VBA routine</param>
        /// <returns>object representing the VBA return value, if one was provided, or null otherwise.</returns>
        public static object ExecuteCode(IVBProject project, string standardModuleName, string procName, object[] args = null)
        {
            using (var typeLib = TypeLibWrapper.FromVBProject(project))
            {
                return ExecuteCode(typeLib, standardModuleName, procName, args);
            }
        }

        /// <summary>
        /// Execute a routine inside a standard VBA code module
        /// </summary>
        /// <remarks>the VBA return value returned here can be a COM object, but needs freeing with Marshal.ReleaseComObject to ensure deterministic behaviour.</remarks>
        /// <param name="projectTypeLib">Low-level ITypeLib wrapper representing the VBA project which contains the routine</param>
        /// <param name="standardModuleName">Module name, as declared in the VBA project</param>
        /// <param name="procName">Procedure name, as declared in the VBA module</param>
        /// <param name="args">optional array of arguments to pass to the VBA routine</param>
        /// <returns>object representing the VBA return value, if one was provided, or null otherwise.</returns>
        public static object ExecuteCode(TypeLibWrapper projectTypeLib, string standardModuleName, string procName, object[] args = null)
        {
            return ExecuteCode(projectTypeLib.TypeInfos.Get(standardModuleName), procName, args);
        }

        /// <summary>
        /// Execute a routine inside a standard VBA code module
        /// </summary>
        /// <remarks>the VBA return value returned here can be a COM object, but needs freeing with Marshal.ReleaseComObject to ensure deterministic behaviour.</remarks>
        /// <param name="component">Safe-com wrapper representing the VBA component where the routine is defined</param>
        /// <param name="procName">Procedure name, as declared in the VBA module</param>
        /// <param name="args">optional array of arguments to pass to the VBA routine</param>
        /// <returns>object representing the VBA return value, if one was provided, or null otherwise.</returns>
        public static object ExecuteCode(IVBComponent component, string procName, object[] args = null)
        {
            return ExecuteCode(component.ParentProject, component.Name, procName, args);
        }

        /// <summary>
        /// Execute a routine inside a standard VBA code module
        /// </summary>
        /// <remarks>the VBA return value returned here can be a COM object, but needs freeing with Marshal.ReleaseComObject to ensure deterministic behaviour.</remarks>
        /// <param name="standardModuleTypeInfo">Low-level ITypeInfo wrapper representing the VBA component which contains the routine</param>
        /// <param name="procName">Procedure name, as declared in the VBA module</param>
        /// <param name="args">optional array of arguments to pass to the VBA routine</param>
        /// <returns>object representing the VBA return value, if one was provided, or null otherwise.</returns>
        public static object ExecuteCode(TypeInfoWrapper standardModuleTypeInfo, string procName, object[] args = null)
        {
            return standardModuleTypeInfo.StdModExecute(procName, args);
        }

        /// <summary>
        /// Retrieves the developer-defined conditional compilation arguments of a VBA project
        /// </summary>
        /// <remarks>does not expose compiler-defined arguments, such as WIN64, VBA7 etc, which must be determined via the running process</remarks>
        /// <param name="ide">Safe-com wrapper representing the VBE</param>
        /// <param name="projectName">VBA Project name, as declared in the VBE</param>
        /// <returns>returns the raw unparsed conditional arguments string, e.g. "foo = 1 : bar = 2"</returns>
        public static string GetProjectConditionalCompilationArgsRaw(IVBE ide, string projectName)
        {
            using (var typeLibs = new VBETypeLibsAccessor(ide))
            {
                return GetProjectConditionalCompilationArgsRaw(typeLibs.Get(projectName));
            }
        }

        /// <summary>
        /// Retrieves the developer-defined conditional compilation arguments of a VBA project
        /// </summary>
        /// <remarks>does not expose compiler-defined arguments, such as WIN64, VBA7 etc, which must be determined via the running process</remarks>
        /// <param name="project">Safe-com wrapper representing the VBA project</param>
        /// <returns>returns the raw unparsed conditional arguments string, e.g. "foo = 1 : bar = 2"</returns>
        public static string GetProjectConditionalCompilationArgsRaw(IVBProject project)
        {
            using (var typeLib = TypeLibWrapper.FromVBProject(project))
            {
                return GetProjectConditionalCompilationArgsRaw(typeLib);
            }
        }

        /// <summary>
        /// Retrieves the developer-defined conditional compilation arguments of a VBA project
        /// </summary>
        /// <remarks>does not expose compiler-defined arguments, such as WIN64, VBA7 etc, which must be determined via the running process</remarks>
        /// <param name="projectTypeLib">Low-level ITypeLib wrapper representing the VBA project</param>
        /// <returns>returns the raw unparsed conditional arguments string, e.g. "foo = 1 : bar = 2"</returns>
        public static string GetProjectConditionalCompilationArgsRaw(TypeLibWrapper projectTypeLib)
        {
            return projectTypeLib.ConditionalCompilationArgumentsRaw;
        }

        /// <summary>
        /// Retrieves the developer-defined conditional compilation arguments of a VBA project
        /// </summary>
        /// <remarks>does not expose compiler-defined arguments, such as WIN64, VBA7 etc, which must be determined via the running process</remarks>
        /// <param name="ide">Safe-com wrapper representing the VBE</param>
        /// <param name="projectName">VBA Project name, as declared in the VBE</param>
        /// <returns>returns a Dictionary<string, string>, parsed from the conditional arguments string</returns>
        public static Dictionary<string, string> GetProjectConditionalCompilationArgs(IVBE ide, string projectName)
        {
            using (var typeLibs = new VBETypeLibsAccessor(ide))
            {
                return GetProjectConditionalCompilationArgs(typeLibs.Get(projectName));
            }
        }

        /// <summary>
        /// Retrieves the developer-defined conditional compilation arguments of a VBA project
        /// </summary>
        /// <remarks>does not expose compiler-defined arguments, such as WIN64, VBA7 etc, which must be determined via the running process</remarks>
        /// <param name="project">Safe-com wrapper representing the VBA project</param>
        /// <returns>returns a Dictionary<string, string>, parsed from the conditional arguments string</returns>
        public static Dictionary<string, string> GetProjectConditionalCompilationArgs(IVBProject project)
        {
            using (var typeLib = TypeLibWrapper.FromVBProject(project))
            {
                return GetProjectConditionalCompilationArgs(typeLib);
            }
        }

        /// <summary>
        /// Retrieves the developer-defined conditional compilation arguments of a VBA project
        /// </summary>
        /// <remarks>does not expose compiler-defined arguments, such as WIN64, VBA7 etc, which must be determined via the running process</remarks>
        /// <param name="projectTypeLib">Low-level ITypeLib wrapper representing the VBA project</param>
        /// <returns>returns a Dictionary<string, string>, parsed from the conditional arguments string</returns>
        public static Dictionary<string, string> GetProjectConditionalCompilationArgs(TypeLibWrapper projectTypeLib)
        {
            return projectTypeLib.ConditionalCompilationArguments;
        }

        /// <summary>
        /// Sets the developer-defined conditional compilation arguments of a VBA project
        /// </summary>
        /// <remarks>don't set compiler-defined arguments, such as WIN64, VBA7 etc</remarks>
        /// <param name="ide">Safe-com wrapper representing the VBE</param>
        /// <param name="projectName">VBA Project name, as declared in the VBE</param>
        /// <param name="newConditionalArgs">Raw string representing the arguments, e.g. "foo = 1 : bar = 2"</param>
        public static void SetProjectConditionalCompilationArgsRaw(IVBE ide, string projectName, string newConditionalArgs)
        {
            using (var typeLibs = new VBETypeLibsAccessor(ide))
            {
                SetProjectConditionalCompilationArgsRaw(typeLibs.Get(projectName), newConditionalArgs);
            }
        }

        /// <summary>
        /// Sets the developer-defined conditional compilation arguments of a VBA project
        /// </summary>
        /// <remarks>don't set compiler-defined arguments, such as WIN64, VBA7 etc</remarks>
        /// <param name="project">Safe-com wrapper representing the VBA project</param>
        /// <param name="newConditionalArgs">Raw string representing the arguments, e.g. "foo = 1 : bar = 2"</param>
        public static void SetProjectConditionalCompilationArgsRaw(IVBProject project, string newConditionalArgs)
        {
            using (var typeLib = TypeLibWrapper.FromVBProject(project))
            {
                SetProjectConditionalCompilationArgsRaw(typeLib, newConditionalArgs);
            }
        }

        /// <summary>
        /// Sets the developer-defined conditional compilation arguments of a VBA project
        /// </summary>
        /// <remarks>don't set compiler-defined arguments, such as WIN64, VBA7 etc</remarks>
        /// <param name="projectTypeLib">Low-level ITypeLib wrapper representing the VBA project</param>
        /// <param name="newConditionalArgs">Raw string representing the arguments, e.g. "foo = 1 : bar = 2"</param>
        public static void SetProjectConditionalCompilationArgsRaw(TypeLibWrapper projectTypeLib, string newConditionalArgs)
        {
            projectTypeLib.ConditionalCompilationArgumentsRaw = newConditionalArgs;
        }

        /// <summary>
        /// Sets the developer-defined conditional compilation arguments of a VBA project
        /// </summary>
        /// <remarks>don't set compiler-defined arguments, such as WIN64, VBA7 etc</remarks>
        /// <param name="ide">Safe-com wrapper representing the VBE</param>
        /// <param name="projectName">VBA Project name, as declared in the VBE</param>
        /// <param name="newConditionalArgs">Dictionary<string, string> representing the argument name-value pairs</param>
        public static void SetProjectConditionalCompilationArgs(IVBE ide, string projectName, Dictionary<string, string> newConditionalArgs)
        {
            using (var typeLibs = new VBETypeLibsAccessor(ide))
            {
                SetProjectConditionalCompilationArgs(typeLibs.Get(projectName), newConditionalArgs);
            }
        }

        /// <summary>
        /// Sets the developer-defined conditional compilation arguments of a VBA project
        /// </summary>
        /// <remarks>don't set compiler-defined arguments, such as WIN64, VBA7 etc</remarks>
        /// <param name="project">Safe-com wrapper representing the VBA project</param>
        /// <param name="newConditionalArgs">Dictionary<string, string> representing the argument name-value pairs</param>
        public static void SetProjectConditionalCompilationArgs(IVBProject project, Dictionary<string, string> newConditionalArgs)
        {
            using (var typeLib = TypeLibWrapper.FromVBProject(project))
            {
                SetProjectConditionalCompilationArgs(typeLib, newConditionalArgs);
            }
        }

        /// <summary>
        /// Sets the developer-defined conditional compilation arguments of a VBA project
        /// </summary>
        /// <remarks>don't set compiler-defined arguments, such as WIN64, VBA7 etc</remarks>
        /// <param name="projectTypeLib">Low-level ITypeLib wrapper representing the VBA project</param>
        /// <param name="newConditionalArgs">Dictionary<string, string> representing the argument name-value pairs</param>
        public static void SetProjectConditionalCompilationArgs(TypeLibWrapper projectTypeLib, Dictionary<string, string> newConditionalArgs)
        {
            projectTypeLib.ConditionalCompilationArguments = newConditionalArgs;
        }

        /// <summary>
        /// Determines whether the specified document class is a known document class type (e.g. Excel._Workbook, Access._Form)
        /// </summary>
        /// <param name="ide">Safe-com wrapper representing the VBE</param>
        /// <param name="projectName">VBA Project name, as declared in the VBE</param>
        /// <param name="className">The name of the class document, as defined in the VBA project</param>
        /// <returns>DocClassType indicating the type of the document class module, or DocType.Unrecognized</returns>
        public static DocClassType DetermineDocumentClassType(IVBE ide, string projectName, string className)
        {
            using (var typeLibs = new VBETypeLibsAccessor(ide))
            {
                return DetermineDocumentClassType(typeLibs.Get(projectName), className);
            }
        }

        /// <summary>
        /// Determines whether the specified document class is a known document class type (e.g. Excel._Workbook, Access._Form)
        /// </summary>
        /// <param name="projectTypeLib">Low-level ITypeLib wrapper representing the VBA project</param>
        /// <param name="className">The name of the class document, as defined in the VBA project</param>
        /// <returns>DocClassType indicating the type of the document class module, or DocType.Unrecognized</returns>
        public static DocClassType DetermineDocumentClassType(TypeLibWrapper projectTypeLib, string className)
        {
            return DetermineDocumentClassType(projectTypeLib.TypeInfos.Get(className));
        }

        /// <summary>
        /// Determines whether the specified document class is a known document class type (e.g. Excel._Workbook, Access._Form)
        /// </summary>
        /// <param name="project">Safe-com wrapper representing the VBA project</param>
        /// <param name="className">The name of the class document, as defined in the VBA project</param>
        /// <returns>DocClassType indicating the type of the document class module, or DocType.Unrecognized</returns>
        public static DocClassType DetermineDocumentClassType(IVBProject project, string className)
        {
            using (var typeLib = TypeLibWrapper.FromVBProject(project))
            {
                return DetermineDocumentClassType(typeLib.TypeInfos.Get(className));
            }
        }

        /// <summary>
        /// Determines whether the specified document class is a known document class type (e.g. Excel._Workbook, Access._Form)
        /// </summary>
        /// <param name="project">Safe-com wrapper representing the VBA component</param>
        /// <returns>DocClassType indicating the type of the document class module, or DocType.Unrecognized</returns>
        public static DocClassType DetermineDocumentClassType(IVBComponent component)
        {
            return DetermineDocumentClassType(component.ParentProject, component.Name);
        }

        /// <summary>
        /// Determines whether the specified document class is a known document class type (e.g. Excel._Workbook, Access._Form)
        /// </summary>
        /// <param name="classTypeInfo">Low-level ITypeInfo wrapper representing the VBA project</param>
        /// <returns>DocClassType indicating the type of the document class module, or DocType.Unrecognized</returns>
        public static DocClassType DetermineDocumentClassType(TypeInfoWrapper classTypeInfo)
        {
            return classTypeInfo.DetermineDocumentClassType();
        }

        /// <summary>
        /// Determines whether the specified VBA class implements a specific interface
        /// </summary>
        /// <param name="ide">Safe-com wrapper representing the VBE</param>
        /// <param name="projectName">VBA Project name, as declared in the VBE</param>
        /// <param name="className">Document class name, as declared in the VBA project</param>
        /// <param name="interfaceProgID">The interface name, preceeded by the library container name, e.g. "Excel._Worksheet"</param>
        /// <returns>bool indicating whether the class does inherit the specified interface</returns>
        public static bool DoesClassImplementInterface(IVBE ide, string projectName, string className, string interfaceProgID)
        {
            using (var typeLibs = new VBETypeLibsAccessor(ide))
            {
                return DoesClassImplementInterface(typeLibs.Get(projectName), className, interfaceProgID);
            }
        }

        /// <summary>
        /// Determines whether the specified VBA class implements a specific interface
        /// </summary>
        /// <param name="project">Safe-com wrapper representing the VBE project</param>
        /// <param name="className">Document class name, as declared in the VBA project</param>
        /// <param name="interfaceProgID">The interface name, preceeded by the library container name, e.g. "Excel._Worksheet"</param>
        /// <returns>bool indicating whether the class does inherit the specified interface</returns>
        public static bool DoesClassImplementInterface(IVBProject project, string className, string interfaceProgID)
        {
            using (var typeLib = TypeLibWrapper.FromVBProject(project))
            {
                return DoesClassImplementInterface(typeLib.TypeInfos.Get(className), interfaceProgID);
            }
        }

        /// <summary>
        /// Determines whether the specified VBA class implements a specific interface
        /// </summary>
        /// <param name="projectTypeLib">Low-level ITypeLib wrapper representing the VBA project</param>
        /// <param name="className">Document class name, as declared in the VBA project</param>
        /// <param name="interfaceProgID">The interface name, preceeded by the library container name, e.g. "Excel._Worksheet"</param>
        /// <returns>bool indicating whether the class does inherit the specified interface</returns>
        public static bool DoesClassImplementInterface(TypeLibWrapper projectTypeLib, string className, string interfaceProgID)
        {
            return DoesClassImplementInterface(projectTypeLib.TypeInfos.Get(className), interfaceProgID);
        }

        /// <summary>
        /// Determines whether the specified VBA class implements a specific interface
        /// </summary>
        /// <param name="component">Safe-com wrapper representing the VBA component</param>
        /// <param name="interfaceProgID">The interface name, preceeded by the library container name, e.g. "Excel._Worksheet"</param>
        /// <returns>bool indicating whether the class does inherit the specified interface</returns>
        public static bool DoesClassImplementInterface(IVBComponent component, string interfaceProgID)
        {
            return DoesClassImplementInterface(component.ParentProject, component.Name, interfaceProgID);
        }

        /// <summary>
        /// Determines whether the specified VBA class implements a specific interface
        /// </summary>
        /// <param name="classTypeInfo">Low-level ITypeInfo wrapper representing the VBA project</param>
        /// <param name="interfaceProgID">The interface name, preceeded by the library container name, e.g. "Excel._Worksheet"</param>
        /// <returns>bool indicating whether the class does inherit the specified interface</returns>
        public static bool DoesClassImplementInterface(TypeInfoWrapper classTypeInfo, string interfaceProgID)
        {
            return classTypeInfo.ImplementedInterfaces.DoesImplement(interfaceProgID);
        }

        /// <summary>
        /// Determines whether the specified VBA class implements a specific interface
        /// </summary>
        /// <param name="ide">Safe-com wrapper representing the VBE</param>
        /// <param name="projectName">VBA Project name, as declared in the VBE</param>
        /// <param name="className">Document class name, as declared in the VBA project</param>
        /// <param name="interfaceProgIDs">An array of interface names, preceeded by the library container name, e.g. "Excel._Worksheet"</param>
        /// <param name="matchedIndex">on return indicates the index into interfaceProgIDs that matched, or -1 if no match</param>
        /// <returns>bool indicating whether the class does inherit one of the specified interfaces</returns>
        public static bool DoesClassImplementInterface(IVBE ide, string projectName, string className, string[] interfaceProgIDs, out int matchedIndex)
        {
            using (var typeLibs = new VBETypeLibsAccessor(ide))
            {
                return DoesClassImplementInterface(typeLibs.Get(projectName), className, interfaceProgIDs, out matchedIndex);
            }
        }

        /// <summary>
        /// Determines whether the specified VBA class implements a specific interface
        /// </summary>
        /// <param name="project">Safe-com wrapper representing the VBE project</param>
        /// <param name="className">Document class name, as declared in the VBA project</param>
        /// <param name="interfaceProgIDs">An array of interface names, preceeded by the library container name, e.g. "Excel._Worksheet"</param>
        /// <param name="matchedIndex">on return indicates the index into interfaceProgIDs that matched, or -1 if no match</param>
        /// <returns>bool indicating whether the class does inherit one of the specified interfaces</returns>
        public static bool DoesClassImplementInterface(IVBProject project, string className, string[] interfaceProgIDs, out int matchedIndex)
        {
            using (var typeLib = TypeLibWrapper.FromVBProject(project))
            {
                return DoesClassImplementInterface(typeLib.TypeInfos.Get(className), interfaceProgIDs, out matchedIndex);
            }
        }

        /// <summary>
        /// Determines whether the specified VBA class implements a specific interface
        /// </summary>
        /// <param name="projectTypeLib">Low-level ITypeLib wrapper representing the VBA project</param>
        /// <param name="className">Document class name, as declared in the VBA project</param>
        /// <param name="interfaceProgIDs">An array of interface names, preceeded by the library container name, e.g. "Excel._Worksheet"</param>
        /// <param name="matchedIndex">on return indicates the index into interfaceProgIDs that matched, or -1 if no match</param>
        /// <returns>bool indicating whether the class does inherit one of the specified interfaces</returns>
        public static bool DoesClassImplementInterface(TypeLibWrapper projectTypeLib, string className, string[] interfaceProgIDs, out int matchedIndex)
        {
            return DoesClassImplementInterface(projectTypeLib.TypeInfos.Get(className), interfaceProgIDs, out matchedIndex);
        }

        /// <summary>
        /// Determines whether the specified VBA class implements a specific interface
        /// </summary>
        /// <param name="component">Safe-com wrapper representing the VBA component</param>
        /// <param name="interfaceProgIDs">An array of interface names, preceeded by the library container name, e.g. "Excel._Worksheet"</param>
        /// <param name="matchedIndex">on return indicates the index into interfaceProgIDs that matched, or -1 if no match</param>
        /// <returns>bool indicating whether the class does inherit one of the specified interfaces</returns>
        public static bool DoesClassImplementInterface(IVBComponent component, string[] interfaceProgIDs, out int matchedIndex)
        {
            return DoesClassImplementInterface(component.ParentProject, component.Name, interfaceProgIDs, out matchedIndex);
        }

        /// <summary>
        /// Determines whether the specified VBA class implements a specific interface
        /// </summary>
        /// <param name="classTypeInfo">Low-level ITypeInfo wrapper representing the VBA project</param>
        /// <param name="interfaceProgIDs">An array of interface names, preceeded by the library container name, e.g. "Excel._Worksheet"</param>
        /// <param name="matchedIndex">on return indicates the index into interfaceProgIDs that matched, or -1 if no match</param>
        /// <returns>bool indicating whether the class does inherit one of the specified interfaces</returns>
        public static bool DoesClassImplementInterface(TypeInfoWrapper classTypeInfo, string[] interfaceProgIDs, out int matchedIndex)
        {
            return classTypeInfo.ImplementedInterfaces.DoesImplement(interfaceProgIDs, out matchedIndex);
        }
        
        /// <summary>
        /// Determines whether the specified VBA class implements a specific interface
        /// </summary>
        /// <param name="ide">Safe-com wrapper representing the VBE</param>
        /// <param name="projectName">VBA Project name, as declared in the VBE</param>
        /// <param name="className">Document class name, as declared in the VBA project</param>
        /// <param name="interfaceIID">The interface IID</param>
        /// <returns>bool indicating whether the class does inherit the specified interface</returns>
        public static bool DoesClassImplementInterface(IVBE ide, string projectName, string className, Guid interfaceIID)
        {
            using (var typeLibs = new VBETypeLibsAccessor(ide))
            {
                return DoesClassImplementInterface(typeLibs.Get(projectName), className, interfaceIID);
            }
        }

        /// <summary>
        /// Determines whether the specified VBA class implements a specific interface
        /// </summary>
        /// <param name="project">Safe-com wrapper representing the VBA project</param>
        /// <param name="className">Document class name, as declared in the VBA project</param>
        /// <param name="interfaceIID">The interface IID</param>
        /// <returns>bool indicating whether the class does inherit the specified interface</returns>
        public static bool DoesClassImplementInterface(IVBProject project, string className, Guid interfaceIID)
        {
            using (var typeLib = TypeLibWrapper.FromVBProject(project))
            {
                return DoesClassImplementInterface(typeLib.TypeInfos.Get(className), interfaceIID);
            }
        }

        /// <summary>
        /// Determines whether the specified VBA class implements a specific interface
        /// </summary>
        /// <param name="projectTypeLib">Low-level ITypeLib wrapper representing the VBA project</param>
        /// <param name="className">Document class name, as declared in the VBA project</param>
        /// <param name="interfaceIID">The interface IID</param>
        /// <returns>bool indicating whether the class does inherit the specified interface</returns>
        public static bool DoesClassImplementInterface(TypeLibWrapper projectTypeLib, string className, Guid interfaceIID)
        {
            return DoesClassImplementInterface(projectTypeLib.TypeInfos.Get(className), interfaceIID);
        }

        /// <summary>
        /// Determines whether the specified VBA class implements a specific interface
        /// </summary>
        /// <param name="component">Safe-com wrapper representing the VBA component</param>
        /// <param name="interfaceIID">The interface IID</param>
        /// <returns>bool indicating whether the class does inherit the specified interface</returns>
        public static bool DoesClassImplementInterface(IVBComponent component, Guid interfaceIID)
        {
            return DoesClassImplementInterface(component.ParentProject, component.Name, interfaceIID);
        }

        /// <summary>
        /// Determines whether the specified VBA class implements a specific interface
        /// </summary>
        /// <param name="classTypeInfo">Low-level ITypeInfo wrapper representing the VBA project</param>
        /// <param name="interfaceIID">The interface IID</param>
        /// <returns>bool indicating whether the class does inherit the specified interface</returns>
        public static bool DoesClassImplementInterface(TypeInfoWrapper classTypeInfo, Guid interfaceIID)
        {
            return classTypeInfo.ImplementedInterfaces.DoesImplement(interfaceIID);
        }

        /// <summary>
        /// Determines whether the specified VBA class implements one of several possible interfaces
        /// </summary>
        /// <param name="ide">Safe-com wrapper representing the VBE</param>
        /// <param name="projectName">VBA Project name, as declared in the VBE</param>
        /// <param name="className">Document class name, as declared in the VBA project</param>
        /// <param name="interfaceIIDs">An array of interface IIDs to check against</param>
        /// <param name="matchedIndex">on return indicates the index into interfaceIIDs that matched, or -1 if no match</param>
        /// <returns>bool indicating whether the class does inherit one of the specified interfaces</returns>
        public static bool DoesClassImplementInterface(IVBE ide, string projectName, string className, Guid[] interfaceIIDs, out int matchedIndex)
        {
            using (var typeLibs = new VBETypeLibsAccessor(ide))
            {
                return DoesClassImplementInterface(typeLibs.Get(projectName), className, interfaceIIDs, out matchedIndex);
            }
        }

        /// <summary>
        /// Determines whether the specified VBA class implements one of several possible interfaces
        /// </summary>
        /// <param name="project">Safe-com wrapper representing the VBA project</param>
        /// <param name="className">Document class name, as declared in the VBA project</param>
        /// <param name="interfaceIIDs">An array of interface IIDs to check against</param>
        /// <param name="matchedIndex">on return indicates the index into interfaceIIDs that matched, or -1 if no match</param>
        /// <returns>bool indicating whether the class does inherit one of the specified interfaces</returns>
        public static bool DoesClassImplementInterface(IVBProject project, string className, Guid[] interfaceIIDs, out int matchedIndex)
        {
            using (var typeLib = TypeLibWrapper.FromVBProject(project))
            {
                return DoesClassImplementInterface(typeLib.TypeInfos.Get(className), interfaceIIDs, out matchedIndex);
            }
        }

        /// <summary>
        /// Determines whether the specified VBA class implements one of several possible interfaces
        /// </summary>
        /// <param name="projectTypeLib">Low-level ITypeLib wrapper representing the VBA project</param>
        /// <param name="className">Document class name, as declared in the VBA project</param>
        /// <param name="interfaceIIDs">An array of interface IIDs to check against</param>
        /// <param name="matchedIndex">on return indicates the index into interfaceIIDs that matched, or -1 if no match</param>
        /// <returns>bool indicating whether the class does inherit one of the specified interfaces</returns>
        public static bool DoesClassImplementInterface(TypeLibWrapper projectTypeLib, string className, Guid[] interfaceIIDs, out int matchedIndex)
        {
            return DoesClassImplementInterface(projectTypeLib.TypeInfos.Get(className), interfaceIIDs, out matchedIndex);
        }

        /// <summary>
        /// Determines whether the specified VBA class implements one of several possible interfaces
        /// </summary>
        /// <param name="component">Safe-com wrapper representing the VBA component</param>
        /// <param name="interfaceIIDs">An array of interface IIDs to check against</param>
        /// <param name="matchedIndex">on return indicates the index into interfaceIIDs that matched, or -1 if no match</param>
        /// <returns>bool indicating whether the class does inherit one of the specified interfaces</returns>
        public static bool DoesClassImplementInterface(IVBComponent component, Guid[] interfaceIIDs, out int matchedIndex)
        {
            return DoesClassImplementInterface(component.ParentProject, component.Name, interfaceIIDs, out matchedIndex);
        }

        /// <summary>
        /// Determines whether the specified VBA class implements one of several possible interfaces
        /// </summary>
        /// <param name="classTypeInfo">Low-level ITypeInfo wrapper representing the VBA project</param>
        /// <param name="interfaceIIDs">An array of interface IIDs to check against</param>
        /// <param name="matchedIndex">on return indicates the index into interfaceIIDs that matched, or -1 if no match</param>
        /// <returns>bool indicating whether the class does inherit one of the specified interfaces</returns>
        public static bool DoesClassImplementInterface(TypeInfoWrapper classTypeInfo, Guid[] interfaceIIDs, out int matchedIndex)
        {
            return classTypeInfo.ImplementedInterfaces.DoesImplement(interfaceIIDs, out matchedIndex);
        }

        /// <summary>
        /// Returns the class progID of a control on a UserForm
        /// </summary>
        /// <param name="ide">Safe-com wrapper representing the VBE</param>
        /// <param name="projectName">VBA Project name, as declared in the VBE</param>
        /// <param name="userFormName">UserForm class name, as declared in the VBA project</param>
        /// <param name="controlName">Control name, as declared on the UserForm</param>
        /// <returns>string class progID of the specified control on a UserForm, e.g. "MSForms.CommandButton"</returns>
        public static string GetUserFormControlType(IVBE ide, string projectName, string userFormName, string controlName)
        {
            using (var typeLibs = new VBETypeLibsAccessor(ide))
            {
                return GetUserFormControlType(typeLibs.Get(projectName), userFormName, controlName);
            }
        }

        /// <summary>
        /// Returns the class progID of a control on a UserForm
        /// </summary>
        /// <param name="project">Safe-com wrapper representing the VBA project</param>
        /// <param name="userFormName">UserForm class name, as declared in the VBA project</param>
        /// <param name="controlName">Control name, as declared on the UserForm</param>
        /// <returns>string class progID of the specified control on a UserForm, e.g. "MSForms.CommandButton"</returns>
        public static string GetUserFormControlType(IVBProject project, string userFormName, string controlName)
        {
            using (var typeLib = TypeLibWrapper.FromVBProject(project))
            {
                return GetUserFormControlType(typeLib, userFormName, controlName);
            }
        }

        /// <summary>
        /// Returns the class progID of a control on a UserForm
        /// </summary>
        /// <param name="projectTypeLib">Low-level ITypeLib wrapper representing the VBA project</param>
        /// <param name="userFormName">UserForm class name, as declared in the VBA project</param>
        /// <param name="controlName">Control name, as declared on the UserForm</param>
        /// <returns>string class progID of the specified control on a UserForm, e.g. "MSForms.CommandButton"</returns>
        public static string GetUserFormControlType(TypeLibWrapper projectTypeLib, string userFormName, string controlName)
        {
            return GetUserFormControlType(projectTypeLib.TypeInfos.Get(userFormName), controlName);
        }

        /// <summary>
        /// Returns the class progID of a control on a UserForm
        /// </summary>
        /// <param name="ide">Safe-com wrapper representing the UserForm VBA component</param>
        /// <param name="controlName">Control name, as declared on the UserForm</param>
        /// <returns>string class progID of the specified control on a UserForm, e.g. "MSForms.CommandButton"</returns>
        public static string GetUserFormControlType(IVBComponent component, string controlName)
        {
            return GetUserFormControlType(component.ParentProject, component.Name, controlName);
        }

        /// <summary>
        /// Returns the class progID of a control on a UserForm
        /// </summary>
        /// <param name="userFormTypeInfo">Low-level ITypeLib wrapper representing the UserForm VBA component</param>
        /// <param name="controlName">Control name, as declared on the UserForm</param>
        /// <returns>string class progID of the specified control on a UserForm, e.g. "MSForms.CommandButton"</returns>
        public static string GetUserFormControlType(TypeInfoWrapper userFormTypeInfo, string controlName)
        {
            return userFormTypeInfo.ImplementedInterfaces.Get("FormItf").GetControlType(controlName).GetProgID();
        }
        
        /// <summary>
        /// Returns the class progID of a control on a UserForm
        /// </summary>
        /// <param name="ide">Safe-com wrapper representing the VBE</param>
        /// <param name="projectName">VBA Project name, as declared in the VBE</param>
        /// <param name="documentClassName">Document class name, as declared in the VBA project</param>
        /// <param name="controlName">Control name, as declared on the UserForm</param>
        /// <returns>string class progID of the specified control on a UserForm, e.g. "MSForms.CommandButton"</returns>
        public static string GetDocumentClassControlType(IVBE ide, string projectName, string documentClassName, string controlName)
        {
            using (var typeLibs = new VBETypeLibsAccessor(ide))
            {
                return GetDocumentClassControlType(typeLibs.Get(projectName), documentClassName, controlName);
            }
        }

        /// <summary>
        /// Returns the class progID of a control on a UserForm
        /// </summary>
        /// <param name="project">Safe-com wrapper representing the VBA project</param>
        /// <param name="documentClassName">Document class name, as declared in the VBA project</param>
        /// <param name="controlName">Control name, as declared on the UserForm</param>
        /// <returns>string class progID of the specified control on a UserForm, e.g. "MSForms.CommandButton"</returns>
        public static string GetDocumentClassControlType(IVBProject project, string documentClassName, string controlName)
        {
            using (var typeLib = TypeLibWrapper.FromVBProject(project))
            {
                return GetDocumentClassControlType(typeLib, documentClassName, controlName);
            }
        }

        /// <summary>
        /// Returns the class progID of a control on a UserForm
        /// </summary>
        /// <param name="projectTypeLib">Low-level ITypeLib wrapper representing the VBA project</param>
        /// <param name="documentClassName">Document class name, as declared in the VBA project</param>
        /// <param name="controlName">Control name, as declared on the UserForm</param>
        /// <returns>string class progID of the specified control on a UserForm, e.g. "MSForms.CommandButton"</returns>
        public static string GetDocumentClassControlType(TypeLibWrapper projectTypeLib, string documentClassName, string controlName)
        {
            return GetDocumentClassControlType(projectTypeLib.TypeInfos.Get(documentClassName), controlName);
        }

        /// <summary>
        /// Returns the class progID of a control on a UserForm
        /// </summary>
        /// <param name="ide">Safe-com wrapper representing the UserForm VBA component</param>
        /// <param name="controlName">Control name, as declared on the UserForm</param>
        /// <returns>string class progID of the specified control on a UserForm, e.g. "MSForms.CommandButton"</returns>
        public static string GetDocumentClassControlType(IVBComponent component, string controlName)
        {
            return GetDocumentClassControlType(component.ParentProject, component.Name, controlName);
        }

        /// <summary>
        /// Returns the class progID of a control on a UserForm
        /// </summary>
        /// <param name="documentClass">Low-level ITypeLib wrapper representing the UserForm VBA component</param>
        /// <param name="controlName">Control name, as declared on the UserForm</param>
        /// <returns>string class progID of the specified control on a UserForm, e.g. "MSForms.CommandButton"</returns>
        public static string GetDocumentClassControlType(TypeInfoWrapper documentClass, string controlName)
        {
            return documentClass.GetSafeImplementedTypeInfo(0).GetControlType(controlName).GetProgID();
        }

        /// <summary>
        /// Retreives the TYPEFLAGS of a VBA component (e.g. module/class), providing flags like TYPEFLAG_FCANCREATE, TYPEFLAG_FPREDECLID
        /// </summary>
        /// <param name="ide">Safe-com wrapper representing the VBE</param>
        /// <param name="projectName">The VBA project name</param>
        /// <param name="componentName">The name of the component (module/class etc) to get flags for</param>
        /// <returns>ComTypes.TYPEFLAGS flags from the ITypeInfo</returns>
        public static ComTypes.TYPEFLAGS GetComponentTypeFlags(IVBE ide, string projectName, string componentName)
        {
            using (var typeLibs = new VBETypeLibsAccessor(ide))
            {
                return GetComponentTypeFlags(typeLibs.Get(projectName), componentName);
            }
        }

        /// <summary>
        /// Retreives the TYPEFLAGS of a VBA component (e.g. module/class), providing flags like TYPEFLAG_FCANCREATE, TYPEFLAG_FPREDECLID
        /// </summary>
        /// <param name="project">Safe-com wrapper representing the VBA project</param>
        /// <param name="componentName">The name of the component (module/class etc) to get flags for</param>
        /// <returns>ComTypes.TYPEFLAGS flags from the ITypeInfo</returns>
        public static ComTypes.TYPEFLAGS GetComponentTypeFlags(IVBProject project, string componentName)
        {
            using (var typeLib = TypeLibWrapper.FromVBProject(project))
            {
                return GetComponentTypeFlags(typeLib, componentName);
            }
        }

        /// <summary>
        /// Retreives the TYPEFLAGS of a VBA component (e.g. module/class), providing flags like TYPEFLAG_FCANCREATE, TYPEFLAG_FPREDECLID
        /// </summary>
        /// <param name="projectTypeLib">Low-level ITypeLib wrapper representing the VBA project</param>
        /// <param name="componentName">The name of the component (module/class etc) to get flags for</param>
        /// <returns>ComTypes.TYPEFLAGS flags from the ITypeInfo</returns>
        public static ComTypes.TYPEFLAGS GetComponentTypeFlags(TypeLibWrapper projectTypeLib, string componentName)
        {
            return GetComponentTypeFlags(projectTypeLib.TypeInfos.Get(componentName));
        }

        /// <summary>
        /// Retreives the TYPEFLAGS of a VBA component (e.g. module/class), providing flags like TYPEFLAG_FCANCREATE, TYPEFLAG_FPREDECLID
        /// </summary>
        /// <param name="component">Safe-com wrapper representing the VBA component to get flags for</param>
        /// <returns>ComTypes.TYPEFLAGS flags from the ITypeInfo</returns>
        public static ComTypes.TYPEFLAGS GetComponentTypeFlags(IVBComponent component)
        {
            return GetComponentTypeFlags(component.ParentProject, component.Name);
        }

        /// <summary>
        /// Retreives the TYPEFLAGS of a VBA component (e.g. module/class), providing flags like TYPEFLAG_FCANCREATE, TYPEFLAG_FPREDECLID
        /// </summary>
        /// <param name="componentTypeInfo">Low-level ITypeInfo wrapper representing the VBA component to get flags for</param>
        /// <returns>ComTypes.TYPEFLAGS flags from the ITypeInfo</returns>
        public static ComTypes.TYPEFLAGS GetComponentTypeFlags(TypeInfoWrapper componentTypeInfo)
        {
            return componentTypeInfo.Flags;
        }

        /// <summary>
        /// Returns a TypeInfoReference object containing information about the specified VBA project reference
        /// </summary>
        /// <param name="ide">Safe-com wrapper representing the VBE</param>
        /// <param name="projectName">VBA Project name, as declared in the VBE</param>
        /// <param name="referenceIdx">Index into the references collection</param>
        /// <returns>TypeInfoReference containing information about the specified VBA project reference</returns>
        public static TypeInfoReference GetReferenceInfo(IVBE ide, string projectName, int referenceIdx)
        {
            using (var typeLibs = new VBETypeLibsAccessor(ide))
            {
                return GetReferenceInfo(typeLibs.Get(projectName), referenceIdx);
            }
        }

        /// <summary>
        /// Returns a TypeInfoReference object containing information about the specified VBA project reference
        /// </summary>
        /// <param name="project">Safe-com wrapper representing the VBA project</param>
        /// <param name="referenceIdx">Index into the references collection</param>
        /// <returns>TypeInfoReference containing information about the specified VBA project reference</returns>
        public static TypeInfoReference GetReferenceInfo(IVBProject project, int referenceIdx)
        {
            using (var typeLib = TypeLibWrapper.FromVBProject(project))
            {
                return GetReferenceInfo(typeLib, referenceIdx);
            }
        }

        /// <summary>
        /// Returns a TypeInfoReference object containing information about the specified VBA project reference
        /// </summary>
        /// <param name="projectTypeLib">Low-level ITypeLib wrapper representing the VBA project</param>
        /// <param name="referenceIdx">Index into the references collection</param>
        /// <returns>TypeInfoReference containing information about the specified VBA project reference</returns>
        public static TypeInfoReference GetReferenceInfo(TypeLibWrapper projectTypeLib, int referenceIdx)
        {
            return projectTypeLib.GetVBEReferenceByIndex(referenceIdx);
        }

        /// <summary>
        /// Returns a TypeInfoReference object containing information about the specified VBA project reference
        /// </summary>
        /// <param name="project">Safe-com wrapper representing the VBA project</param>
        /// <param name="vbeReference">Safe-com wrapper representing the VBA project reference</param>
        /// <returns>TypeInfoReference containing information about the specified VBA project reference</returns>
        public static TypeInfoReference GetReferenceInfo(IVBProject project, IReference vbeReference)
        {
            using (var typeLib = TypeLibWrapper.FromVBProject(project))
            {
                return typeLib.GetVBEReferenceByGuid(Guid.Parse(vbeReference.Guid));
            }
        }

        /// <summary>
        /// Documents the type libaries of all loaded VBA projects
        /// </summary>
        /// <param name="ide">Safe-com wrapper representing the VBE</param>
        /// <returns>text document, in a non-standard format, useful for debugging purposes</returns>
        public static string DocumentAll(IVBE ide)
        {
            using (var typeLibs = new VBETypeLibsAccessor(ide))
            {
                var output = new StringLineBuilder();

                foreach (var typeLib in typeLibs)
                {
                    typeLib.Document(output);
                }

                return output.ToString();
            }
        }

        /// <summary>
        /// Documents the type libary of a single VBA project
        /// </summary>
        /// <param name="project">Safe-com wrapper representing the VBA project</param>
        /// <returns>text document, in a non-standard format, useful for debugging purposes</returns>
        public static string DocumentAll(IVBProject project)
        {
            using (var typeLib = TypeLibWrapper.FromVBProject(project))
            {
                return DocumentAll(typeLib);
            }
        }

        /// <summary>
        /// Documents the type libary of a single VBA project
        /// </summary>
        /// <param name="projectTypeLib">Low-level ITypeLib wrapper representing the VBA project</param>
        /// <returns>text document, in a non-standard format, useful for debugging purposes</returns>
        public static string DocumentAll(TypeLibWrapper projectTypeLib)
        {
            var output = new StringLineBuilder();
            projectTypeLib.Document(output);
            return output.ToString();
        }
    }
}
