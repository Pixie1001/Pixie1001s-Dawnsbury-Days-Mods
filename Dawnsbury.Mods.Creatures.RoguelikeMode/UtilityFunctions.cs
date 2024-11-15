using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode {
    internal static class UtilityFunctions {

        public static string FilenameToMapName(string filename) {
            filename = filename.Substring(0, filename.Length - 4);

            // From Github User Binary Worrier: https://stackoverflow.com/a/272929
            if (string.IsNullOrWhiteSpace(filename))
                return "";
            StringBuilder newText = new StringBuilder(filename.Length * 2);
            newText.Append(filename[0]);
            for (int i = 1; i < filename.Length; i++) {
                if (char.IsUpper(filename[i]) && filename[i - 1] != ' ')
                    newText.Append(' ');
                newText.Append(filename[i]);
            }
            string output = newText.ToString();
            // This part was added on by me
            if (output.EndsWith(".png")) {
                output = output.Substring(0, output.Length - 4);
            }
            if (output.EndsWith("256")) {
                output = output.Substring(0, output.Length - 3);
            }
            return output;
        }

    }
}
