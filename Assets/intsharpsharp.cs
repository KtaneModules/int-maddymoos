using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using KModkit;
using System.Text.RegularExpressions;
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
		Buttons[X].AddInteractionPunch();
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Buttons[X].transform);
		if(Solved)
			return;
		if(X==1){
			Pointer[3]= (Pointer[3]+1)%8;
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
				case '}': bool yes=true; 
				for(int i=0;i<3;i++){ 
				if (Innards[i]!=Innards[i+3])
					{yes=false;}}
				if(yes){
					Module.HandlePass(); Solved=true;}
				else{
					Module.HandleStrike();Innards[3] = 0;Innards[4] = 0;Innards[5] = 0;Pointer[2]=0;Started=false;} 
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
		Interpreter();
	}
	
	// Update is called once per frame
	void Update () {
		
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
		Debug.LogFormat("[int## #{0}]: The answer is {1}{2}{3}",_moduleId,Innards[0],Innards[1],Innards[2]);
	}
}
