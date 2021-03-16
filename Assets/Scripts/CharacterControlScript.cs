using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControlScript : CharMain {

    void Awake() {
    }

    protected override void Update() {

        if (KeyboardKeys.LeftCTRL) {
            CharacterCrouch();            
        }

        if (KeyboardKeys.ResetCharacter) {
            ResetCharacter();
        }
        if (KeyboardKeys.ButtonY)
        {
            CharacterJump();
        }
        ControlCharacter();
        MouseMovement();

        base.Update();
    }

  void MouseMovement() {

        if (KeyboardKeys.MouseAxisX != 0 || KeyboardKeys.RightStickAxisY != 0) {
            MoveCamera(KeyboardKeys.MouseAxisX, KeyboardKeys.RightStickAxisY);
        }
    }

    //Движения персонажа
    private void ControlCharacter() {
        MovCharact(KeyboardKeys.KeyWS, KeyboardKeys.KeyAD, KeyboardKeys.LeftShift, KeyboardKeys.LeftTrigger);
    }
}
