using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class Billboards : MonoBehaviour
{
    public Transform Player;

    private void Update()
    {
        foreach (Transform targetTransform in transform) {
            targetTransform.LookAt(Player);
            targetTransform.rotation = Quaternion.Euler(90, targetTransform.rotation.eulerAngles.y, 0);
        }
    }
}