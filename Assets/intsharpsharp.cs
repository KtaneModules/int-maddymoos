using System.Collections;
using UnityEngine;
using System.Linq;
using System;
using Rnd = UnityEngine.Random;

public class intsharpsharp : MonoBehaviour
{
    //import assets
    public KMAudio Audio;
    public KMBombModule Module;
    public KMBombInfo Bomb;
    public TextMesh[] Text;
    public KMSelectable[] Buttons;
	private string Chars = ".,<>-+{}";
	private string Stuff = "34567890345678904747";
	private string Store = "";
	private char[] Store2 = new char[20];
	private int[] Pointer = new int[4];
	private int[] Innards = new int[6];
	private bool Solved, Started;
	static private int _moduleIdCounter = 1;
	private int _moduleId;

	void Awake () {
        _moduleId = _moduleIdCounter++;
        for (byte i = 0; i < Buttons.Length; i++)
        {
            byte j = i;
            Buttons[j].OnInteract += delegate
            {
                HandlePress(Buttons[j]);
                return false;
            };
        }
	}	
	void HandlePress(KMSelectable btn) {
		int X = Array.IndexOf(Buttons,btn);
		btn.AddInteractionPunch();
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, btn.transform);
		if(Solved)
			return;
		if(X==1){
			Pointer[3]= (Pointer[3]+1)%8;
            if (Pointer[3] == 1)
                Text[1].gameObject.transform.localPosition = new Vector3(0f, 0.0123f, 0.002f);
            else if (Pointer[3] == 2)
                Text[0].gameObject.transform.localPosition = new Vector3(0f, 0.0123f, 0.002f);
            else if (Pointer[3] == 5)
                Text[1].gameObject.transform.localPosition = new Vector3(0f, 0.0123f, 0f);
            else if (Pointer[3] == 6)
                Text[0].gameObject.transform.localPosition = new Vector3(0f, 0.0123f, 0f);
        }
		else{
			switch(Chars[Pointer[3]]){
				case '.': Innards[3] = 0;Innards[4] = 0;Innards[5] = 0;Pointer[2]=0;Started=false;break;
				case ',': Innards[3] = 0;Innards[4] = 0;Innards[5] = 0;Pointer[2]=0;break;
				case '<': if(Started){Innards[Pointer[2]+3]=(Innards[Pointer[2]+3]+9)%10;} break;
				case '>': if(Started){Innards[Pointer[2]+3]=(Innards[Pointer[2]+3]+1)%10;} break;
				case '-': if(Started){Pointer[2]=(Pointer[2]+2)%3;} break;
				case '+': if(Started){Pointer[2]=(Pointer[2]+1)%3;} break;
				case '{': Started=true; break;
				case '}': bool yes = true; if(!Started) yes=false;
                for (int i=0;i<3;i++){ 
				if (Innards[i]!=Innards[i+3])
					{yes=false;}}
				if(yes){
					Module.HandlePass(); Solved=true; Debug.LogFormat("[int## #{0}] Inputted digits {1}{2}{3}, module solved", _moduleId, Innards[3], Innards[4], Innards[5]);}
				else{
					Module.HandleStrike(); if (!Started) Debug.LogFormat("[int## #{0}] Input was never started with an open curly bracket, strike", _moduleId); else Debug.LogFormat("[int## #{0}] Inputted digits {1}{2}{3}, strike", _moduleId, Innards[3], Innards[4], Innards[5]); Innards[3] = 0;Innards[4] = 0;Innards[5] = 0;Pointer[2]=0;Started=false;} 
				break;
			}

		}
		Text[0].text=Chars[Pointer[3]].ToString();
		Text[1].text=Chars[(Pointer[3]+1)%8].ToString();
	}
	void Start () {
		Store2 = Stuff.ToArray().Shuffle();
		for(int i=0;i<2;i++){
			Store2[Rnd.Range(0+i*10,10+i*10)] = (i+1).ToString().ToArray()[0];
		}
		Store = new string(Store2);
		Text[2].text = Store;
        Debug.LogFormat("[int## #{0}] The program is {1}",_moduleId,Store);
        Interpreter();
	}
	
	void Interpreter()
    {
        int State = 0;
        // 0 = Normal
        // 1 = Reached 1
        // 2 = Reached 2
        // 3 = Reached End 1
        // 4 = Reached End 2
        // 5 = Death
        while (true)
        {
            switch (Store[Pointer[1]])
            {
                case '1': Console.WriteLine('1'); if (State == 0) { State = 1; Pointer[1] = Store.IndexOf('2'); } break;
                case '2': Console.WriteLine('2'); if (State == 4) State = 5; if (State == 0 || State == 3) State = 2; break;
                case '3': Console.WriteLine('3'); Pointer[0] = (Pointer[0] + 2) % 3; break;
                case '4': Console.WriteLine('4'); Innards[Pointer[0]]=(Innards[Pointer[0]]+1)%10; break;
                case '5': Console.WriteLine('5'); Pointer[0] = (Pointer[0] + 1) % 3; break;
                case '6': Console.WriteLine('6'); Pointer[0] = 0; break;
                case '7': Console.WriteLine('7'); Innards[Pointer[0]]=(Innards[Pointer[0]]+9)%10; break;
                case '8': Console.WriteLine('8'); Pointer[0] = 2; break;
                case '9': Console.WriteLine('9'); Innards[0] = (Innards[0] + 1) % 10; Innards[1] = (Innards[1] + 1) % 10; Innards[2] = (Innards[2] + 1) % 10; break;
                case '0': Console.WriteLine('0'); Pointer[0] = 1; break;
            }
            if (State == 5) break;
            Pointer[1]++;
            if (Pointer[1] >= Store.Length)
            {
                // State 3 isn't possible here
                if (State == 1) State = 3;
                if (State == 2) State = 4;
                Pointer[1] = Store.IndexOf('1');
            }
        }
		Debug.LogFormat("[int## #{0}] The three computed digits are {1}{2}{3}",_moduleId,Innards[0],Innards[1],Innards[2]);
	}

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} (.,<>-+) [Inputs the specified command] | Parentheses represent curly brackets";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        char[] valids = new char[] { '(', ')', '.', ',', '<', '>', '-', '+' };
        for (int i = 0; i < command.Length; i++)
        {
            if (!valids.Contains(command[i]))
            {
                yield return "sendtochaterror The specified character '" + command[i] + "' is invalid!";
                yield break;
            }
        }
        command = command.Replace("(", "{");
        command = command.Replace(")", "}");
        yield return null;
        for (int i = 0; i < command.Length; i++)
        {
            while (Text[0].text != command[i].ToString())
            {
                Buttons[1].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            Buttons[0].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (!Started)
        {
            while (Text[0].text != "{")
            {
                Buttons[1].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            Buttons[0].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        bool happened1 = false;
        bool happened2 = false;
        int[] order = new int[] { Pointer[2], (Pointer[2] + 1) % 3, (Pointer[2] + 2) % 3 };
        for (int i = 0; i < 3; i++)
        {
            if (Innards[order[i]] != Innards[order[i] + 3])
            {
                if (i == 0)
                    happened1 = true;
                else if ((i == 1 && happened1) || (i == 2 && happened2 && happened1))
                {
                    if (i == 1)
                        happened2 = true;
                    while (Text[0].text != "+")
                    {
                        Buttons[1].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                    Buttons[0].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
                else if (i == 2 && happened1 && !happened2)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        while (Text[0].text != "+")
                        {
                            Buttons[1].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                        Buttons[0].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                int count1 = Pointer[3] + Chars.IndexOf('<');
                int count2 = Pointer[3] + Chars.IndexOf('>');
                int count1old = Pointer[3] + Chars.IndexOf('<');
                int count2old = Pointer[3] + Chars.IndexOf('>');
                int left = Innards[order[i] + 3];
                int right = Innards[order[i] + 3];
                while (left != Innards[order[i]])
                {
                    left--;
                    if (left < 0)
                        left = 9;
                    count1++;
                }
                while (right != Innards[order[i]])
                {
                    right++;
                    if (right > 9)
                        right = 0;
                    count2++;
                }
                if (count1 < count2)
                {
                    while (Text[0].text != "<")
                    {
                        Buttons[1].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                    for (int k = 0; k < (count1 - count1old); k++)
                    {
                        Buttons[0].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                else if (count1 > count2)
                {
                    while (Text[0].text != ">")
                    {
                        Buttons[1].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                    for (int k = 0; k < (count2 - count2old) ; k++)
                    {
                        Buttons[0].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                else
                {
                    int choice = Rnd.Range(2, 4);
                    while (Text[0].text != Chars[choice].ToString())
                    {
                        Buttons[1].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                    for (int k = 0; k < (choice == 2 ? (count1 - count1old) : (count2 - count2old)); k++)
                    {
                        Buttons[0].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }
        while (Text[0].text != "}")
        {
            Buttons[1].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        Buttons[0].OnInteract();
    }
}
