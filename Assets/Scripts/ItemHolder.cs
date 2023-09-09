using System.IO;
using TMPro;
using UnityEngine;
public class ItemHolder : MonoBehaviour
{
    public TextMeshProUGUI SaveNumberText;

    public TextMeshProUGUI SavePlaytimeText;

    public TextMeshProUGUI SaveDateTimeText;

    public int SaveNumber;
    
    public void OpenPrompt()
    {
        SaveManagerTest.Instance.UIPrompt.SetActive(true);
        SaveManagerTest.Instance.SaveToBeDeletedID = SaveNumber;
    }
}
