using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UniqueNamer : ScriptableObject
{
    private List<string> keys = new List<string>();
    private List<string> names = new List<string>();

    public void AddName(string key, string name, int count = 0)
    {
        string nameToAdd = count > 0 ? name + count : name;

        if (names.Contains(nameToAdd))
        {
            AddName(key, name, ++count);
        }
        else
        {
            int index = keys.IndexOf(key);

            if (index >= 0)
            {
                names[index] = nameToAdd;
            }
            else
            {
                keys.Add(key);
                names.Add(nameToAdd);
            }
        }
    }

    public string GetName(string key)
    {
        int index = keys.IndexOf(key);

        if (index >= 0)
        {
            return names[index];
        }
        else
        {
            Debug.LogError("[OnGetName] Key not found");
            return null;
        }
    }

    public string GenerateUniqueName(string key, string name)
    {
        AddName(key, name);
        return GetName(key);
    }

    public void RemoveName(string key)
    {
        int index = keys.IndexOf(key);

        if (index >= 0)
        {
            keys.RemoveAt(index);
            names.RemoveAt(index);
        }
        else
        {
            Debug.LogError("[OnDelete] Key not found");
        }
    }
}