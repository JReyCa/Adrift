using Gravity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : RogueObject
{
    #region Inspector Variables
    [SerializeField]
    private float moveSpeed = 10.0f;
    #endregion

    private Transform cameraT;  // Camera transform
    private bool isFloatingFree = false;

    public float VInput { private get; set; }   // Vertical input
    public float HInput { private get; set; }   // Horizontal input

    protected override void Start()
    {
        base.Start();
        rbody.constraints = RigidbodyConstraints.FreezeRotation;

        cameraT = Camera.main.transform;
    }

    protected override void Motion()
    {
        if (isFloatingFree)
            base.Motion();

        GravityCentre nearestCentre = GravityManager.GetNearestCentre(transform);
        Vector3 up = transform.position - nearestCentre.transform.position;

        Vector3 moveDirection = VInput * transform.forward;
        rbody.AddForce(moveDirection * moveSpeed);
    }
}
