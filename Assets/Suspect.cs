using System;

namespace Mafia
{
    sealed class SuspectInfo
    {
        public Suspect Name { get; private set; }
        public Func<KMBombInfo, Suspect[], Suspect[], float, Suspect> GodfatherGetter { get; private set; }
        public SuspectInfo(Suspect name, Func<KMBombInfo, Suspect[], Suspect[], float, Suspect> godfatherGetter)
        {
            Name = name;
            GodfatherGetter = godfatherGetter;
        }
        public SuspectInfo(Suspect name, Func<KMBombInfo, Suspect[], Suspect[], Suspect> godfatherGetter)
        {
            Name = name;
            GodfatherGetter = (bomb, suspects, eliminated, startingTime) => godfatherGetter(bomb, suspects, eliminated);
        }
        public Suspect GetGodfather(KMBombInfo bomb, Suspect[] suspects, Suspect[] eliminated, float startingTime)
        {
            return GodfatherGetter(bomb, suspects, eliminated, startingTime);
        }
    }
}
