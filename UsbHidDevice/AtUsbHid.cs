using System;
using System.Runtime.InteropServices;

namespace UsbHidDevice
{
    internal class AtUsbHid
    {
        private static IntPtr _libIntPtr;

        [DllImport("kernel32.dll")]
        private static extern bool freeLibrary(IntPtr hModule);
        public static bool FreeLibrary()
        {
            return _libIntPtr != IntPtr.Zero && freeLibrary(_libIntPtr);
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);

        public static void LoadUnmanagedDll(string file)
        {
            _libIntPtr = LoadLibrary(file);
            if (_libIntPtr == IntPtr.Zero) 
                throw new System.ComponentModel.Win32Exception();
        }

        [DllImport("AtUsbHid.dll", EntryPoint = "findHidDevice")]
        private static extern bool findHidDevice(int vendorId, int productId);
        public static bool FindHidDevice(int vendorId, int productId)
        {
            return findHidDevice(vendorId, productId);
        }

        [DllImport("AtUsbHid.dll", EntryPoint = "closeDevice")]
        private static extern void closeDevice();
        public static void CloseDevice()
        {
            closeDevice();
        }

        [DllImport("AtUsbHid.dll", EntryPoint = "writeData")]
        private static extern bool writeData(byte[] buffer);
        public static bool WriteData(byte[] buffer)
        {
            return writeData(buffer);
        }

        [DllImport("AtUsbHid.dll", EntryPoint = "readData")]
        private static extern bool readData(byte[] buffer);
        public static bool ReadData(byte[] buffer)
        {
            return readData(buffer);
        }

        [DllImport("AtUsbHid.dll", EntryPoint = "setFeature")]
        private static extern bool setFeature(byte[] buffer);
        public static bool SetFeature(byte[] buffer)
        {
            return setFeature(buffer);
        }

        [DllImport("AtUsbHid.dll", EntryPoint = "getFeatureReportLength")]
        private static extern int getFeatureReportLength();
        public static int GetFeatureReportLength()
        {
            return getFeatureReportLength();
        }

        [DllImport("AtUsbHid.dll", EntryPoint = "getOutputReportLength")]
        private static extern int getOutputReportLength();
        public static int GetOutputReportLength()
        {
            return getOutputReportLength();
        }

        [DllImport("AtUsbHid.dll", EntryPoint = "getInputReportLength")]
        private static extern int getInputReportLength();
        public static int GetInputReportLength()
        {
            return getInputReportLength();
        }

        [DllImport("AtUsbHid.dll", EntryPoint = "hidRegisterDeviceNotification")]
        private static extern  int  hidRegisterDeviceNotification(IntPtr hWnd);
        public static int HidRegisterDeviceNotification(IntPtr hWnd)
        {
            return hidRegisterDeviceNotification(hWnd);
        }

        [DllImport("AtUsbHid.dll", EntryPoint = "hidUnregisterDeviceNotification")]
        private static extern void hidUnregisterDeviceNotification(IntPtr hWnd);
        public static void HidUnregisterDeviceNotification(IntPtr hWnd)
        {
            hidUnregisterDeviceNotification(hWnd);
        }

        [DllImport("AtUsbHid.dll", EntryPoint = "isMyDeviceNotification")]
        private static extern int isMyDeviceNotification(int dwData);
        public static int HidUnregisterDeviceNotification(int dwData)
        {
            return isMyDeviceNotification(dwData);
        }
    }
}
