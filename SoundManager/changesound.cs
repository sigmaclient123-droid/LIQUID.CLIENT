using System;

namespace liquid.client.SoundManager
{
    public class ChangeSoundManager
    {
        public static Action[] sounds;
        public static string[] soundNames;

        public static Action[] opensounds;
        public static string[] openSoundNames;

        public static int clickInt;
        public static int openInt;

        static ChangeSoundManager()
        {
            sounds = new Action[]
            {
                MenuAudioHandler.PlayClick,
                MenuAudioHandler.PlayClosesound,
                MenuAudioHandler.PlayClick2
            };
            soundNames = new string[]
            {
                "Click",
                "Minecraft",
                "Click2"
            };

            opensounds = new Action[]
            {
                MenuAudioHandler.PlayOpen,
                MenuAudioHandler.PlayOpen2,
                MenuAudioHandler.PlayOpen3,
                MenuAudioHandler.PlaySwich
            };
            openSoundNames = new string[]
            {
                "Open",
                "Open2",
                "Open3",
                "Switch"
            };

            clickInt = 0;
            openInt = 0;
        }

        public static void ChangeClickSound()
        {
            clickInt = (clickInt + 1) % sounds.Length;
            sounds[clickInt]?.Invoke();
        }

        public static void ChangeOpenSound()
        {
            openInt = (openInt + 1) % opensounds.Length;
            opensounds[openInt]?.Invoke();
        }

        public static string CurrentClickSoundName() => soundNames[clickInt];
        public static string CurrentOpenSoundName() => openSoundNames[openInt];
    }
}