using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace matritsa.Util {
    public static class Utils {
        public static void OpenUrl(string url) {
            try {
                Process.Start(url);
            }
            catch {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                    Process.Start("open", url);
                }
                else {
                    throw;
                }
            }
        }


        public static string GetNumbers(this string input) {
            return new string(input.Where(c => char.IsDigit(c)).ToArray());
        }
    }
}
