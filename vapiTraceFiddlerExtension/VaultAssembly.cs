using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;

namespace vapiTraceFiddlerExtension
{
    internal class VaultAssembly
    {
        private static Assembly _instance;
        public static Assembly Instance {
            get
            {
                if (_instance == null)
                {
                    var assemblyFile = GetAssemblyFile();
                    if (assemblyFile == null)
                        _instance = null;
                    else
                    {
                        try
                        {
                            _instance = Assembly.LoadFrom(assemblyFile);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            _instance = null;
                        }
                    }
                }

                return _instance;
            }
        }

        public static Type GetServiceType(string service)
        {
            var typeName = $"Autodesk.Connectivity.WebServices.{service}Service";
            if (typeName.EndsWith("ServiceService"))
                typeName = typeName.Replace("ServiceService", "Service");
            
            return Instance.GetType(typeName);
        }



        public static string GetAssemblyFile()
        {
            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            using (var key = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
            {
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        var subKey = key.OpenSubKey(subKeyName);
                        var displayName = subKey?.GetValue("DisplayName")?.ToString();
                        if (displayName != null && displayName.StartsWith("Autodesk Vault"))
                        {
                            var installLocation = subKey.GetValue("InstallLocation").ToString();

                            string[] files = Directory.GetFiles(installLocation,
                                "Autodesk.Connectivity.WebServices.dll", SearchOption.AllDirectories);
                            if (files.Length > 0)
                                return files.First();
                        }
                    }
                }
            }

            return null;
        }
    }
}
