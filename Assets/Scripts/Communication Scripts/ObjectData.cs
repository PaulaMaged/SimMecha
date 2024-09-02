using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using System.Text;
using System;
using System.Threading.Tasks;
// using UnityEditor.Experimental.GraphView;
using UIScripts;

public class JsonSerializer
{
    public static string SerializeToCustomFormat(List<Dictionary<string, object>> data)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < data.Count; i++)
        {
            var dict = data[i];
            sb.Append("{");

            int dictCount = 0;
            foreach (var kvp in dict)
            {
                if (dictCount > 0)
                {
                    sb.Append(", ");
                }

                sb.AppendFormat("{0}: ", EscapeString(kvp.Key));
                sb.AppendFormat("{0}", FormatValue(kvp.Value));

                dictCount++;
            }

            sb.Append("}");

            if (i < data.Count - 1)
            {
                sb.Append(", ");
            }
        }

        return sb.ToString();
    }

    private static string EscapeString(string str)
    {
        // Escape special characters for valid output
        return $"'{str.Replace("'", "''")}'";
    }

    private static string FormatValue(object value)
    {
        if (value is string strValue)
        {
            return EscapeString(strValue);
        }
        else if (value is float || value is double || value is int || value is long)
        {
            // Format number to scientific notation
            return value is double dValue ? dValue.ToString("G17") : value.ToString();
        }
        else if (value is Dictionary<string, object> dictValue)
        {
            return SerializeToCustomFormat(new List<Dictionary<string, object>> { dictValue });
        }
        else if (value is List<object> listValue)
        {
            var sb = new StringBuilder();
            sb.Append("(");
            for (int i = 0; i < listValue.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(FormatValue(listValue[i]));
            }
            sb.Append(")");
            return sb.ToString();
        }
        else
        {
            throw new NotSupportedException($"Unsupported type: {value.GetType()}");
        }
    }
}


// --------------------------------------------------------------------


public class ObjectData : MonoBehaviour
{
    private RuntimeURDFLoader urdfLoader;
    private ShowHierarchyMenu showHierarchyMenu;
    private AddConstraintController addConstraintController;

    public GameObject urdfLoaderGameObject;
    public GameObject showHierarchyMenuGameObject;
    public GameObject addConstraintControllerGameObject;

    private List<GameObject> Robots;
    private List<string> urls;

    private List<string> motorNames;
    private List<int> robotNums;
    private List<string> linkNames;
    private List<Dictionary<string, object>> motorParamsList;


    void Awake()
    {
        urdfLoader = urdfLoaderGameObject.GetComponent<RuntimeURDFLoader>();
        showHierarchyMenu = showHierarchyMenuGameObject.GetComponent<ShowHierarchyMenu>();
        addConstraintController = addConstraintControllerGameObject.GetComponent<AddConstraintController>();
    }

    public async void sendRobotData()
    {
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
            float scaling = (transform.localScale.x + transform.localScale.y + transform.localScale.z)/3;

            string message = urls[i] + ", " + pos.ToString() + ", " + orientation.ToString() + ", " + scaling.ToString();
            MySender.Instance.SendMessage("\n" + message);
            await Task.Delay(20);
        }
    }

    public async void SendMotorData()  //not yet implemented
    {
        showHierarchyMenu.PopulateFinalLists();

        motorNames = showHierarchyMenu.GetMotorNames();
        robotNums = showHierarchyMenu.GetRobotId();
        linkNames = showHierarchyMenu.GetLinkNames();
        motorParamsList = showHierarchyMenu.GetMotorParameters();

        for (int i = 0; i < motorNames.Count; i++)
        {
            // Get the corresponding motor data for index i
            string motor = motorNames[i];
            string robot = robotNums[i].ToString();
            string link = linkNames[i];
            string motorParam = JsonSerializer.SerializeToCustomFormat(new List<Dictionary<string, object>> { motorParamsList[i] });

            // Create the message with individual motor details
            string message = $"({motor}, {robot}, {link}, {motorParam})";

            // Send the message for this motor
            MySender.Instance.SendMessage("\n" + message);
            await Task.Delay(20);
        }

    }

    public async void SendConstraintsData()
    {
        addConstraintController.PopulateConstraintStringsList();
        List<string> constraints = addConstraintController.GetConstraintStrings();

        foreach (string constraint in constraints)
        {
            MySender.Instance.SendMessage("\n" + constraint);
            await Task.Delay(20);
        }
    }

    public void SendAllData()
    {
        sendRobotData();
        SendMotorData();
        SendConstraintsData();
    }


    async Task Delayed(int milli)
    {
        await Task.Delay(milli);
    }

}
