using System;
using System.Collections.Generic;
using System.Linq;
using Discord;

namespace ComradeMajor.App
{
    public static class Reactions
    {
        public static readonly IEmote kLike = new Emoji("👍");
        public static readonly IEmote kOk = new Emoji("👌");
        public static readonly IEmote kBlush = new Emoji("😊");
        public static readonly IEmote kHeart = new Emoji("❤️");
        public static readonly IEmote kBrokenHeart = new Emoji("💔");
        public static readonly IEmote kFrowning = new Emoji("😠");
        public static readonly IEmote kDevil = new Emoji("👿");
        public static readonly IEmote kDevilSmile = new Emoji("😈");
        public static readonly IEmote kAngry = new Emoji("😡");
        public static readonly IEmote kCurse = new Emoji("🤬");
        public static readonly IEmote kMonocle = new Emoji("🧐");
        public static readonly IEmote kTea = new Emoji("☕");

        public static readonly IEmote[] kPositive =
            new [] { kLike, kOk, kBlush, kHeart, kDevilSmile };
        public static readonly IEmote[] kNegative =
            new [] { kBrokenHeart, kFrowning, kAngry, kCurse, kMonocle, kDevil };

        public static readonly IEmote[] kAll =
            Enumerable.Concat(kPositive, kNegative).ToArray();

        public static IEmote Random(this IEmote[] array)
        {
            var rng = new Random();
            return array[rng.Next(0, array.Length)];
        }
    }
}