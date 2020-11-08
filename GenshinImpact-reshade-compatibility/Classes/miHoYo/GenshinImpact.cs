using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GenshinImpact_reshade_compatibility.Classes.miHoYo
{
    static class GenshinImpact
    {
        static GenshinImpact()
        {
            // mhyprot2.Sys
            // GenshinImpact_Data\\app.info
        }

        public static bool IsGenshinImpactGameDirectory(string directoryPath)
        {
            if (File.Exists(Path.Combine(directoryPath, "mhyprot2.Sys")))
            {
                string appInfoPath = Path.Combine(directoryPath, "GenshinImpact_Data", "app.info");
                if (File.Exists(appInfoPath))
                {
                    bool isMiyoho = false;

                    /* File content look like below:
                     * miHoYo
                     * Genshin Impact
                     */
                    // Check for mihoyo and Genshin Impact, strict order (miHoYo appears first, then Genshin Impact).

                    foreach (var line in EnumerableLineTextFile(appInfoPath))
                    {
                        if (isMiyoho)
                        {
                            if (string.Equals(line, "Genshin Impact", StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (string.Equals(line, "miHoYo", StringComparison.OrdinalIgnoreCase))
                            {
                                isMiyoho = true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static IEnumerable<string> EnumerableLineTextFile(string filename)
        {
            using (var sr = new StreamReader(filename))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }
}
