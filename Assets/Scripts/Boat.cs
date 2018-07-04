using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boat : MonoBehaviour {

    ThirdPersonController tpc;
    public Rigidbody rb;
    //UI walking
    Image symbol; // 2d sprite renderer icon reference
    AnimateUI symbolAnimator;
    public Sprite[] getInBoat; // walking feet cursor
    public float withinClickDist = 12;

    bool changedSprites;

    void Start () {
        tpc = GameObject.FindGameObjectWithTag("Player").GetComponent<ThirdPersonController>();
        rb = GetComponent<Rigidbody>();

        // get in boat UI
        symbol = GameObject.FindGameObjectWithTag("Symbol").GetComponent<Image>(); //searches for InteractSymbol
        symbolAnimator = symbol.GetComponent<AnimateUI>();
        rb.isKinematic = true;
    }

    void OnMouseOver()
    {
        if (!tpc.inBoat && !changedSprites && Vector3.Distance(tpc.transform.position, transform.position) < withinClickDist )
        {
            symbolAnimator.animationSprites = getInBoat;
            symbolAnimator.active = true;
            changedSprites = true;
            tpc.walkingSpritesOn = false;
            tpc.touchingSomething = true;
        }
    }

    void OnMouseExit()
    {
        changedSprites = false;
        tpc.touchingSomething = false;
    }

    void OnMouseDown()
    {
        if (!tpc.inBoat)
        {
            tpc.inBoat = true;
            tpc.boat = gameObject;
            tpc.boatBody = rb;
            tpc.ChangeAnimState(tpc.boatIdle);
        }
    }
}
