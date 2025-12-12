using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMoveControll : MonoBehaviour
{
    public new Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.current;
    }

    public IEnumerator MoveCameraTo(Vector3 targetPos, float moveSpeed)
    {
        while (Vector3.Distance(camera.transform.position, targetPos) > 0.01f)
        {
            camera.transform.position = Vector3.MoveTowards(
                camera.transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        camera.transform.position = targetPos; // «O©³¹ï»ô
    }

}
