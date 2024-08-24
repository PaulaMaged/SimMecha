using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using System.Text;
using System;

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
    public GameObject urdfLoaderGameObject;

    private List<GameObject> Robots;
    private List<string> urls;

    private List<string> motorNames;
    private List<int> robotNums;
    private List<string> linkNames;
    private List<Dictionary<string, object>> motorParamsList;
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
            float scaling = (transform.localScale.x + transform.localScale.y + transform.localScale.z)/3;

            string message = urls[i] + ",, " + pos.ToString() + ",, " + orientation.ToString() + ",, " + scaling.ToString();
            MySender.Instance.SendMessage(message);
        }
    }

    public void SendMotorData()  //not yet implemented
    {
        motorNames = new List<string> { "PermMagnetSynch", "PermMagnetSynch" };
        robotNums = new List<int> { 0, 0 };
        linkNames = new List<string> { "panda_link1", "panda_link4" };
        motorParamsList = new List<Dictionary<string, object>>();

        motorParamsList.Add(new Dictionary<string, object>
        {
            { "r_s", 18e-2 },  // Stator resistance
            { "l_d", 0.37e-2 },  // Direct axis inductance
            { "l_q", 1.2e-2 },  // Quadrature axis inductance
            { "p", 3 },  // Pole pair number
            { "j_rotor", 0.03883 }  // Rotor moment of inertia
        });

        // Add another dictionary dynamically
        motorParamsList.Add(new Dictionary<string, object>
        {
            { "r_s", 20e-2 },
            { "l_d", 0.4e-2 },
            { "l_q", 1.5e-2 },
            { "p", 4 },
            { "j_rotor", 0.040 }
        });

        string motors = string.Join(", ", motorNames);
        string robots = string.Join(", ", robotNums);
        string links = string.Join(", ", linkNames);
        string motorParams = JsonSerializer.SerializeToCustomFormat(motorParamsList);

        string message = $"({motors}), ({robots}), ({links}), ({motorParams})";
        MySender.Instance.SendMessage("\n" + message);
    }

}
