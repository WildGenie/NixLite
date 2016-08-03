using System;

namespace Nix
{
    sealed public partial class NixLite
    {

        public uint InjectDllCreateThread(string szDllPath)
        {
            if (!m_bProcessOpen)
                return RETURN_ERROR;

            return SInject.InjectDllCreateThread(m_hProcess, szDllPath);
        }

        public uint InjectDllRedirectThread(string szDllPath)
        {
            if (!m_bProcessOpen)
                return RETURN_ERROR;

            if (m_bThreadOpen)
                return SInject.InjectDllRedirectThread(m_hProcess, m_hThread, szDllPath);

            return SInject.InjectDllRedirectThread(m_hProcess, m_ProcessId, szDllPath);
        }

        public uint InjectDllRedirectThread(IntPtr hThread, string szDllPath)
        {
            if (!m_bProcessOpen)
                return RETURN_ERROR;

            return SInject.InjectDllRedirectThread(m_hProcess, hThread, szDllPath);
        }
    }
}
