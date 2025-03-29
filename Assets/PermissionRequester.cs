using UnityEngine;
using UnityEngine.Android;

public class PermissionRequester : MonoBehaviour
{
    void Awake()
    {

        // Check if the camera permission is granted
        Permission.RequestUserPermission(Permission.Camera);

}
}
