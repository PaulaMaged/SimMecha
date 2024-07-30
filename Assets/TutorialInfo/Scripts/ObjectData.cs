using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectData : MonoBehaviour
{
    public GameObject[] gameObjects; // Array to hold the GameObjects

    public void sendObjectData()
    {
        foreach (GameObject obj in gameObjects)
        {
            Vector3 pos = obj.transform.position;
            Quaternion orientation = obj.transform.rotation;

            //pos = new Vector3(pos.x, pos.z, pos.y);
            // Send the data to the receiver script
            MySender.Instance.ReceiveData(obj.name, pos, orientation);
        }
    }

}
