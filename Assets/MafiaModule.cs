using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mafia;
using UnityEngine;
using Rnd = UnityEngine.Random;

/// <summary>
/// On the Subject of Mafia
/// Created by MarioXMan, Zeke and Timwi
/// </summary>
public class MafiaModule : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public KMAudio Audio;
    public TextMesh[] NameMeshes;
    public KMSelectable[] StickFigures;

    private static int _moduleIdCounter = 1;
    private int _moduleId;
    private float _startingTime;
    private Suspect[] _suspects;
    private Suspect _godfather;
    private bool _isSolved;
    private bool _animating;
    private static Suspect[] _allSuspects = (Suspect[]) Enum.GetValues(typeof(Suspect));

    private const int _numSuspects = 8;

    private static readonly string[] _TimModules = new[] { "Friendship", "Only Connect", "Battleship", "Marble Tumble" };
    private static readonly string[] _LacyModules = new[] { "Boolean Venn Diagram", "Bitwise Operations" };  // plus containing “Logic”
    private static readonly string[] _JimModules = new[] { "Chord Qualities", "Rhythms" };    // plus containing “Piano Keys”
    private static readonly string[] _BobModules = new[] { "Laundry", "Morse-A-Maze", "Big Circle", "Painting", "Dr. Doctor", "The Code" };
    private static readonly string[] _GaryModules = new[] { "Cheap Checkout", "Ice Cream", "Cooking" };
    private static readonly string[] _SamModules = new[] { "Creation", "The Gamepad", "Minesweeper", "Skewed Slots" };
    private static readonly string[] _EdModules = new[] { "Gridlock", "Human Resources", "Lasers" };    // plus containing “Double-Oh”
    private static readonly string[] _VanillaModules = new[] { "The Button", "Needy Capacitor", "Complicated Wires", "Keypad", "Needy Knob", "Maze", "Memory", "Morse Code", "Password", "Simon Says", "Needy Vent Gas", "Who's on First", "Wire Sequence", "Wires" };
    private static readonly string[] _NickModules = new[] { "Zoo", "Nonogram", "Murder", "X01" };
    private static readonly string[] _TedModules = new[] { "Black Hole", "The Sun", "The Moon", "Lightspeed", "Astrology" };
    private static readonly string[] _JerryModules = new[] { "The Clock", "Rubik's Clock", "The Stopwatch", "Timezone", "The Time Keeper" };

    private static Dictionary<Suspect, SuspectInfo> _suspectInfos = Ut.NewArray(
        new SuspectInfo(Suspect.Rob, (bomb, suspects, eliminated) => bomb.GetSerialNumberLetters().Any(ch => "AEIOU".Contains(ch)) ? suspects.After(Suspect.Rob) : Suspect.Rob),
        new SuspectInfo(Suspect.Tim, (bomb, suspects, eliminated) => bomb.GetModuleNames().Intersect(_TimModules).Any() ? eliminated[0] : Suspect.Tim),
        new SuspectInfo(Suspect.Mary, (bomb, suspects, eliminated) => new[] { Suspect.Bob, Suspect.Walter, Suspect.Cher }.Any(n => suspects.IndexOf(n) != -1) ? suspects[suspects[0] == Suspect.Mary ? 1 : 0] : Suspect.Mary),
        new SuspectInfo(Suspect.Briane, (bomb, suspects, eliminated) => bomb.IsTwoFactorPresent() || bomb.IsIndicatorOn(Indicator.CAR) ? eliminated.Last() : Suspect.Briane),
        new SuspectInfo(Suspect.Hunter, (bomb, suspects, eliminated) => bomb.GetPortCount() > bomb.GetBatteryCount() ? suspects.FindOrDefault(Suspect.Rick, eliminated[3]) : Suspect.Hunter),
        new SuspectInfo(Suspect.Macy, (bomb, suspects, eliminated) => suspects.FindOrDefault(Suspect.Tommy, Suspect.Macy)),
        new SuspectInfo(Suspect.John, (bomb, suspects, eliminated) => suspects.Count(s => s.ToString().StartsWith("J")) == 1 ? suspects[suspects.IndexOf(Suspect.John) ^ 1] : Suspect.John),
        new SuspectInfo(Suspect.Will, (bomb, suspects, eliminated) => (bomb.IsPortPresent(Port.PS2) || bomb.IsPortPresent(Port.DVI)) && bomb.GetSerialNumberNumbers().Any(i => i % 2 == 0) ? eliminated[4] : Suspect.Will),
        new SuspectInfo(Suspect.Lacy, (bomb, suspects, eliminated) => bomb.GetModuleNames().Any(m => m.Contains("Logic") || _LacyModules.Contains(m)) ? suspects[suspects.IndexOf(Suspect.Lacy) ^ 1] : Suspect.Lacy),
        new SuspectInfo(Suspect.Claire, (bomb, suspects, eliminated) => bomb.GetModuleNames().Count < 20 ? eliminated.Last() : Suspect.Claire),
        new SuspectInfo(Suspect.Kenny, (bomb, suspects, eliminated) => bomb.GetOffIndicators().Any() ? Suspect.Kenny : suspects.After(eliminated[0], skip: Suspect.Kenny)),
        new SuspectInfo(Suspect.Rick, (bomb, suspects, eliminated) => bomb.GetPortPlates().Any(pp => pp.Length == 0) ? suspects.After(Suspect.Rick, suspects.Length - 1) : Suspect.Rick),
        new SuspectInfo(Suspect.Walter, (bomb, suspects, eliminated) => bomb.GetSerialNumberLetters().Any(ch => "WALTER".Contains(ch)) ? eliminated[0] : Suspect.Walter),
        new SuspectInfo(Suspect.Bonnie, (bomb, suspects, eliminated) => suspects.FirstAfter(Suspect.Bonnie, s => s.ToString().StartsWith("B"))),
        new SuspectInfo(Suspect.Luke, (bomb, suspects, eliminated) => _allSuspects.First(s => s != Suspect.Luke && suspects.Contains(s))),
        new SuspectInfo(Suspect.Bill, (bomb, suspects, eliminated) => new[] { 0, 2, 3, 5, 7 }.Contains(bomb.GetSerialNumberNumbers().Last()) ? _allSuspects.Last(s => s != Suspect.Bill && suspects.Contains(s)) : Suspect.Bill),
        new SuspectInfo(Suspect.Sarah, (bomb, suspects, eliminated) => bomb.GetColoredIndicators().Any() || bomb.IsPortPresent(Port.HDMI) || bomb.GetSerialNumber().Any(ch => "SH3".Contains(ch)) ? eliminated.Last() : Suspect.Sarah),
        new SuspectInfo(Suspect.Larry, (bomb, suspects, eliminated) => bomb.GetModuleNames().Any(m => m.ContainsNoCase("color") || m.ContainsNoCase("colour")) ? Suspect.Larry : eliminated[0]),
        new SuspectInfo(Suspect.Kate, (bomb, suspects, eliminated) => bomb.GetSerialNumberLetters().Any(ch => "LOST".Contains(ch)) || bomb.GetModuleNames().Contains("The Swan") ? (suspects.Contains(Suspect.John) ? Suspect.John : suspects[suspects.IndexOf(Suspect.Kate) ^ 1]) : Suspect.Kate),
        new SuspectInfo(Suspect.Stacy, (bomb, suspects, eliminated, startingTime) => bomb.GetModuleNames().Count < startingTime ? eliminated[0] : Suspect.Stacy),
        new SuspectInfo(Suspect.Diane, (bomb, suspects, eliminated) => bomb.IsPortPresent(Port.VGA) || bomb.IsPortPresent(Port.USB) || bomb.GetModuleNames().Contains("The Screw") ? eliminated.Last() : Suspect.Diane),
        new SuspectInfo(Suspect.Mac, (bomb, suspects, eliminated) => bomb.GetPortPlates().Any(pp => pp.Contains(Port.Parallel.ToString()) && pp.Contains(Port.Serial.ToString())) ? eliminated[5] : Suspect.Mac),
        new SuspectInfo(Suspect.Jim, (bomb, suspects, eliminated) => bomb.GetModuleNames().Any(m => _JimModules.Contains(m) || m.Contains("Piano Keys") || m.Contains("Jukebox") || m.Contains("Guitar Chords")) ? suspects[suspects.IndexOf(Suspect.Jim) ^ 1] : Suspect.Jim),
        new SuspectInfo(Suspect.Clyde, (bomb, suspects, eliminated) => suspects.Contains(Suspect.Bonnie) ? Suspect.Bonnie : Suspect.Clyde),
        new SuspectInfo(Suspect.Tommy, (bomb, suspects, eliminated) => bomb.GetBatteryCount() == 0 && bomb.GetPortCount() == 0 ? eliminated[3] : Suspect.Tommy),
        new SuspectInfo(Suspect.Lenny, (bomb, suspects, eliminated) => { var ssn = suspects[suspects.IndexOf(Suspect.Lenny) ^ 1]; return ssn.ToString().Length == 3 ? Suspect.Lenny : ssn; }),
        new SuspectInfo(Suspect.Molly, (bomb, suspects, eliminated) => bomb.GetModuleNames().Except(new[] { "Mafia" }).Any(m => m.StartsWith("M") || m.StartsWith("The M")) ? Suspect.Molly : suspects.After(Suspect.Molly)),
        new SuspectInfo(Suspect.Benny, (bomb, suspects, eliminated) => new[] { Suspect.Hunter, Suspect.Cher, Suspect.Nick }.Contains(eliminated[0]) ? Suspect.Benny : suspects.After(Suspect.Benny, 3)),
        new SuspectInfo(Suspect.Phil, (bomb, suspects, eliminated) => suspects[4]),
        new SuspectInfo(Suspect.Bob, (bomb, suspects, eliminated) => bomb.GetModuleNames().Intersect(_BobModules).Any() || bomb.IsIndicatorPresent(Indicator.BOB) ? eliminated[2] : Suspect.Bob),
        new SuspectInfo(Suspect.Gary, (bomb, suspects, eliminated) => bomb.GetModuleNames().Intersect(_GaryModules).Any() ? eliminated.Last() : Suspect.Gary),
        new SuspectInfo(Suspect.Ted, (bomb, suspects, eliminated) => bomb.GetModuleNames().Intersect(_TedModules).Any() ? suspects[suspects.IndexOf(Suspect.Ted) ^ 1] : Suspect.Ted),
        new SuspectInfo(Suspect.Kim, (bomb, suspects, eliminated) => _allSuspects.IndexOf(eliminated[0]) < 25 ? eliminated[0] : Suspect.Kim),
        new SuspectInfo(Suspect.Nate, (bomb, suspects, eliminated) => bomb.GetOnIndicators().Count() > bomb.GetOffIndicators().Count() ? suspects.After(Suspect.Nate) : Suspect.Nate),
        new SuspectInfo(Suspect.Cher, (bomb, suspects, eliminated) => bomb.GetPortCount() > 0 && bomb.GetSolvableModuleNames().Count == bomb.GetModuleNames().Count ? eliminated.Last() : Suspect.Cher),
        new SuspectInfo(Suspect.Ron, (bomb, suspects, eliminated) => bomb.GetSerialNumberLetters().Intersect(bomb.GetIndicators().SelectMany(ind => ind)).Any() ? suspects[suspects.IndexOf(Suspect.Ron) ^ 1] : Suspect.Ron),
        new SuspectInfo(Suspect.Thomas, (bomb, suspects, eliminated) => bomb.GetModuleNames().Any(name => name.ToLowerInvariant().Contains("maze")) ? Suspect.Thomas : suspects.After(Suspect.Thomas, suspects.Length - 2)),
        new SuspectInfo(Suspect.Sam, (bomb, suspects, eliminated) => bomb.GetModuleNames().Intersect(_SamModules).Any() ? eliminated.Last() : Suspect.Sam),
        new SuspectInfo(Suspect.Duke, (bomb, suspects, eliminated) => _allSuspects.IndexOf(eliminated.Last()) >= 25 ? eliminated.Last() : Suspect.Duke),
        new SuspectInfo(Suspect.Jack, (bomb, suspects, eliminated) => { var ssn = suspects[suspects.IndexOf(Suspect.Jack) ^ 1]; return ssn.ToString().Length == 4 ? ssn : Suspect.Jack; }),
        new SuspectInfo(Suspect.Ed, (bomb, suspects, eliminated) => bomb.GetModuleNames().Count(m => _EdModules.Contains(m) || m.Contains("Double-Oh")) == 1 ? eliminated[1] : Suspect.Ed),
        new SuspectInfo(Suspect.Ronny, (bomb, suspects, eliminated) => _VanillaModules.Intersect(bomb.GetModuleNames()).Any() && bomb.GetPortCount() < 4 ? Suspect.Ronny : eliminated[0]),
        new SuspectInfo(Suspect.Terry, (bomb, suspects, eliminated) => bomb.GetBatteryCount() >= 3 ? eliminated[2] : Suspect.Terry),
        new SuspectInfo(Suspect.Claira, (bomb, suspects, eliminated) => bomb.GetPortPlates().Count(pp => pp.Intersect(new[] { Port.RJ45, Port.StereoRCA, Port.PS2 }.Select(p => p.ToString())).Any()) >= 2 ? suspects[suspects.IndexOf(Suspect.Claira) ^ 1] : Suspect.Claira),
        new SuspectInfo(Suspect.Nick, (bomb, suspects, eliminated) => bomb.GetModuleNames().Intersect(_NickModules).Any() ? Suspect.Nick : eliminated[0]),
        new SuspectInfo(Suspect.Cob, (bomb, suspects, eliminated) => bomb.GetModuleNames().AnyDuplicates() ? suspects.CycleTo(Suspect.Cob).MaxElement(s => s.ToString().Length) : Suspect.Cob),
        new SuspectInfo(Suspect.Ash, (bomb, suspects, eliminated) => bomb.GetModuleNames().Any(m => m.Contains("Monsplode")) ? eliminated.Last() : Suspect.Ash),
        new SuspectInfo(Suspect.Don, (bomb, suspects, eliminated) => Suspect.Don),
        new SuspectInfo(Suspect.Jerry, (bomb, suspects, eliminated) => bomb.GetSolvableModuleNames().Intersect(_JerryModules).Any() ? suspects.After(Suspect.Jerry, suspects.Length - 1) : Suspect.Jerry),
        new SuspectInfo(Suspect.Simon, (bomb, suspects, eliminated) => bomb.GetModuleNames().Any(m => m.Contains("Simon")) ? Suspect.Simon : suspects[suspects.IndexOf(Suspect.Simon) ^ 1])
    )
        .ToDictionary(f => f.Name);

    void Start()
    {
        _moduleId = _moduleIdCounter++;
        _isSolved = false;

        StartCoroutine(Initialize(firstRun: true, startFrom: Rnd.Range(0, 8)));

        for (int i = 0; i < StickFigures.Length; i++)
            StickFigures[i].OnInteract = getInteract(i);
    }

    private KMSelectable.OnInteractHandler getInteract(int i)
    {
        return delegate
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, StickFigures[i].transform);
            StickFigures[i].AddInteractionPunch();

            if (_isSolved || _animating)
                return false;

            if (_suspects[i] == _godfather)
            {
                Debug.LogFormat("[Mafia #{0}] Clicked {1}: correct.", _moduleId, _suspects[i]);
                Module.HandlePass();
                StartCoroutine(Done(startFrom: i));
                _isSolved = true;
            }
            else
            {
                Debug.LogFormat("[Mafia #{0}] Clicked {1}: wrong.", _moduleId, _suspects[i]);
                Module.HandleStrike();
                StartCoroutine(Initialize(firstRun: false, startFrom: i));
            }
            return false;
        };
    }

    private IEnumerator Done(int startFrom)
    {
        for (int i = 1; i < _numSuspects; i++)
        {
            StickFigures[(i + startFrom) % _numSuspects].gameObject.SetActive(false);
            yield return new WaitForSeconds(.07f);
        }
    }

    private IEnumerator Initialize(bool firstRun, int startFrom)
    {
        _animating = true;
        yield return null;

        if (firstRun)
        {
            _startingTime = Bomb.GetTime() / 60;
            Debug.LogFormat("[Mafia #{0}] Bomb starting time: {1} minutes", _moduleId, _startingTime);
        }

        for (int i = 0; i < _numSuspects; i++)
        {
            StickFigures[(i + startFrom) % _numSuspects].gameObject.SetActive(false);
            if (!firstRun)
                yield return new WaitForSeconds(.05f);
        }

        _suspects = _allSuspects.ToArray().Shuffle().Take(_numSuspects).ToArray();
        var positions = "top left|top right|right top|right bottom|bottom right|bottom left|left bottom|left top".Split('|');
        for (int i = 0; i < _numSuspects; i++)
        {
            Debug.LogFormat("[Mafia #{0}] {1} suspect: {2}", _moduleId, positions[i], _suspects[i]);
            NameMeshes[i].text = _suspects[i].ToString();
        }

        var num = Bomb.GetSerialNumber().Select(c => c >= 'A' && c <= 'Z' ? c - 'A' + 1 : c - '0').Sum();
        Debug.LogFormat("[Mafia #{0}] Serial number sum is: {1} ({2}).", _moduleId, num, _allSuspects[(num - 1) % _allSuspects.Length]);
        while (!_suspects.Contains(_allSuspects[(num - 1) % _allSuspects.Length]))
            num++;
        Debug.LogFormat("[Mafia #{0}] First matching suspect is: {1} ({2}).", _moduleId, num, _allSuspects[(num - 1) % _allSuspects.Length]);

        var left = new List<Suspect>(_suspects);
        if (Bomb.GetIndicators().Count() >= 2)
        {
            left.Reverse();
            Debug.LogFormat("[Mafia #{0}] Eliminating suspects in counter-clockwise order.", _moduleId);
        }
        else
            Debug.LogFormat("[Mafia #{0}] Eliminating suspects in clockwise order.", _moduleId);

        var eliminated = new List<Suspect>();
        var ix = left.IndexOf(_allSuspects[(num - 1) % _allSuspects.Length]);
        var lastDigit = Bomb.GetSerialNumberNumbers().Last();
        for (int i = 0; i < _numSuspects - 1; i++)
        {
            eliminated.Add(left[ix]);
            Debug.LogFormat("[Mafia #{0}] Eliminating {1}.", _moduleId, left[ix]);
            left.RemoveAt(ix);
            ix = (ix + lastDigit) % left.Count;
        }

        Debug.LogFormat("[Mafia #{0}] Last remaining suspect is {1}.", _moduleId, left[0]);
        _godfather = _suspectInfos[left[0]].GetGodfather(Bomb, _suspects, eliminated.ToArray(), _startingTime);
        Debug.LogFormat("[Mafia #{0}] Godfather is {1}.", _moduleId, _godfather);

        yield return new WaitForSeconds(1.5f);
        var startFrom2 = Rnd.Range(0, _numSuspects);
        for (int i = 0; i < _numSuspects; i++)
        {
            StickFigures[(i + startFrom2) % _numSuspects].gameObject.SetActive(true);
            yield return new WaitForSeconds(.15f);
        }
        _animating = false;
    }

#pragma warning disable 414
    private string TwitchHelpMessage = @"Use !{0} execute <name> to execute the godfather.";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        var pieces = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (pieces.Length == 2 && pieces[0].Equals("execute", StringComparison.InvariantCultureIgnoreCase) && !_isSolved && !_animating)
        {
            yield return null;
            for (int i = 0; i < _suspects.Length; i++)
            {
                if (_suspects[i].ToString().Equals(pieces[1], StringComparison.InvariantCultureIgnoreCase))
                {
                    StickFigures[i].OnInteract();
                    yield return new WaitForSeconds(.5f);
                    yield break;
                }
            }
            yield return string.Format("sendtochat Who is {0}? Make sure you spell the name right.", pieces[1]);
        }
    }
}
