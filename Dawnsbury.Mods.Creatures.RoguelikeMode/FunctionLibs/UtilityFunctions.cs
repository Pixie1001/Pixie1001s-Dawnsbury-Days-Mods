﻿using System;
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
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;

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

        public static bool IsFlanking(Creature flanker, Creature flankee) {
            int num1 = flankee.Occupies.X - flanker.Occupies.X;
            int num2 = flankee.Occupies.Y - flanker.Occupies.Y;
            int num3 = Math.Abs(num1);
            int num4 = Math.Abs(num2);
            if (num3 > 2 || num4 > 2)
                return false;
            bool flag = num3 <= 1 && num4 <= 1;
            if (num3 != num4 && num3 != 0 && num4 != 0)
                return FlanksWith(flanker, num1 <= 1 && num2 <= 1, flankee.Occupies.X + num1, flankee.Occupies.Y + num2);
            int num5 = Math.Sign(num1);
            int num6 = Math.Sign(num2);
            return FlanksWith(flanker, true, flankee.Occupies.X + num5, flankee.Occupies.Y + num6) || FlanksWith(flanker, false, flankee.Occupies.X + num5 * 2, flankee.Occupies.Y + num6 * 2);
        }

        private static bool FlanksWith(Creature flanker, bool adjacent, int otherX, int otherY) {
            Creature primaryOccupant = flanker.Battle.Map.GetTile(otherX, otherY)?.PrimaryOccupant;
            return primaryOccupant != null && primaryOccupant.FriendOf(flanker) && (adjacent || primaryOccupant.WieldsItem(Trait.Reach)) && primaryOccupant.Actions.CanTakeActions();
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
