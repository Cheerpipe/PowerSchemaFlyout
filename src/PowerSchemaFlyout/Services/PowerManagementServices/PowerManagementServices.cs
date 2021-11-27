
// https://github.com/petrroll/PowerSwitcher

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using PowerSchemaFlyout.Models;
using PowerSchemaFlyout.PowerManagement;

namespace PowerSchemaFlyout.Services
{

    public class PowerManagementServices : IPowerManagementServices
    {
        public IEnumerable<Guid> GetAllPowerSchemaGuids()
        {
            var schemeGuid = Guid.Empty;

            uint sizeSchemeGuid = (uint)Marshal.SizeOf(typeof(Guid));
            uint schemeIndex = 0;

            while (true)
            {
                uint errCode = PowerSchemasWrapperNative.PowerEnumerate(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, (uint)PowerSchemasWrapperNative.AccessFlags.AccessScheme, schemeIndex, ref schemeGuid, ref sizeSchemeGuid);
                if (errCode == PowerSchemasWrapperNative.ErrorNoMoreItems) { yield break; }
                if (errCode != 0) { throw new PowerSwitcherWrappersException($"GetPowerSchemeGUIDs() failed when getting buffer pointer with code {errCode}"); }

                yield return schemeGuid;
                schemeIndex++;
            }
        }

        public string GetPowerPlanName(Guid guid)
        {
            string name;

            IntPtr bufferPointer = IntPtr.Zero;
            uint bufferSize = 0;

            try
            {
                var errCode = PowerSchemasWrapperNative.PowerReadFriendlyName(IntPtr.Zero, ref guid, IntPtr.Zero, IntPtr.Zero, bufferPointer, ref bufferSize);
                if (errCode != 0) { throw new PowerSwitcherWrappersException($"GetPowerPlanName() failed when getting buffer size with code {errCode}"); }

                if (bufferSize <= 0) { return String.Empty; }
                bufferPointer = Marshal.AllocHGlobal((int)bufferSize);

                errCode = PowerSchemasWrapperNative.PowerReadFriendlyName(IntPtr.Zero, ref guid, IntPtr.Zero, IntPtr.Zero, bufferPointer, ref bufferSize);
                if (errCode != 0) { throw new PowerSwitcherWrappersException($"GetPowerPlanName() failed when getting buffer pointer with code {errCode}"); }

                name = Marshal.PtrToStringUni(bufferPointer);
            }
            finally
            {
                if (bufferPointer != IntPtr.Zero) { Marshal.FreeHGlobal(bufferPointer); }
            }

            return name;
        }

        public List<PowerSchema> GetCurrentSchemas()
        {
            var powerSchemas = GetAllPowerSchemaGuids().Select(guid => new PowerSchema(GetPowerPlanName(guid), guid, GetActiveGuid() == guid)).ToList();
            return powerSchemas;
        }

        public Guid GetActiveGuid()
        {
            Guid activeSchema;
            IntPtr guidPtr = IntPtr.Zero;

            try
            {
                var errCode = PowerSchemasWrapperNative.PowerGetActiveScheme(IntPtr.Zero, out guidPtr);

                if (errCode != 0) { throw new PowerSwitcherWrappersException($"GetActiveGuid() failed with code {errCode}"); }
                if (guidPtr == IntPtr.Zero) { throw new PowerSwitcherWrappersException("GetActiveGuid() returned null pointer for GUID"); }

                activeSchema = (Guid)Marshal.PtrToStructure(guidPtr, typeof(Guid))!;
            }
            finally
            {
                if (guidPtr != IntPtr.Zero) { PowerSchemasWrapperNative.LocalFree(guidPtr); }
            }

            return activeSchema;
        }

        public void SetActiveGuid(Guid guid)
        {
            if (guid == GetActiveGuid())
                return;

            var errCode = PowerSchemasWrapperNative.PowerSetActiveScheme(IntPtr.Zero, ref guid);
            if (errCode != 0) { throw new PowerSwitcherWrappersException($"SetActiveGuid() failed with code {errCode}"); }
            ActiveGuidChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ActiveGuidChanged;
    }
}
