using System.Collections.Generic;

#nullable enable

namespace Stolon
{
    public class FlashHandler
    {
        private bool flash;
        private bool hasEnded;

        private int flashCount;

        private int duration;
        private int interval;
        private int lastFlash;

        private int milisecondsSinceStart;

        private List<int> flashMap;

        public bool Flash => flash;
        public bool HasEnded => hasEnded;

        public FlashHandler(int duration, int initialInterval)
        {
            this.duration = duration;
            interval = initialInterval;

            flash = false;

            flashMap = new List<int>();

            for (int index = 0; true; index++)
            {
                int toAdd = (int)(initialInterval - 5 * index * index * 0.8f);
                if (toAdd < 0) break;
                flashMap.Add(toAdd);
            }
        }

        public void Update(int elapsedMiliseconds)
        {
            if (hasEnded) return;

            milisecondsSinceStart += elapsedMiliseconds;

            for (int i = 0; i < flashMap.Count; i++)
            {
                int relevantMiliseconds = flashMap[i];
                if (milisecondsSinceStart > relevantMiliseconds)
                {
                    if (lastFlash != relevantMiliseconds)
                    {
                        flash = !flash;
                        hasEnded = i == 0;
                    }
                    lastFlash = relevantMiliseconds;
                    break;
                }
            }
        }
    }
}
