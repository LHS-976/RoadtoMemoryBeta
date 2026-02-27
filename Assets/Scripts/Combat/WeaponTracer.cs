using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class WeaponTracer : MonoBehaviour
{

    private IWeaponHitRange _weaponHitRange;

    [Header("Setup")]
    [SerializeField] private Transform[] _tracePoints;
    [SerializeField] private LayerMask _hitLayer;
    public float hitRadius = 0.4f;

    private bool _isTracing = false;
    private Vector3[] _prevPositions;

    private HashSet<int> _hitVictims = new HashSet<int>(); //중복을 허용하지 않는 컬렉션(집합)
    private RaycastHit[] _rayHitBuffer = new RaycastHit[10];


    [Header("Weapon Models View")]
    public GameObject weaponHand;
    public GameObject weaponBack;

    [Header("Effects")]
    public VisualEffect drawVFX;
    public VisualEffect sheathVFX;

    [Header("Combat Unlock")]
    [SerializeField] private VoidEventChannelSO _enableCombatChannel;
    private Vector3 _prevMidPoint;
    private bool _weaponUnlocked = false;
    public void Initialize(IWeaponHitRange weaponHitRange)
    {
        _weaponHitRange = weaponHitRange;

        if (_tracePoints == null || _tracePoints.Length == 0)
        {
            _tracePoints = new Transform[0];
            _prevPositions = new Vector3[0];
            return;
        }
        _prevPositions = new Vector3[_tracePoints.Length];
        for (int i = 0; i < _tracePoints.Length; i++)
        {
            if (_tracePoints[i] != null)
            {
                _prevPositions[i] = _tracePoints[i].position;
            }
            else
            {
                Debug.LogWarning($"[WeaponTracer] TracePoint[{i}]가 null입니다!", this);
            }
        }
    }

    //공격을 시작하면 CombatSystem에서 호출
    public void EnableTrace()
    {
        if (_isTracing) return;

        _isTracing = true;
        _hitVictims.Clear();
        _prevMidPoint = GetSlashPosition();

        for (int i = 0; i < _tracePoints.Length; i++)
        {
            if (_tracePoints[i] != null)
            {
                _prevPositions[i] = _tracePoints[i].position;
            }
        }
    }
    public void DisableTrace()
    {
        if (!_isTracing) return;

        _isTracing = false;
    }
    private void LateUpdate()
    {
        if (!_isTracing || _weaponHitRange == null)
        {
            _prevMidPoint = GetSlashPosition();
            return;
        }

        PerformTrace();
        _prevMidPoint = GetSlashPosition();
    }
    private void PerformTrace()
    {
        if (_tracePoints.Length == 1 && _tracePoints[0] != null)
        {
            Vector3 attackPos = _tracePoints[0].position;

            Collider[] hits = Physics.OverlapSphere(attackPos, hitRadius, _hitLayer);
            foreach (Collider hitCollider in hits)
            {
                if (hitCollider.transform.IsChildOf(transform.root)) continue;

                IDamageable damageable = hitCollider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    int rootID = damageable.GetTransform().root.GetInstanceID();
                    if (_hitVictims.Contains(rootID)) continue;

                    _hitVictims.Add(rootID);

                    if (_weaponHitRange != null)
                    {
                        Vector3 hitDir = (attackPos - transform.position).normalized;
                        hitDir.y = 0;
                        _weaponHitRange.OnWeaponHit(damageable, attackPos, hitDir);
                    }
                }
            }
            return;
        }

        for (int i = 0; i < _tracePoints.Length; i++)
        {
            Vector3 startPos = _prevPositions[i];
            Vector3 endPos = _tracePoints[i].position;

            Debug.DrawLine(startPos, endPos, Color.red, 5.0f);
            Vector3 direction = endPos - startPos;
            float distance = direction.magnitude;

            if (distance <= 0.001f) continue;

            int hitCount = Physics.RaycastNonAlloc(startPos, direction.normalized, _rayHitBuffer, distance, _hitLayer);

            for (int j = 0; j < hitCount; j++)
            {
                Collider hitCollider = _rayHitBuffer[j].collider;

                if(hitCollider.transform.IsChildOf(transform.root))
                {
                    continue;
                }

                IDamageable damageable = hitCollider.GetComponent<IDamageable>();

                if (damageable != null)
                {
                    int rootID = damageable.GetTransform().root.GetInstanceID();

                    if (_hitVictims.Contains(rootID)) continue;
                    _hitVictims.Add(rootID);

                    if(_weaponHitRange != null)
                    {
                        Vector3 hitPoint = _rayHitBuffer[j].point;
                        Vector3 hitDir = direction.normalized;
                        _weaponHitRange.OnWeaponHit(damageable, hitPoint, hitDir);
                    }
                }
            }
            _prevPositions[i] = endPos;
        }
    }
    private void Start()
    {
        GameData data = Core.GameCore.Instance?.DataManager?.CurrentData;
        if (data != null && data.IsCombatUnlocked)
        {
            _weaponUnlocked = true;
            SheathWeapon();  //등에 표시
        }
        else
        {
            HideWeapon();    //완전 숨김
        }
    }
    private void OnEnable()
    {
        if (_enableCombatChannel != null)
            _enableCombatChannel.OnEventRaised += OnWeaponUnlocked;
    }

    private void OnDisable()
    {
        if (_enableCombatChannel != null)
            _enableCombatChannel.OnEventRaised -= OnWeaponUnlocked;
    }
    private void OnWeaponUnlocked()
    {
        _weaponUnlocked = true;
        SheathWeapon();  //등에 칼 표시
    }

    private void HideWeapon()
    {
        if (weaponHand != null) weaponHand.SetActive(false);
        if (weaponBack != null) weaponBack.SetActive(false);
    }

    public void DrawWeapon()
    {
        if (weaponBack != null) weaponBack.SetActive(false);
        if (weaponHand != null) weaponHand.SetActive(true);

        if (drawVFX != null) drawVFX.Play();

    }

    public void SheathWeapon()
    {
        if (weaponHand != null) weaponHand.SetActive(false);
        if (weaponBack != null) weaponBack.SetActive(true);

        if (sheathVFX != null) sheathVFX.Play();

    }
    public Vector3 GetSlashPosition()
    {
        if (_tracePoints.Length == 0) return transform.position;

        Vector3 sum = Vector3.zero;
        for (int i = 0; i < _tracePoints.Length; i++)
        {
            sum += _tracePoints[i].position;
        }
        return sum / _tracePoints.Length;
    }

    public Quaternion GetSlashRotation()
    {
        Vector3 currentMid = GetSlashPosition();
        Vector3 swingDir = currentMid - _prevMidPoint;

        Vector3 bladeDir = (_tracePoints[_tracePoints.Length - 1].position
                           - _tracePoints[0].position).normalized;

        if (swingDir.sqrMagnitude < 0.001f)
        {
            return Quaternion.LookRotation(-transform.forward, bladeDir);
        }

        return Quaternion.LookRotation(-swingDir.normalized, bladeDir);
    }
}
