using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;



public class ObjectData : MonoBehaviour
{
    private RuntimeURDFLoader urdfLoader;
    public GameObject urdfLoaderGameObject;

    private List<GameObject> Robots;
    private List<string> urls;

    public void sendObjectData()
    {
        urdfLoader = urdfLoaderGameObject.GetComponent<RuntimeURDFLoader>();

        Robots = urdfLoader.GetImportedRobots();
        urls = urdfLoader.GetUrls();

        if(Robots.Count != urls.Count) {
            Debug.Log("url number is not equal to robot number");
            return;
        }

        for(int i=0; i<Robots.Count; i++)
        {
            Transform transform = Robots[i].transform;
            Vector3 pos = transform.position;
            Quaternion orientation = transform.rotation;

            //not sent yet 
            float scaling = (transform.localScale.x + transform.localScale.y + transform.localScale.z)/3;

            MySender.Instance.ReceiveData(urls[i], pos, orientation);
        }
    }

}
