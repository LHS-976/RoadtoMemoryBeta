using UnityEngine;
using UnityEngine.UI;

public class BarCellUnit : MonoBehaviour
{
    [SerializeField] private Image _bgImage;
    [SerializeField] private Image _fillImage;

    public void SetFill(float amount)
    {
        if (amount <= 0f)
        {
            _fillImage.enabled = false;
        }
        else
        {
            _fillImage.enabled = true;
            _fillImage.fillAmount = Mathf.Clamp01(amount);
            _fillImage.color = Color.white;
        }
    }
    public void SetGhost(float amount, Color ghostColor)
    {
        if (amount <= 0f)
        {
            _fillImage.enabled = false;
            return;
        }
        _fillImage.enabled = true;
        _fillImage.fillAmount = Mathf.Clamp01(amount);
        _fillImage.color = ghostColor;
    }
    public float GetFill()
    {
        if (!_fillImage.enabled) return 0f;
        return _fillImage.fillAmount;
    }
}