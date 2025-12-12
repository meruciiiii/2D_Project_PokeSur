using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public void OnButtonPressed()
    {
        // ¾À ·Îµå
        SceneManager.LoadScene("GmaeScene");

    }
}
