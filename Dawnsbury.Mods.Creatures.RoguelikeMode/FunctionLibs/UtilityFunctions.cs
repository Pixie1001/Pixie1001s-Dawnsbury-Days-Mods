using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Display.Illustrations;
using Microsoft.Xna.Framework;
using System.Text;
using System.IO;
using System.Buffers.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs
{
    internal static class UtilityFunctions
    {
        internal static string WithPlus(int num) {
            if (num >= 0) {
                return $"+{num}";
            }
            return $"{num}";
        }

        public static string FilenameToMapName(string filename)
        {
            filename = filename.Substring(0, filename.Length - 4);

            // From Github User Binary Worrier: https://stackoverflow.com/a/272929
            if (string.IsNullOrWhiteSpace(filename))
                return "";
            StringBuilder newText = new StringBuilder(filename.Length * 2);
            newText.Append(filename[0]);
            for (int i = 1; i < filename.Length; i++)
            {
                if (char.IsUpper(filename[i]) && filename[i - 1] != ' ')
                    newText.Append(' ');
                newText.Append(filename[i]);
            }
            string output = newText.ToString();
            // This part was added on by me
            if (output.EndsWith(".png"))
            {
                output = output.Substring(0, output.Length - 4);
            }
            if (output.EndsWith("256"))
            {
                output = output.Substring(0, output.Length - 3);
            }
            return output;
        }

    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public record WandIllustration(Illustration NewMain, Illustration Center) : ScrollIllustration(IllustrationName.None, Center)
    {
        public override string IllustrationAsIconString => Center.IllustrationAsIconString;

        public override void DrawImage(Rectangle rectangle, Color? color, bool scale, bool scaleUp, Color? scaleBgColor)
        {
            int num = rectangle.Width / 6;
            int num2 = rectangle.Height / 6;
            //NewMain.DrawImage(new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), color, scale, scaleUp, scaleBgColor);
            // Primitives.DrawImage(Assets.TextureFromName(Main), rectangle, color, scale, scaleUp, scaleBgColor);

            Primitives.DrawImage(NewMain, rectangle: new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), color, scale, scaleUp, scaleBgColor);
            Primitives.DrawImage(Center, rectangle, color, scale, scaleUp, scaleBgColor);

            //Primitives.DrawImage(NewMain, rectangle, color, scale, scaleUp, scaleBgColor);
            //Primitives.DrawImage(rectangle: new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), illustration: Center, color: color, scale: scale, scaleUp: scaleUp, scaleBgColor: scaleBgColor);

        }

        public override void DrawImageNative(Rectangle rectangle, Color? color, bool scale, bool scaleUp, Color? scaleBgColor)
        {
            int num = rectangle.Width / 6;
            int num2 = rectangle.Height / 6;
            //NewMain.DrawImage(new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), color, scale, scaleUp, scaleBgColor);
            //Primitives.DrawImageNative(Assets.TextureFromName(Main), rectangle, color, scale, scaleUp, scaleBgColor);
            Primitives.DrawImageNative(NewMain, new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), color, scale, scaleUp, scaleBgColor);
            Primitives.DrawImageNative(Center, rectangle, color, scale, scaleUp, scaleBgColor);
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public record DualIllustration(Illustration NewMain, Illustration Center) : ScrollIllustration(IllustrationName.None, Center)
    {
        public override string IllustrationAsIconString => Center.IllustrationAsIconString;

        public override void DrawImage(Rectangle rectangle, Color? color, bool scale, bool scaleUp, Color? scaleBgColor)
        {
            int num = rectangle.Width / 6;
            int num2 = rectangle.Height / 6;
            //NewMain.DrawImage(new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), color, scale, scaleUp, scaleBgColor);
            // Primitives.DrawImage(Assets.TextureFromName(Main), rectangle, color, scale, scaleUp, scaleBgColor);

            Primitives.DrawImage(NewMain, rectangle, color, scale, scaleUp, scaleBgColor);
            Primitives.DrawImage(Center, rectangle: new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), color, scale, scaleUp, scaleBgColor);

            //Primitives.DrawImage(NewMain, rectangle, color, scale, scaleUp, scaleBgColor);
            //Primitives.DrawImage(rectangle: new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), illustration: Center, color: color, scale: scale, scaleUp: scaleUp, scaleBgColor: scaleBgColor);

        }

        public override void DrawImageNative(Rectangle rectangle, Color? color, bool scale, bool scaleUp, Color? scaleBgColor)
        {
            int num = rectangle.Width / 6;
            int num2 = rectangle.Height / 6;
            //NewMain.DrawImage(new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), color, scale, scaleUp, scaleBgColor);
            //Primitives.DrawImageNative(Assets.TextureFromName(Main), rectangle, color, scale, scaleUp, scaleBgColor);
            Primitives.DrawImageNative(NewMain, rectangle, color, scale, scaleUp, scaleBgColor);
            Primitives.DrawImageNative(Center, new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), color, scale, scaleUp, scaleBgColor);
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public record SameSizeDualIllustration(Illustration NewMain, Illustration Center) : ScrollIllustration(IllustrationName.None, Center)
    {
        public override string IllustrationAsIconString => Center.IllustrationAsIconString;

        public override void DrawImage(Rectangle rectangle, Color? color, bool scale, bool scaleUp, Color? scaleBgColor)
        {
            //NewMain.DrawImage(new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), color, scale, scaleUp, scaleBgColor);
            // Primitives.DrawImage(Assets.TextureFromName(Main), rectangle, color, scale, scaleUp, scaleBgColor);

            Primitives.DrawImage(NewMain, rectangle, color, scale, scaleUp, scaleBgColor);
            Primitives.DrawImage(Center, rectangle, color, scale, scaleUp, scaleBgColor);

            //Primitives.DrawImage(NewMain, rectangle, color, scale, scaleUp, scaleBgColor);
            //Primitives.DrawImage(rectangle: new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), illustration: Center, color: color, scale: scale, scaleUp: scaleUp, scaleBgColor: scaleBgColor);

        }

        public override void DrawImageNative(Rectangle rectangle, Color? color, bool scale, bool scaleUp, Color? scaleBgColor)
        {
            Primitives.DrawImageNative(NewMain, rectangle, color, scale, scaleUp, scaleBgColor);
            Primitives.DrawImageNative(Center, rectangle, color, scale, scaleUp, scaleBgColor);
        }
    }
}
