using UnityEngine;
using System;

public enum DataType
{
    Int,
    Float
}

public class DataManager : MonoBehaviour
{
    private static Data[] data;

    [SerializeField] private Data[] customData;

    [Space]
    [SerializeField] private bool deleteAll;

    void Awake()
    {
        if (data != null) return;

        if (deleteAll)
        {
            PlayerPrefs.DeleteAll();
        }

        CreateData();

        data = customData;
    }

    /// <summary>
    /// Creates new data if they do not exist in the PlayerPrefs registers yet.
    /// </summary>
    private void CreateData()
    {
        foreach (Data data in customData)
        {
            if (data.Type == DataType.Int)
            {
                int retrievedData = PlayerPrefs.GetInt(data.Name);
                if (data.Delete)
                {
                    PlayerPrefs.DeleteKey(data.Name);
                    print($"Data named '{data.Name}' has been deleted with a result of success");
                    continue;
                }

                if (retrievedData == 0)
                {
                    PlayerPrefs.SetInt(data.Name, 0);
                    print($"Data named '{data.Name}' of type 'int' has been created or reset with a result of success");
                }
            }
            else if (data.Type == DataType.Float)
            {
                float retrievedData = PlayerPrefs.GetFloat(data.Name);
                if (data.Delete)
                {
                    PlayerPrefs.DeleteKey(data.Name);
                    print($"Data named '{data.Name}' has been deleted with a result of success");
                    continue;
                }

                if (retrievedData == 0)
                {
                    PlayerPrefs.SetFloat(data.Name, 0);
                    print($"Data named '{data.Name}' of type 'float' has been created or reset with a result of success");
                }
            }
        }
    }

    /// <summary>
    /// Gets the value of the data with the name given.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="name">The name of the data.</param>
    /// <returns>The value of the data with the name given.</returns>
    public static T GetData<T>(string name)
    {
        foreach (Data data in data)
        {
            if (data.Name == name)
            {
                if (data.Delete)
                {
                    print($"You are trying to retrieve data named '{data.Name}' but it has just been deleted");
                    return default;
                }

                return RetrieveData<T>(data);
            }
        }

        print($"You are trying to retrieve data named '{name}' but it does not exist");
        return default;
    }

    /// <summary>
    /// Retrieves tha data given from the PlayerPrefs registers.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="data">The data to retrieve.</param>
    /// <returns>The value of the data retrieved.</returns>
    private static T RetrieveData<T>(Data data)
    {
        if (data.Type == DataType.Int)
        {
            return (T)Convert.ChangeType(PlayerPrefs.GetInt(data.Name), typeof(int));
        }
        else if (data.Type == DataType.Float)
        {
            return (T)Convert.ChangeType(PlayerPrefs.GetFloat(data.Name), typeof(float));
        }

        print($"Invalid data type. Data type: {data.Type}");
        return default;
    }

    /// <summary>
    /// Saves the value to the data with the name given.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="name">The name of the data.</param>
    /// <param name="value">The value to store.</param>
    public static void SaveData<T>(string name, T value)
    {
        foreach (Data data in data)
        {
            if (data.Name == name)
            {
                if (data.Delete)
                {
                    print($"You are trying to set data named '{data.Name}' but it has just been deleted");
                    return;
                }

                SetData(data, value);
                return;
            }
        }

        print($"You are trying to set data named '{name}' but it does not exist");
        return;
    }

    /// <summary>
    /// Sets the value to the data given in the PlayerPrefs registers.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="data">The data to store.</param>
    /// <param name="value">The value to store.</param>
    private static void SetData<T>(Data data, T value)
    {
        if (data.Type == DataType.Int)
        {
            int v = (int)Convert.ChangeType(value, typeof(T));
            PlayerPrefs.SetInt(data.Name, v);
            return;
        }
        else if (data.Type == DataType.Float)
        {
            float v = (float)Convert.ChangeType(value, typeof(T));
            PlayerPrefs.SetFloat(data.Name, v);
            return;
        }

        print($"Invalid data type. Data type: {data.Type}");
        return;
    }
}

[Serializable]
public class Data
{
    #region Public References
    public string Name => name;
    public DataType Type => type;
    public bool Delete => delete;
    #endregion

    [SerializeField] private string name;
    [SerializeField] private DataType type;
    [SerializeField] private bool delete;
}