using UnityEngine;
using System.Collections.Generic;

public class parse : MonoBehaviour
{
    public TextAsset file;

    public List<Vector3> ParseFile()
	{
		float ScaleFactor = 1.0f / 39.37f;
		List<Vector3> positions = new List<Vector3>();

        if (file == null)
        {
            Debug.LogError("TextAsset file not assigned");
            return positions;
        }

        //string content = file.ToString();
		string[] lines = file.text.Split('\n');

      
        for (int i = 0; i < lines.Length; i++)
		{
			string[] coords = lines[i].Split(' ');
			Vector3 pos = new Vector3(float.Parse(coords[0]), float.Parse(coords[1]), float.Parse(coords[2]));
			positions.Add(pos * ScaleFactor);
		}
		return positions;
	}
}