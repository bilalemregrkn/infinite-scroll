using TMPro;
using UnityEngine;

public class SampleCell : BaseCell
{
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;

    public override void UpdateDisplay(int index)
    {
        base.UpdateDisplay(index);
        textMeshProUGUI.SetText($"{index + 1}");
    }
}