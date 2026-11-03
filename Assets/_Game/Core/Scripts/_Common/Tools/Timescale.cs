using MoreMountains.Feedbacks;

namespace Game.Tools
{
    public static class Timescale
    {
        public static void Pause()
        {
            MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
        }

        public static void UnPause()
        {
            MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
        }

        public static void For(float modifier)
        {
            MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, modifier, 1f, true, 5f, true);
        }

    }
}
