using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private PlayerController controller;

    private void Start()
    {
        controller = GetComponent<PlayerController>();
    }

    private void Update()
    {
        ControllerInput();
    }

    private void ControllerInput()
    {
        if (!controller)
            return;

        controller.VInput = Input.GetAxisRaw("Vertical");
        controller.HInput = Input.GetAxisRaw("Horizontal");
    }
}
