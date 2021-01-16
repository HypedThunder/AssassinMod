using R2API;
using System;
using UnityEngine;

namespace AssassinAssets
{
    public static class Assets
    {
        public static AssetBundle AssassinAssetBundle = LoadAssetBundle(Assassin.Properties.Resources.assassinassets);
        public static AssetBundle AssassinAssetBundle2 = LoadAssetBundle(Assassin.Properties.Resources.assassinassets21);

        public static Texture charPortrait = AssassinAssetBundle2.LoadAsset<Texture>("icon 1");
        public static Sprite iconP = AssassinAssetBundle.LoadAsset<Sprite>("iconP");
        public static Sprite icon1 = AssassinAssetBundle.LoadAsset<Sprite>("icon1");
        public static Sprite icon2 = AssassinAssetBundle2.LoadAsset<Sprite>("icon9");
        public static Sprite icon2b = AssassinAssetBundle2.LoadAsset<Sprite>("icon2b");
        public static Sprite icon3 = AssassinAssetBundle2.LoadAsset<Sprite>("icon10");
        public static Sprite icon4 = AssassinAssetBundle.LoadAsset<Sprite>("icon4");

        static AssetBundle LoadAssetBundle(Byte[] resourceBytes)
        {
            //Check to make sure that the byte array supplied is not null, and throw an appropriate exception if they are.
            if (resourceBytes == null) throw new ArgumentNullException(nameof(resourceBytes));

            //Actually load the bundle with a Unity function.
            var bundle = AssetBundle.LoadFromMemory(resourceBytes);

            return bundle;
        }
    }
}