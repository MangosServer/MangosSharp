//
// Copyright (C) 2013-2022 getMaNGOS <https://getmangos.eu>
//
// This program is free software. You can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation. either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY. Without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using Mangos.Common.Enums.Global;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Mangos.World.Globals;

public class ScriptedObject : IDisposable
{
    public Assembly ass;

    private bool _disposedValue;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public ScriptedObject()
    {
        var AssemblyFile = "Mangos.World.Scripts.dll";
        var AssemblySources = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\Scripts\\", "*.cs", SearchOption.AllDirectories);
        var AssemblySources2 = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\Scripts\\", "*.vb", SearchOption.AllDirectories);
        DateTime LastDate = default;
        foreach (var Source in AssemblySources)
        {
            if (DateTime.Compare(LastDate, FileSystem.FileDateTime(Source)) < 0)
            {
                LastDate = FileSystem.FileDateTime(Source);
            }
        }
        foreach (var Source in AssemblySources2)
        {
            if (DateTime.Compare(LastDate, FileSystem.FileDateTime(Source)) < 0)
            {
                LastDate = FileSystem.FileDateTime(Source);
            }
        }
        if (Operators.CompareString(Path.GetFileName(AssemblyFile), "", TextCompare: false) != 0 && DateTime.Compare(LastDate, FileSystem.FileDateTime(AssemblyFile)) < 0)
        {
            LoadAssemblyObject(AssemblyFile);
            return;
        }
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.SUCCESS, "Compiling: \\Scripts\\*.*");
        try
        {
            CSharpCodeProvider CSCP = new();
            VBCodeProvider VBCP = new();
            CompilerParameters cParameters = new();
            IEnumerator enumerator = default;
            try
            {
                enumerator = WorldServiceLocator._ConfigurationProvider.GetConfiguration().CompilerInclude.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var include = Conversions.ToString(enumerator.Current);
                    cParameters.ReferencedAssemblies.Add(include);
                }
            }
            finally
            {
                if (enumerator is IDisposable)
                {
                    (enumerator as IDisposable).Dispose();
                }
            }
            cParameters.OutputAssembly = AssemblyFile;
            cParameters.ReferencedAssemblies.Add(AppDomain.CurrentDomain.FriendlyName);
            cParameters.GenerateExecutable = false;
            cParameters.GenerateInMemory = false;
            cParameters.IncludeDebugInformation = true;
            var cResults = CSCP.CompileAssemblyFromFile(cParameters, AssemblySources);
            var cResults2 = VBCP.CompileAssemblyFromFile(cParameters, AssemblySources2);
            if (cResults.Errors.HasErrors)
            {
                IEnumerator enumerator2 = default;
                try
                {
                    enumerator2 = cResults.Errors.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        CompilerError err = (CompilerError)enumerator2.Current;
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Compiling: Error on line {1} in {3}:{0}{2}", Environment.NewLine, err.Line, err.ErrorText, err.FileName);
                    }
                }
                finally
                {
                    if (enumerator2 is IDisposable)
                    {
                        (enumerator2 as IDisposable).Dispose();
                    }
                }
            }
            else
            {
                ass = cResults.CompiledAssembly;
            }
        }
        catch (Exception e)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Unable to compile scripts. {1}{0}", e.ToString(), Environment.NewLine);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public ScriptedObject(string AssemblySourceFile, string AssemblyFile, bool InMemory)
    {
        if (!InMemory && Operators.CompareString(Path.GetFileName(AssemblyFile), "", TextCompare: false) != 0 && DateTime.Compare(FileSystem.FileDateTime(AssemblySourceFile), FileSystem.FileDateTime(AssemblyFile)) < 0)
        {
            LoadAssemblyObject(AssemblyFile);
            return;
        }
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.SUCCESS, "Compiling: {0}", AssemblySourceFile);
        try
        {
            CSharpCodeProvider CSCP = new();
            VBCodeProvider VBCP = new();
            CompilerParameters cParameters = new();
            if (!InMemory)
            {
                cParameters.OutputAssembly = AssemblyFile;
            }
            IEnumerator enumerator = default;
            try
            {
                enumerator = WorldServiceLocator._ConfigurationProvider.GetConfiguration().CompilerInclude.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var Include = Conversions.ToString(enumerator.Current);
                    cParameters.ReferencedAssemblies.Add(Include);
                }
            }
            finally
            {
                if (enumerator is IDisposable)
                {
                    (enumerator as IDisposable).Dispose();
                }
            }
            cParameters.ReferencedAssemblies.Add(AppDomain.CurrentDomain.FriendlyName);
            cParameters.GenerateExecutable = false;
            cParameters.GenerateInMemory = InMemory;
            cParameters.IncludeDebugInformation = true;
            CompilerResults cResults;
            if (AssemblySourceFile.IndexOf(".cs") != -1)
            {
                cResults = CSCP.CompileAssemblyFromFile(cParameters, AppDomain.CurrentDomain.BaseDirectory + AssemblySourceFile);
                goto IL_01b5;
            }
            if (AssemblySourceFile.IndexOf(".vb") != -1)
            {
                cResults = VBCP.CompileAssemblyFromFile(cParameters, AppDomain.CurrentDomain.BaseDirectory + AssemblySourceFile);
                goto IL_01b5;
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Compiling: Unsupported file type: {0}", AssemblySourceFile);
            goto end_IL_0068;
        IL_01b5:
            if (cResults.Errors.HasErrors)
            {
                IEnumerator enumerator2 = default;
                try
                {
                    enumerator2 = cResults.Errors.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        CompilerError err = (CompilerError)enumerator2.Current;
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Compiling: Error on line {1}:{0}{2}", Environment.NewLine, err.Line, err.ErrorText);
                    }
                }
                finally
                {
                    if (enumerator2 is IDisposable)
                    {
                        (enumerator2 as IDisposable).Dispose();
                    }
                }
            }
            else
            {
                ass = cResults.CompiledAssembly;
            }
        end_IL_0068:
            ;
        }
        catch (Exception e)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Unable to compile script [{0}]. {2}{1}", AssemblySourceFile, e.ToString(), Environment.NewLine);
        }
    }

    public void InvokeFunction(string MyModule, string MyMethod, object Parameters = null)
    {
        try
        {
            var ty = ass.GetType("Scripts." + MyModule);
            var mi = ty.GetMethod(MyMethod);
            mi.Invoke(null, (object[])Parameters);
        }
        catch (TargetInvocationException e2)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Script execution error:{1}{0}", e2.GetBaseException().ToString(), Environment.NewLine);
        }
        catch (Exception ex2)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Script Method [{0}] not found in [Scripts.{1}]!", MyMethod, MyModule, ex2);
        }
    }

    public object InvokeConstructor(string MyBaseClass, object Parameters = null)
    {
        try
        {
            var ty = ass.GetType("Scripts." + MyBaseClass);
            var ci = ty.GetConstructors();
            return ci[0].Invoke((object[])Parameters);
        }
        catch (NullReferenceException ex)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Scripted Class [{0}] not found in [Scripts]!", MyBaseClass, ex);
        }
        catch (Exception e)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Script execution error:{1}{0}", e.GetBaseException().ToString(), Environment.NewLine);
        }
        return null;
    }

    public object InvokeProperty(string MyModule, string MyProperty)
    {
        try
        {
            var ty = ass.GetType("Scripts." + MyModule);
            var pi = ty.GetProperty(MyProperty);
            return pi.GetValue(null, null);
        }
        catch (NullReferenceException ex)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Scripted Property [{1}] not found in [Scripts.{1}]!", MyModule, MyProperty, ex);
        }
        catch (Exception e)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Script execution error:{1}{0}", e.GetBaseException().ToString(), Environment.NewLine);
        }
        return null;
    }

    public object InvokeField(string MyModule, string MyField)
    {
        try
        {
            var ty = ass.GetType("Scripts." + MyModule);
            var fi = ty.GetField(MyField, BindingFlags.Static | BindingFlags.Public);
            return fi.GetValue(null);
        }
        catch (NullReferenceException ex)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Scripted Field [{1}] not found in [Scripts.{0}]!", MyModule, MyField, ex);
        }
        catch (Exception e)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Script execution error:{1}{0}", e.GetBaseException().ToString(), Environment.NewLine);
        }
        return null;
    }

    public bool ContainsMethod(string MyModule, string MyMethod)
    {
        var ty = ass.GetType("Scripts." + MyModule);
        var mi = ty.GetMethod(MyMethod);
        return mi is not null;
    }

    public void LoadAssemblyObject(string dllLocation)
    {
        try
        {
            ass = Assembly.LoadFrom(dllLocation);
        }
        catch (FileNotFoundException ex)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DLL not found error:{1}{0}", ex.GetBaseException().ToString(), Environment.NewLine);
        }
        catch (ArgumentNullException ex2)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DLL NULL error:{1}{0}", ex2.GetBaseException().ToString(), Environment.NewLine);
        }
        catch (BadImageFormatException ex3)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DLL not a valid assembly error:{1}{0}", ex3.GetBaseException().ToString(), Environment.NewLine);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
        }
        _disposedValue = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    void IDisposable.Dispose()
    {
        //ILSpy generated this explicit interface implementation from .override directive in Dispose
        Dispose();
    }
}
