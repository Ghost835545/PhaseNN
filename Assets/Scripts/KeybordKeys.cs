using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KeyboardKeys {

    public static float MouseAxisX { get { return Input.GetAxis("MouseAxisX"); } }
    public static float RightStickAxisY { get { return Input.GetAxis("Right Stick Y"); } }
    public static float KeyWS { get { return Input.GetAxis("Horizontal"); } }
    public static float KeyAD { get { return Input.GetAxis("Vertical"); } }
    public static bool LeftCTRL { get { return Input.GetButton("Crouch"); } }
    public static bool ResetCharacter { get { return Input.GetButton("ResetCharacter"); } }

    public static float LeftTrigger { get { return Input.GetAxis("Left Trigger"); } }
    public static float LeftShift { get { return Input.GetAxis("Run"); } }
   
    public static bool ButtonY { get { return Input.GetButton("Jump"); } }
 


}
