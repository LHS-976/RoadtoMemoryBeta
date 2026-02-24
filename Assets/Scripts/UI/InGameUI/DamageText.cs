using System.Collections;
using UnityEngine;
using TMPro; //TextMeshPro를 사용하기 위한 네임스페이스

public class DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMesh;
    [SerializeField] private float _moveSpeed = 2.0f; //위로 올라가는 속도
    [SerializeField] private float _lifeTime = 0.8f;  //화면에 머무는 시간


    private GameObject _originalPrefab;

    public void Setup(float damageAmount, GameObject prefab)
    {
        _textMesh.text = damageAmount.ToString("F0");

        Color color = _textMesh.color;
        color.a = 1f;
        _textMesh.color = color;

        StartCoroutine(FloatingRoutine());
    }

    private IEnumerator FloatingRoutine()
    {
        float timer = 0f;
        Color startColor = _textMesh.color;

        while (timer < _lifeTime)
        {
            transform.position += Vector3.up * _moveSpeed * Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, timer / _lifeTime);
            _textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            timer += Time.deltaTime;
            yield return null; 
        }
        if (VFXManager.Instance != null && _originalPrefab != null)
        {
            VFXManager.Instance.ReturnToPool(_originalPrefab, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}