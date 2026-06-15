using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private Image _loadingImage;
    [SerializeField] private TextMeshProUGUI _loadingText;
    public void SetPercent(float percent)
    {
        _loadingImage.fillAmount = percent/100;
        _loadingText.text = percent.ToString("F0") + "%";
    }
}
