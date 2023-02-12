using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using System.Text.RegularExpressions;

public class LetterMathModule : MonoBehaviour
{

    private static int _moduleIdCounter = 1;
    private int _moduleID = 0;
    private bool _moduleSolved;

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable[] Buttons;
    public TextMesh[] ButtonTexts;
    public TextMesh ScreenText;

    private readonly string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private int[] characters = new int[2];
    private bool _operator; //False = -, true = +
    private int answer;
    private int[] choices = new int[6];
    private int correctButton;
    public Color[] colors;
    private string[] colorOptions = { "red", "white", "blue", "green", "magenta", "yellow" };
    private string[] textColors = new string[6];


    void Awake()
    {
        _moduleID = _moduleIdCounter++;
        for (int i = 0; i < 2; i++)
            characters[i] = Rnd.Range(0, letters.Length);

        _operator = Rnd.Range(0, 2) == 0;

        if (_operator)
            answer = (characters[0] + 1) + (characters[1] + 1);
        else
            answer = (characters[0] + 1) - (characters[1] + 1);

        correctButton = Rnd.Range(0, Buttons.Length);

        for (int i = 0; i < Buttons.Length; i++)
            choices[i] = answer;
        for (int i = 0; i < Buttons.Length; i++)
            while (i != correctButton && (choices[i] == answer || Enumerable.Range(0, 3).Any(x => x != i && choices[x] == choices[i])))
                choices[i] = Rnd.Range(-25, 50);



        for (int btn = 0; btn < Buttons.Length; btn++)
        {
            int randColor = Rnd.Range(0, colors.Length);
            textColors[btn] = colorOptions[randColor];
            ButtonTexts[btn].GetComponent<TextMesh>().color = colors[randColor];
        }

       


    }

    // Use this for initialization
    void Start()
    {
        ColorChange();
        for (int btn = 0; btn < Buttons.Length; btn++)
        {
            Buttons[btn].OnInteract = ButtonPressed(btn);
            ButtonTexts[btn].text = choices[btn].ToString();


        }
        ScreenText.text = letters[characters[0]].ToString() + (_operator ? " + " : " - ") + letters[characters[1]].ToString();
        Log("The display is {0}", ScreenText.text);
        Log("The correct answer to the equation that has been generated is {0}", answer);
        Log("The correct button to press is button {0} which says {1} on it.", correctButton+1, choices[correctButton]);
    }

    private KMSelectable.OnInteractHandler ButtonPressed(int btn)
    {
        return delegate
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Buttons[btn].transform);
            Buttons[btn].AddInteractionPunch();
            if (!_moduleSolved)
            {
                if (btn == correctButton)
                {
                    _moduleSolved = true;
                    Module.HandlePass();
                    ScreenText.text = "✓";
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                    Log("Correct button has been pressed. Module solved!");
                    StartCoroutine(CorrectAnim());
                }
                else
                {
                    Module.HandleStrike();
                    Log("Incorrect button has been pressed. You have pressed {0}. I was expecting {1} which is actually {2}.", choices[btn], choices[correctButton], answer);
                }
            }
            return false;
        };
    }

    IEnumerator CorrectAnim()
    {
        string CorrectText = "GG!✓✓✓";
        for (int i = 0; i < Buttons.Length; i++)
        {
            ButtonTexts[i].color = new Color32(6, 190, 0, 255);
            ButtonTexts[i].text = CorrectText[i].ToString();
            yield return new WaitForSeconds(0.2f);

        }
        yield return null;
    }

    void ColorChange()
    {
        for (int i = 0; i < Buttons.Length; i++)
        {
            Debug.Log(textColors[i]);
            switch (textColors[i])
            {
                
                case "red":
                    choices[i] -= 13;
                    break;
                case "white":
                    choices[i] += (Bomb.GetBatteryCount() * Bomb.GetOffIndicators().Count());
                    break;
                case "blue":
                    choices[i] -= (Bomb.GetOnIndicators().Count() - Bomb.GetSerialNumberNumbers().Last());
                    break;
                case "green":
                    choices[i] += (Bomb.GetPortCount() + Bomb.GetPortPlateCount());
                    break;
                case "magenta":
                    choices[i] += (Bomb.GetModuleNames().Count() * 2);
                    break;
                case "yellow":
                    choices[i] -= ((Bomb.GetPortPlateCount() - Bomb.GetBatteryHolderCount()) * 3);
                    break;

            }

        }
    }

    private void Log(string message, params object[] args)
    {
        Debug.LogFormat("[Letter Math #{0}] {1}", _moduleID, string.Format(message, args));
    }
#pragma warning disable 0414
    private readonly string TwitchHelpMessage = "!{0} press 1-6 | Presses buttons 1-6 in reading order.";
#pragma warning restore 0414

    KMSelectable[] ProcessTwitchCommand(string command)
    {
        if (_moduleSolved)
            return null;
        var m = Regex.Match(command, @"^\s*(press\s+)?([1-6])\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!m.Success)
            return null;
        return new[] { Buttons[m.Groups[2].Value[0] - '1'] };
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (!_moduleSolved)
        {
            Buttons[correctButton].OnInteract();
            yield return new WaitForSeconds(.1f);
        }
    }
}
