using UnityEngine;

public class Interact_Default : MonoBehaviour, IInteraction
{

    public void Interact()
    {
        Debug.Log("Hellooo , you interacted with " + gameObject.name);
    }
}
