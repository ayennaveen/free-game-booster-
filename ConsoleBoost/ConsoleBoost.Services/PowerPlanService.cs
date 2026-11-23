using System.Runtime.InteropServices;
using ConsoleBoost.Helpers;
using ConsoleBoost.Models;

namespace ConsoleBoost.Services;

public class PowerPlanService : IPowerPlanService
{
    private readonly ILoggingService _logger;

    public PowerPlanService(ILoggingService logger)
    {
        _logger = logger;
    }

    public PowerPlanInfo GetActivePowerPlan()
    {
        try
        {
            uint res = NativeMethods.PowerGetActiveScheme(IntPtr.Zero, out IntPtr activeGuidPtr);
            if (res == 0 && activeGuidPtr != IntPtr.Zero)
            {
                Guid guid = Marshal.PtrToStructure<Guid>(activeGuidPtr);
                Marshal.FreeHGlobal(activeGuidPtr);

                string name = GetFriendlyName(guid);
                PowerPlanType type = MapType(guid, name);

                return new PowerPlanInfo
                {
                    SchemeGuid = guid,
                    Name = name,
                    IsActive = true,
                    Type = type
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get active power plan via Win32 API", ex);
        }

        return new PowerPlanInfo
        {
            SchemeGuid = NativeMethods.GUID_TYPICAL_POWER_SAVINGS,
            Name = "Balanced",
            IsActive = true,
            Type = PowerPlanType.Balanced
        };
    }

    public IEnumerable<PowerPlanInfo> GetAvailablePowerPlans()
    {
        var list = new List<PowerPlanInfo>();
        var active = GetActivePowerPlan();

        var standardPlans = new (Guid guid, string defaultName, PowerPlanType type)[]
        {
            (NativeMethods.GUID_TYPICAL_POWER_SAVINGS, "Balanced", PowerPlanType.Balanced),
            (NativeMethods.GUID_MIN_POWER_SAVINGS, "High Performance", PowerPlanType.HighPerformance),
            (NativeMethods.GUID_ULTIMATE_PERFORMANCE, "Ultimate Performance", PowerPlanType.UltimatePerformance),
            (NativeMethods.GUID_MAX_POWER_SAVINGS, "Power Saver", PowerPlanType.PowerSaver)
        };

        foreach (var (guid, defaultName, type) in standardPlans)
        {
            string name = GetFriendlyName(guid);
            if (string.IsNullOrWhiteSpace(name)) name = defaultName;

            list.Add(new PowerPlanInfo
            {
                SchemeGuid = guid,
                Name = name,
                IsActive = active.SchemeGuid == guid,
                Type = type
            });
        }

        return list;
    }

    public bool SetActivePowerPlan(Guid schemeGuid)
    {
        try
        {
            uint res = NativeMethods.PowerSetActiveScheme(IntPtr.Zero, ref schemeGuid);
            if (res == 0)
            {
                _logger.LogInfo($"Power plan switched to: {schemeGuid}");
                return true;
            }
            _logger.LogWarning($"PowerSetActiveScheme returned error code {res}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception setting power plan {schemeGuid}", ex);
        }
        return false;
    }

    public bool SetHighPerformance()
    {
        bool success = SetActivePowerPlan(NativeMethods.GUID_ULTIMATE_PERFORMANCE);
        if (!success)
        {
            success = SetActivePowerPlan(NativeMethods.GUID_MIN_POWER_SAVINGS);
        }
        return success;
    }

    public bool RestorePowerPlan(Guid schemeGuid)
    {
        return SetActivePowerPlan(schemeGuid);
    }

    private string GetFriendlyName(Guid schemeGuid)
    {
        uint bufferSize = 256;
        IntPtr buffer = Marshal.AllocHGlobal((int)bufferSize);
        try
        {
            uint res = NativeMethods.PowerReadFriendlyName(IntPtr.Zero, ref schemeGuid, IntPtr.Zero, IntPtr.Zero, buffer, ref bufferSize);
            if (res == 0)
            {
                return Marshal.PtrToStringUni(buffer) ?? string.Empty;
            }
        }
        catch
        {
            // Fallback
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
        return string.Empty;
    }

    private PowerPlanType MapType(Guid guid, string name)
    {
        if (guid == NativeMethods.GUID_MIN_POWER_SAVINGS || name.Contains("High Performance", StringComparison.OrdinalIgnoreCase))
            return PowerPlanType.HighPerformance;
        if (guid == NativeMethods.GUID_ULTIMATE_PERFORMANCE || name.Contains("Ultimate", StringComparison.OrdinalIgnoreCase))
            return PowerPlanType.UltimatePerformance;
        if (guid == NativeMethods.GUID_MAX_POWER_SAVINGS || name.Contains("Saver", StringComparison.OrdinalIgnoreCase))
            return PowerPlanType.PowerSaver;

        return PowerPlanType.Balanced;
    }
}
