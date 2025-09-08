using UnityEngine;

public class Trap : MonoBehaviour
{
    public string trapId = "";
    public string trapType = "generic";

    void Reset()
    {
        if (string.IsNullOrEmpty(trapId))
            trapId = gameObject.name;
    }
}