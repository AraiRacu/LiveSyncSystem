
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FadeJackAnimControl : UdonSharpBehaviour
{
    public void CloseObject()
    {
        this.gameObject.SetActive(false);
    }
}
