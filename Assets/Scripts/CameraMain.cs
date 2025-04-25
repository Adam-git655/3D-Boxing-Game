using UnityEngine;

public class CameraMain : MonoBehaviour
{

    public bool CanStartGame = false;

    public void OnIntroCutsceneEnd()
    {
        CanStartGame = true;
    }
}
