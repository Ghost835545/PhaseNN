using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterfaceScript : MonoBehaviour {

    private Text testText;

    public enum DebugType {
        KeyboardKeys,
        Traject,
        Bodies
    }
    public DebugType debug;

    void Awake() {

        testText = GameObject.Find("DebugText").GetComponent<Text>();        
    }


    void Start () {

    }
	
	
	void Update () {

        if (debug == DebugType.KeyboardKeys) {
            PrintGamepadInputs();

        } else if (debug == DebugType.Traject) {
            PrintTrajectoryData();

        } else if (debug == DebugType.Bodies) {


        }
        
    }

    private void PrintGamepadInputs() {

        string output = "";

        output += "Left stick Axis X: " + KeyboardKeys.KeyWS + "\n";
        output += "Left stick Axis Y: " + KeyboardKeys.KeyAD + "\n";
        output += "MouseAxisX: " + KeyboardKeys.MouseAxisX + "\n";
        output += "Crouch: " + KeyboardKeys.LeftCTRL + "\n";
        output += "Button Y: " + KeyboardKeys.ButtonY + "\n";
        output += "Left trigger: " + KeyboardKeys.LeftTrigger + "\n";
        output += "Left Shift: " + KeyboardKeys.LeftShift + "\n";
        output += "Left ALT: " + KeyboardKeys.ResetCharacter + "\n";
  

        

        testText.text = output;
    }

    private void PrintTrajectoryData() {

        string output = "";



        testText.text = output;

    }

}
