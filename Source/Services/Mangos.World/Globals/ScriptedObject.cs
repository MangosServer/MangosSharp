// 
// Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
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

using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using Mangos.Common.Enums.Global;
using Microsoft.VisualBasic;

namespace Mangos.World.Globals
{

    // NOTE: How to use ScriptedObject as Function
    // Dim test As New ScriptedObject("scripts\test.vb", "test.dll")
    // test.Invoke(".TestScript", "TestMeSub")
    // x = test.Invoke(".TestScript", "TestMeFunction")
    // NOTE: How to use ScriptedObject as Constructor
    // creature = test.Invoke("DefaultAI_1")
    // creature.Move()

    public class ScriptedObject : IDisposable
    {
        public Assembly ass;

        public ScriptedObject()
        {
            var LastDate = default(DateTime);
            string AssemblyFile = "Mangos.Scripts.dll";
            var AssemblySources = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\Scripts\", "*.vb", SearchOption.AllDirectories);
            foreach (string Source in AssemblySources)
            {
                if (LastDate < FileSystem.FileDateTime(Source))
                {
                    LastDate = FileSystem.FileDateTime(Source);
                }
            }

            if (!string.IsNullOrEmpty(FileSystem.Dir(AssemblyFile)) && LastDate < FileSystem.FileDateTime(AssemblyFile))
            {
                // DONE: We have latest source compiled already
                LoadAssemblyObject(AssemblyFile);
                return;
            }

            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.SUCCESS, @"Compiling: \Scripts\*.*");
            try
            {
                var vBcp = new VBCodeProvider();
                // Dim CScp As New Microsoft.CSharp.CSharpCodeProvider

                var cParameters = new CompilerParameters();
                CompilerResults cResults;
                foreach (string include in WorldServiceLocator._WorldServer.Config.CompilerInclude)
                    cParameters.ReferencedAssemblies.Add(include);
                cParameters.OutputAssembly = AssemblyFile;
                cParameters.ReferencedAssemblies.Add(AppDomain.CurrentDomain.FriendlyName);
                cParameters.GenerateExecutable = false;
                cParameters.GenerateInMemory = false;
                /* TODO ERROR: Skipped IfDirectiveTrivia */
                cParameters.IncludeDebugInformation = true;
                /* TODO ERROR: Skipped ElseDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
                cResults = vBcp.CompileAssemblyFromFile(cParameters, AssemblySources);
                if (cResults.Errors.HasErrors == true)
                {
                    foreach (CompilerError err in cResults.Errors)
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Compiling: Error on line {1} in {3}:{0}{2}", Environment.NewLine, err.Line, err.ErrorText, err.FileName);
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

        public ScriptedObject(string AssemblySourceFile, string AssemblyFile, bool InMemory)
        {
            if (!InMemory && !string.IsNullOrEmpty(FileSystem.Dir(AssemblyFile)) && FileSystem.FileDateTime(AssemblySourceFile) < FileSystem.FileDateTime(AssemblyFile))
            {
                // DONE: We have latest source compiled already
                LoadAssemblyObject(AssemblyFile);
                // Dim ass As [Assembly] = [Assembly].LoadFrom("test.dll")
                return;
            }

            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.SUCCESS, "Compiling: {0}", AssemblySourceFile);
            try
            {
                var VBcp = new VBCodeProvider();
                var CScp = new Microsoft.CSharp.CSharpCodeProvider();
                var cParameters = new CompilerParameters();
                CompilerResults cResults;
                if (!InMemory)
                    cParameters.OutputAssembly = AssemblyFile;
                foreach (string Include in WorldServiceLocator._WorldServer.Config.CompilerInclude)
                    cParameters.ReferencedAssemblies.Add(Include);
                cParameters.ReferencedAssemblies.Add(AppDomain.CurrentDomain.FriendlyName);
                cParameters.GenerateExecutable = false;     // result is a .DLL
                cParameters.GenerateInMemory = InMemory;
                /* TODO ERROR: Skipped IfDirectiveTrivia */
                cParameters.IncludeDebugInformation = true;
                /* TODO ERROR: Skipped ElseDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
                if (AssemblySourceFile.IndexOf(".cs") != -1)
                {
                    cResults = CScp.CompileAssemblyFromFile(cParameters, AppDomain.CurrentDomain.BaseDirectory + AssemblySourceFile);
                }
                else if (AssemblySourceFile.IndexOf(".vb") != -1)
                {
                    cResults = VBcp.CompileAssemblyFromFile(cParameters, AppDomain.CurrentDomain.BaseDirectory + AssemblySourceFile);
                }
                else
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Compiling: Unsupported file type: {0}", AssemblySourceFile);
                    return;
                }

                if (cResults.Errors.HasErrors == true)
                {
                    foreach (CompilerError err in cResults.Errors)
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Compiling: Error on line {1}:{0}{2}", Environment.NewLine, err.Line, err.ErrorText);
                }
                else
                {
                    ass = cResults.CompiledAssembly;
                }
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
            catch (TargetInvocationException e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Script execution error:{1}{0}", e.GetBaseException().ToString(), Environment.NewLine);
            }
            catch (Exception)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Script Method [{0}] not found in [Scripts.{1}]!", MyMethod, MyModule);
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
            catch (NullReferenceException)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Scripted Class [{0}] not found in [Scripts]!", MyBaseClass);
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
            catch (NullReferenceException)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Scripted Property [{1}] not found in [Scripts.{1}]!", MyModule, MyProperty);
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
                var fi = ty.GetField(MyField, (BindingFlags)((int)BindingFlags.Public + (int)BindingFlags.Static));
                return fi.GetValue(null);
            }
            catch (NullReferenceException)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Scripted Field [{1}] not found in [Scripts.{0}]!", MyModule, MyField);
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
            if (mi is null)
                return false;
            else
                return true;
        }

        // Load an already compiled script.
        public void LoadAssemblyObject(string dllLocation)
        {
            try
            {
                ass = Assembly.LoadFrom(dllLocation);
            }
            catch (FileNotFoundException fnfe)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DLL not found error:{1}{0}", fnfe.GetBaseException().ToString(), Environment.NewLine);
            }
            catch (ArgumentNullException ane)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DLL NULL error:{1}{0}", ane.GetBaseException().ToString(), Environment.NewLine);
            }
            catch (BadImageFormatException bife)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DLL not a valid assembly error:{1}{0}", bife.GetBaseException().ToString(), Environment.NewLine);
            }
        }

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        private bool _disposedValue; // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
            }

            _disposedValue = true;
        }

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}