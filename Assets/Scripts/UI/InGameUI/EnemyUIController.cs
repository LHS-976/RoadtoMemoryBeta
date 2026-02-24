using UnityEngine;
using UnityEngine.UI;

public class EnemyUIController : MonoBehaviour
{
    [Header("Listening Channel")]
    [SerializeField] private EnemyDamageUIEventChannelSO _damageUIChannel;

    [Header("UI References")]
    [SerializeField] private Image _hpImage;
    [SerializeField] private GameObject _damageTextPrefab;

    private Transform _rootTransform;

    private void Awake()
    {
        _rootTransform = GetComponentInParent<EnemyManager>().transform;
    }

    private void OnEnable()
    {
        if (_damageUIChannel != null)
            _damageUIChannel.OnEventRaised += OnDamageReceived;
    }

    private void OnDisable()
    {
        if (_damageUIChannel != null)
            _damageUIChannel.OnEventRaised -= OnDamageReceived;
    }

    private void OnDamageReceived(DamageUIPayLoad payload)
    {
        if (payload.targetEnemy != _rootTransform) return;

        if (_hpImage != null)
        {
            _hpImage.fillAmount = payload.currentHealth / payload.maxHealth;
            if(payload.currentHealth <= 0)
            {
                _hpImage.gameObject.SetActive(false);
            }
        }
        if (_damageTextPrefab != null && VFXManager.Instance != null)
        {
            Vector3 spawnPos = payload.hitPoint + Vector3.up * 0.5f;

            GameObject textObj = VFXManager.Instance.PlayVFX(_damageTextPrefab, spawnPos, transform.rotation);

            if (textObj != null)
            {
                DamageText dmgText = textObj.GetComponent<DamageText>();
                //Setup할 때 '자신이 돌아갈 프리팹 주소'를 같이 넘겨줍니다.
                if (dmgText != null) dmgText.Setup(payload.damageAmount, _damageTextPrefab);
            }
        }
    }
}