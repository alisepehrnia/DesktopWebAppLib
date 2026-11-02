using System;
using System.Management;
using SG.Core.Models;
using SG.Core.Security;

namespace SG.Core.Licensing
{
    /// <summary>WMI based hardware reader. Same fields, same hashing and same order as the old Subscribe.Machine, so fingerprints stay identical.</summary>
    public sealed class WmiHardwareProvider : IHardwareProvider
    {
        public Hardware Read()
        {
            return new Hardware { CPU = Cpu(), Motherboard = Board(), BIOS = Bios() };
        }

        /// <summary>The stable fingerprint string stored on the server and in the license file.</summary>
        public static string Fingerprint(Hardware hardware)
        {
            return Cryptography.Fingerprint(hardware);
        }

        private static string First(string wmiClass, string property)
        {
            try
            {
                using (var mc = new ManagementClass(wmiClass))
                using (ManagementObjectCollection instances = mc.GetInstances())
                {
                    foreach (ManagementBaseObject mo in instances)
                    {
                        try
                        {
                            object v = mo[property];
                            if (v != null) return v.ToString();
                        }
                        catch (Exception) { }
                        break; // only the first instance, like before (reading all is slow)
                    }
                }
            }
            catch (Exception ex) { CoreLog.Write("WMI " + wmiClass + "." + property, ex); }
            return "";
        }

        private static CPU Cpu()
        {
            var cpu = new CPU { UniqueId = First("Win32_Processor", "UniqueId") };
            if (string.IsNullOrEmpty(cpu.UniqueId))
            {
                cpu.ProcessorId = First("Win32_Processor", "ProcessorId");
                if (string.IsNullOrEmpty(cpu.ProcessorId))
                {
                    cpu.Manufacturer = First("Win32_Processor", "Manufacturer");
                    cpu.MaxClockSpeed = First("Win32_Processor", "MaxClockSpeed");
                }
            }
            return new CPU
            {
                UniqueId = Cryptography.XHash(cpu.UniqueId),
                ProcessorId = Cryptography.XHash(cpu.ProcessorId),
                Manufacturer = Cryptography.XHash(cpu.Manufacturer),
                MaxClockSpeed = Cryptography.XHash(cpu.MaxClockSpeed)
            };
        }

        private static BIOS Bios()
        {
            return new BIOS
            {
                Manufacturer = Cryptography.XHash(First("Win32_BIOS", "Manufacturer")),
                IdentificationCode = Cryptography.XHash(First("Win32_BIOS", "IdentificationCode")),
                SerialNumber = Cryptography.XHash(First("Win32_BIOS", "SerialNumber"))
            };
        }

        private static Motherboard Board()
        {
            return new Motherboard
            {
                Manufacturer = Cryptography.XHash(First("Win32_BaseBoard", "Manufacturer")),
                Model = Cryptography.XHash(First("Win32_BaseBoard", "Model")),
                SerialNumber = Cryptography.XHash(First("Win32_BaseBoard", "SerialNumber"))
            };
        }
    }

}
