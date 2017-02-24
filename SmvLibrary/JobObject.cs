﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SmvLibrary
{
    class JobObject : IDisposable
    {

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr CreateJobObject(IntPtr a, string lpName);
        [DllImport("kernel32.dll")]
        static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInfoType jobType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hHandle);
        [DllImport("kernel32.dll")]
        static extern uint GetLastError();
        [DllImport("kernel32.dll")]
        static extern bool QueryInformationJobObject(IntPtr hJob, JobObjectInfoType JobObjectInformationClass, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength, IntPtr lpReturnLength);

        public IntPtr handle { get; set; }
        private bool disposed = false;

        /// <summary>
        /// Dispose the job object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose and close the job object
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing){ }

            Close();
            disposed = true;
        }

        /// <summary>
        /// Close the job object handle
        /// </summary>
        public void Close()
        {
            CloseHandle(handle);
            handle = IntPtr.Zero;
        }

        /// <summary>
        /// Add process to job object using handle
        /// </summary>
        /// <param name="processHandle"></param>
        /// <returns></returns>
        public bool AddProcess(IntPtr processHandle)
        {
            return AssignProcessToJobObject(handle, processHandle);
        }

        /// <summary>
        /// Add process to job object using processId
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        public bool AddProcess(int processId)
        {
            return AddProcess(Process.GetProcessById(processId).Handle);
        }

        /// <summary>
        /// Creates job object with ProcessMemoryLimit as maxMemory
        /// </summary>
        /// <param name="maxMemory"></param>
        public void setMaxMemory(int maxMemory)
        {
            handle = CreateJobObject(IntPtr.Zero, null);

            JOBOBJECT_BASIC_LIMIT_INFORMATION info = new JOBOBJECT_BASIC_LIMIT_INFORMATION();
            info.LimitFlags = 0x100;
            JOBOBJECT_EXTENDED_LIMIT_INFORMATION extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION();
            extendedInfo.BasicLimitInformation = info;
            extendedInfo.ProcessMemoryLimit = new UIntPtr((uint)maxMemory);
            int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
            IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

            if (!SetInformationJobObject(handle, JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (uint)length))
            {
                throw new Exception("Cannot set object to extended limit info class " + GetLastError());
            }

        }

        /// <summary>
        /// Prints the PeakProcessMemoryUsed
        /// </summary>
        public void QueryExtendedLimitInformation()
        {

            JOBOBJECT_EXTENDED_LIMIT_INFORMATION extendedLimit;
            int extenedLimitLength = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
            IntPtr extendedLimitPtr = Marshal.AllocHGlobal(extenedLimitLength);
            try
            {
                bool success = QueryInformationJobObject(this.handle, JobObjectInfoType.ExtendedLimitInformation, extendedLimitPtr, (uint)extenedLimitLength, IntPtr.Zero);
                if (success == false)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(error, "QueryInformationJobObject failed.");
                }
                extendedLimit = (JOBOBJECT_EXTENDED_LIMIT_INFORMATION)Marshal.PtrToStructure(extendedLimitPtr, typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
                ulong peakProcessMemory = (ulong)extendedLimit.PeakProcessMemoryUsed;
                Log.LogInfo("Peak process memory :" + peakProcessMemory);
            }
            finally
            {
                Marshal.FreeHGlobal(extendedLimitPtr);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct IO_COUNTERS
    {
        public UInt64 ReadOperationCount;
        public UInt64 WriteOperationCount;
        public UInt64 OtherOperationCount;
        public UInt64 ReadTransferCount;
        public UInt64 WriteTransferCount;
        public UInt64 OtherTransferCount;
    }


    [StructLayout(LayoutKind.Sequential)]
    struct JOBOBJECT_BASIC_LIMIT_INFORMATION
    {
        public Int64 PerProcessUserTimeLimit;
        public Int64 PerJobUserTimeLimit;
        public UInt32 LimitFlags;
        public UIntPtr MinimumWorkingSetSize;
        public UIntPtr MaximumWorkingSetSize;
        public UInt32 ActiveProcessLimit;
        public UIntPtr Affinity;
        public UInt32 PriorityClass;
        public UInt32 SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES
    {
        public UInt32 nLength;
        public IntPtr lpSecurityDescriptor;
        public Int32 bInheritHandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
    {
        public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
        public IO_COUNTERS IoInfo;
        public UIntPtr ProcessMemoryLimit;
        public UIntPtr JobMemoryLimit;
        public UIntPtr PeakProcessMemoryUsed;
        public UIntPtr PeakJobMemoryUsed;
    }

    public enum JobObjectInfoType
    {
        AssociateCompletionPortInformation = 7,
        BasicLimitInformation = 2,
        BasicUIRestrictions = 4,
        EndOfJobTimeInformation = 6,
        ExtendedLimitInformation = 9,
        SecurityLimitInformation = 5,
        GroupInformation = 11
    }
}