using System;
using System.Diagnostics;
using System.Collections.Generic;
using Fasm;

namespace Nix
{
    sealed public partial class NixLite
    {
        #region Variables

        private const uint RETURN_ERROR = 0;

        public bool SetDebugPrivileges = true;

        private bool m_bProcessOpen = false;

        public bool IsProcessOpen { get { return m_bProcessOpen; } }

        private bool m_bThreadOpen = false;

        public bool IsThreadOpen { get { return m_bThreadOpen; } }

        private IntPtr m_hProcess = IntPtr.Zero;

        public IntPtr ProcessHandle { get { return m_hProcess; } }

        private int m_ProcessId = 0;

        public int ProcessId { get { return m_ProcessId; } }

        private IntPtr m_hWnd = IntPtr.Zero;

        public IntPtr WindowHandle { get { return m_hWnd; } }

        private int m_ThreadId = 0;

        public int ThreadId { get { return m_ThreadId; } }

        private IntPtr m_hThread = IntPtr.Zero;

        public IntPtr ThreadHandle { get { return m_hThread; } }

        private ProcessModule m_MainModule;

        public ProcessModule MainModule { get { return m_MainModule; } }

        private ProcessModuleCollection m_Modules;

        public ProcessModuleCollection Modules { get { return m_Modules; } }

        public ManagedFasm Asm { get; set; }
        #endregion

        #region Constructors

        public NixLite()
        {
            Asm = new ManagedFasm();
            m_Data = new List<PatternDataEntry>();

            if (m_bProcessOpen && m_hProcess != IntPtr.Zero)
                Asm.SetProcessHandle(m_hProcess);
        }

        public NixLite(int ProcessId) : this()
        {
            m_bProcessOpen = Open(ProcessId);
        }

        public NixLite(IntPtr WindowHandle) : this(SProcess.GetProcessFromWindow(WindowHandle))
        { }
        #endregion

        #region Destructor

        ~NixLite()
        {
            this.Close();
        }

        #endregion

        #region Open

        public bool Open(int ProcessId)
        {
            if (ProcessId == 0)
                return false;

            if (ProcessId == m_ProcessId)
                return true;

            if (m_bProcessOpen)
                this.CloseProcess();

            if (SetDebugPrivileges)
                System.Diagnostics.Process.EnterDebugMode();

            m_bProcessOpen = (m_hProcess = SProcess.OpenProcess(ProcessId)) != IntPtr.Zero;

            if (m_bProcessOpen)
            {
                m_ProcessId = ProcessId;
                m_hWnd = SWindow.FindWindowByProcessId(ProcessId);

                m_Modules = Process.GetProcessById(m_ProcessId).Modules;
                m_MainModule = m_Modules[0];

                if (Asm == null)
                    Asm = new ManagedFasm(m_hProcess);
                else
                    Asm.SetProcessHandle(m_hProcess);
            }

            return m_bProcessOpen;
        }

        public bool Open(IntPtr WindowHandle)
        {
            if (WindowHandle == IntPtr.Zero)
                return false;

            return this.Open(SProcess.GetProcessFromWindow(WindowHandle));
        }

        public bool OpenThread(int dwThreadId)
        {
            if (dwThreadId == 0)
                return false;

            if (dwThreadId == m_ThreadId)
                return true;

            if (m_bThreadOpen)
                this.CloseThread();

            m_bThreadOpen = (m_hThread = SThread.OpenThread(dwThreadId)) != IntPtr.Zero;

            if (m_bThreadOpen)
                m_ThreadId = dwThreadId;

            return m_bThreadOpen;
        }

        public bool OpenThread()
        {
            if (m_bProcessOpen)
                return this.OpenThread(SThread.GetMainThreadId(m_ProcessId));
            return false;
        }

        public bool OpenProcessAndThread(int dwProcessId)
        {
            if (this.Open(dwProcessId) && this.OpenThread())
                return true;

            this.Close();
            return false;
        }

        public bool OpenProcessAndThread(IntPtr WindowHandle)
        {
            if (this.Open(WindowHandle) && this.OpenThread())
                return true;

            this.Close();
            return false;
        }
        #endregion

        #region Close

        public void Close()
        {
            Asm.Dispose();

            this.CloseProcess();
            this.CloseThread();
        }

        public void CloseProcess()
        {
            if (m_hProcess != IntPtr.Zero)
                Imports.CloseHandle(m_hProcess);

            m_hProcess = IntPtr.Zero;
            m_hWnd = IntPtr.Zero;
            m_ProcessId = 0;

            m_MainModule = null;
            m_Modules = null;

            m_bProcessOpen = false;

            Asm.SetProcessHandle(IntPtr.Zero);
        }

        public void CloseThread()
        {
            if (m_hThread != IntPtr.Zero)
                Imports.CloseHandle(m_hThread);

            m_hThread = IntPtr.Zero;
            m_ThreadId = 0;

            m_bThreadOpen = false;
        }
        #endregion

        #region Modules

        public string GetModuleFilePath()
        {
            return m_MainModule.FileName;
        }

        public string GetModuleFilePath(int index)
        {
            return m_Modules[index].FileName;
        }

        public string GetModuleFilePath(string sModuleName)
        {
            foreach (ProcessModule pMod in m_Modules)
                if (pMod.ModuleName.ToLower().Equals(sModuleName.ToLower()))
                    return pMod.FileName;

            return String.Empty;
        }

        public ProcessModule GetModule(string sModuleName)
        {
            foreach (ProcessModule pMod in m_Modules)
                if (pMod.ModuleName.ToLower().Equals(sModuleName.ToLower()))
                    return pMod;

            return null;
        }

        public ProcessModule GetModule(uint dwAddress)
        {
            foreach (ProcessModule pMod in m_Modules)
                if ((uint)pMod.BaseAddress <= dwAddress && ((uint)pMod.BaseAddress + pMod.ModuleMemorySize) >= dwAddress)
                    return pMod;

            return null;
        }
        #endregion
    }
}
