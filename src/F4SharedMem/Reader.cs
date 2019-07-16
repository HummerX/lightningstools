using F4SharedMem.Headers;
using F4SharedMem.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace F4SharedMem
{
    public sealed class Reader : IDisposable
    {
        private const string PrimarySharedMemoryAreaFileName = "FalconSharedMemoryArea";
        private const string SecondarySharedMemoryFileName = "FalconSharedMemoryArea2";
        private const string OsbSharedMemoryAreaFileName = "FalconSharedOsbMemoryArea";
        private const string IntelliVibeSharedMemoryAreaFileName = "FalconIntellivibeSharedMemoryArea";
        private const string RadioClientControlSharedMemoryAreaFileName = "FalconRccSharedMemoryArea";
        private const string RadioClientStatusSharedMemoryAreaFileName = "FalconRcsSharedMemoryArea";
        private bool _disposed;
        private IntPtr _hOsbSharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _hPrimarySharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _hSecondarySharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _hIntelliVibeSharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _hRadioClientControlSharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _hRadioClientStatusSharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _lpOsbSharedMemoryAreaBaseAddress = IntPtr.Zero;
        private IntPtr _lpPrimarySharedMemoryAreaBaseAddress = IntPtr.Zero;
        private IntPtr _lpSecondarySharedMemoryAreaBaseAddress = IntPtr.Zero;
        private IntPtr _lpIntelliVibeSharedMemoryAreaBaseAddress = IntPtr.Zero;
        private IntPtr _lpRadioClientControlSharedMemoryAreaBaseAddress = IntPtr.Zero;
        private IntPtr _lpRadioClientStatusSharedMemoryAreaBaseAddress = IntPtr.Zero;


        public bool IsFalconRunning
        {
            get
            {
                try
                {
                    ConnectToFalcon();
                    return _lpPrimarySharedMemoryAreaBaseAddress != IntPtr.Zero;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public byte[] GetRawOSBData()
        {
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                ConnectToFalcon();
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                return null;
            }
            var bytesRead = new List<byte>();
            if (!_hOsbSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                var fileSizeBytes = GetMaxMemFileSize(_lpOsbSharedMemoryAreaBaseAddress);
                if (fileSizeBytes > Marshal.SizeOf(typeof(OSBData))) fileSizeBytes = Marshal.SizeOf(typeof(OSBData));
                for (var i = 0; i < fileSizeBytes; i++)
                {
                    try
                    {
                        bytesRead.Add(Marshal.ReadByte(_lpOsbSharedMemoryAreaBaseAddress, i));
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            var toReturn = bytesRead.ToArray();
            return toReturn.Length == 0 ? null : toReturn;
        }
        public byte[] GetRawIntelliVibeData()
        {
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                ConnectToFalcon();
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                return null;
            }
            var bytesRead = new List<byte>();
            if (!_hIntelliVibeSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                var fileSizeBytes = GetMaxMemFileSize(_lpIntelliVibeSharedMemoryAreaBaseAddress);
                if (fileSizeBytes > Marshal.SizeOf(typeof(IntellivibeData))) fileSizeBytes = Marshal.SizeOf(typeof(IntellivibeData));
                for (var i = 0; i < fileSizeBytes; i++)
                {
                    try
                    {
                        bytesRead.Add(Marshal.ReadByte(_lpIntelliVibeSharedMemoryAreaBaseAddress, i));
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            var toReturn = bytesRead.ToArray();
            return toReturn.Length == 0 ? null : toReturn;
        }
        public byte[] GetRawRadioClientControlData()
        {
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                ConnectToFalcon();
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                return null;
            }
            var bytesRead = new List<byte>();
            if (!_hRadioClientControlSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                var fileSizeBytes = GetMaxMemFileSize(_lpRadioClientControlSharedMemoryAreaBaseAddress);
                if (fileSizeBytes > Marshal.SizeOf(typeof(RadioClientControl))) fileSizeBytes = Marshal.SizeOf(typeof(RadioClientControl));
                for (var i = 0; i < fileSizeBytes; i++)
                {
                    try
                    {
                        bytesRead.Add(Marshal.ReadByte(_lpRadioClientControlSharedMemoryAreaBaseAddress, i));
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            var toReturn = bytesRead.ToArray();
            return toReturn.Length == 0 ? null : toReturn;
        }

        public byte[] GetRawRadioClientStatusData()
        {
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                ConnectToFalcon();
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                return null;
            }
            var bytesRead = new List<byte>();
            if (!_hRadioClientStatusSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                var fileSizeBytes = GetMaxMemFileSize(_lpRadioClientStatusSharedMemoryAreaBaseAddress);
                if (fileSizeBytes > Marshal.SizeOf(typeof(RadioClientStatus))) fileSizeBytes = Marshal.SizeOf(typeof(RadioClientStatus));
                for (var i = 0; i < fileSizeBytes; i++)
                {
                    try
                    {
                        bytesRead.Add(Marshal.ReadByte(_lpRadioClientStatusSharedMemoryAreaBaseAddress, i));
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            var toReturn = bytesRead.ToArray();
            return toReturn.Length == 0 ? null : toReturn;
        }

        private static long GetMaxMemFileSize(IntPtr pMemAreaBaseAddr)
        {
            var mbi = new NativeMethods.MEMORY_BASIC_INFORMATION();
            NativeMethods.VirtualQuery(ref pMemAreaBaseAddr, ref mbi, new IntPtr(Marshal.SizeOf(mbi)));
            return mbi.RegionSize.ToInt64();
        }

        public byte[] GetRawFlightData2()
        {
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                ConnectToFalcon();
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                return null;
            }
            var bytesRead = new List<byte>();
            if (!_hSecondarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                var fileSizeBytes = GetMaxMemFileSize(_lpSecondarySharedMemoryAreaBaseAddress);
                if (fileSizeBytes > Marshal.SizeOf(typeof(FlightData2)))
                    fileSizeBytes = Marshal.SizeOf(typeof(FlightData2));
                for (var i = 0; i < fileSizeBytes; i++)
                {
                    try
                    {
                        bytesRead.Add(Marshal.ReadByte(_lpSecondarySharedMemoryAreaBaseAddress, i));
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            var toReturn = bytesRead.ToArray();
            return toReturn.Length == 0 ? null : toReturn;
        }

        public byte[] GetRawPrimaryFlightData()
        {
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                ConnectToFalcon();
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                return null;
            }
            var bytesRead = new List<byte>();
            if (!_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                var fileSizeBytes = GetMaxMemFileSize(_lpPrimarySharedMemoryAreaBaseAddress);
                if (fileSizeBytes > Marshal.SizeOf(typeof(BMS4FlightData)))
                    fileSizeBytes = Marshal.SizeOf(typeof(BMS4FlightData));
                for (var i = 0; i < fileSizeBytes; i++)
                {
                    try
                    {
                        bytesRead.Add(Marshal.ReadByte(_lpPrimarySharedMemoryAreaBaseAddress, i));
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            var toReturn = bytesRead.ToArray();
            return toReturn.Length == 0 ? null : toReturn;
        }

        public FlightData GetCurrentData()
        {
            var dataType = typeof(BMS4FlightData);

            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                ConnectToFalcon();
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                return null;
            }

            var data = Convert.ChangeType(Marshal.PtrToStructure(_lpPrimarySharedMemoryAreaBaseAddress, dataType), dataType);
            var toReturn = new FlightData((BMS4FlightData)data);

            if (!_hSecondarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                data = (FlightData2)(Marshal.PtrToStructure(_lpSecondarySharedMemoryAreaBaseAddress, typeof(FlightData2)));
                toReturn.PopulateFromStruct(data);
            }

            if (!_hOsbSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                data = (OSBData)(Marshal.PtrToStructure(_lpOsbSharedMemoryAreaBaseAddress, typeof(OSBData)));
                toReturn.PopulateFromStruct(data);
            }

            if (!_hIntelliVibeSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                data = (IntellivibeData)(Marshal.PtrToStructure(_lpIntelliVibeSharedMemoryAreaBaseAddress, typeof(IntellivibeData)));
                toReturn.IntellivibeData = (IntellivibeData)data;
            }

            if (!_hRadioClientControlSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                data = (RadioClientControl)(Marshal.PtrToStructure(_lpRadioClientControlSharedMemoryAreaBaseAddress, typeof(RadioClientControl)));
                toReturn.RadioClientControlData = (RadioClientControl)data;
            }

            if (!_hRadioClientStatusSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                data = (RadioClientStatus)(Marshal.PtrToStructure(_lpRadioClientStatusSharedMemoryAreaBaseAddress, typeof(RadioClientStatus)));
                toReturn.RadioClientStatus = (RadioClientStatus)data;
            }

            return toReturn;
        }

        private void ConnectToFalcon()
        {
            if (!_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero)) return;

            _hPrimarySharedMemoryAreaFileMappingObject = NativeMethods.OpenFileMapping(NativeMethods.SECTION_MAP_READ,
                false, PrimarySharedMemoryAreaFileName);
            _lpPrimarySharedMemoryAreaBaseAddress =
                NativeMethods.MapViewOfFile(_hPrimarySharedMemoryAreaFileMappingObject, NativeMethods.SECTION_MAP_READ,
                    0, 0, IntPtr.Zero);

            _hSecondarySharedMemoryAreaFileMappingObject = NativeMethods.OpenFileMapping(
                NativeMethods.SECTION_MAP_READ, false, SecondarySharedMemoryFileName);
            _lpSecondarySharedMemoryAreaBaseAddress =
                NativeMethods.MapViewOfFile(_hSecondarySharedMemoryAreaFileMappingObject, NativeMethods.SECTION_MAP_READ,
                    0, 0, IntPtr.Zero);

            _hOsbSharedMemoryAreaFileMappingObject = NativeMethods.OpenFileMapping(NativeMethods.SECTION_MAP_READ, false,
                OsbSharedMemoryAreaFileName);
            _lpOsbSharedMemoryAreaBaseAddress = NativeMethods.MapViewOfFile(_hOsbSharedMemoryAreaFileMappingObject,
                NativeMethods.SECTION_MAP_READ, 0, 0, IntPtr.Zero);

            _hIntelliVibeSharedMemoryAreaFileMappingObject = NativeMethods.OpenFileMapping(NativeMethods.SECTION_MAP_READ, false,
                IntelliVibeSharedMemoryAreaFileName);
            _lpIntelliVibeSharedMemoryAreaBaseAddress = NativeMethods.MapViewOfFile(_hIntelliVibeSharedMemoryAreaFileMappingObject,
                NativeMethods.SECTION_MAP_READ, 0, 0, IntPtr.Zero);

            _hRadioClientControlSharedMemoryAreaFileMappingObject = NativeMethods.OpenFileMapping(NativeMethods.SECTION_MAP_READ, false,
                RadioClientControlSharedMemoryAreaFileName);
            _lpRadioClientControlSharedMemoryAreaBaseAddress = NativeMethods.MapViewOfFile(_hRadioClientControlSharedMemoryAreaFileMappingObject,
                NativeMethods.SECTION_MAP_READ, 0, 0, IntPtr.Zero);

            _hRadioClientStatusSharedMemoryAreaFileMappingObject = NativeMethods.OpenFileMapping(NativeMethods.SECTION_MAP_READ, false,
                RadioClientStatusSharedMemoryAreaFileName);
            _lpRadioClientStatusSharedMemoryAreaBaseAddress = NativeMethods.MapViewOfFile(_hRadioClientStatusSharedMemoryAreaFileMappingObject,
                NativeMethods.SECTION_MAP_READ, 0, 0, IntPtr.Zero);

        }

        private void Disconnect()
        {
            if (!_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                NativeMethods.UnmapViewOfFile(_lpPrimarySharedMemoryAreaBaseAddress);
                NativeMethods.CloseHandle(_hPrimarySharedMemoryAreaFileMappingObject);
            }

            if (!_hSecondarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                NativeMethods.UnmapViewOfFile(_lpSecondarySharedMemoryAreaBaseAddress);
                NativeMethods.CloseHandle(_hSecondarySharedMemoryAreaFileMappingObject);
            }

            if (!_hOsbSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                NativeMethods.UnmapViewOfFile(_lpOsbSharedMemoryAreaBaseAddress);
                NativeMethods.CloseHandle(_hOsbSharedMemoryAreaFileMappingObject);
            }

            if (!_hIntelliVibeSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                NativeMethods.UnmapViewOfFile(_lpIntelliVibeSharedMemoryAreaBaseAddress);
                NativeMethods.CloseHandle(_hIntelliVibeSharedMemoryAreaFileMappingObject);
            }

            if (!_hRadioClientControlSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                NativeMethods.UnmapViewOfFile(_lpRadioClientControlSharedMemoryAreaBaseAddress);
                NativeMethods.CloseHandle(_hRadioClientControlSharedMemoryAreaFileMappingObject);
            }

            if (!_hRadioClientStatusSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                NativeMethods.UnmapViewOfFile(_lpRadioClientStatusSharedMemoryAreaBaseAddress);
                NativeMethods.CloseHandle(_hRadioClientStatusSharedMemoryAreaFileMappingObject);
            }
        }

        internal void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                Disconnect();
            }

            _disposed = true;
        }

        ~Reader()
        {
            Dispose(false);
        }
    }
}