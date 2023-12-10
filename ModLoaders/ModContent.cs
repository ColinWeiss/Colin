using System.Reflection;

namespace Colin.Core.ModLoaders
{
    public class ModContent
    {
        public static Dictionary<string, IMod> Mods = new Dictionary<string, IMod>();

        public static Dictionary<IMod, Assembly> ModCodes = new Dictionary<IMod, Assembly>();

        public static Assembly GetCode(string modName)
        {
            if (Mods.TryGetValue(modName, out IMod mod))
            {
                if (ModCodes.TryGetValue(mod, out Assembly code))
                    return code;
                else
                    return null;
            }
            else
                return null;
        }

        internal static void DoInitialize()
        {
            Mods.Add(EngineInfo.Engine.Name, EngineInfo.Engine);
            ModCodes.Add(EngineInfo.Engine, Assembly.GetExecutingAssembly());
        }

        public static string GetModDomain(IMod mod) => string.Concat(mod.Name, ": ");
    }
}