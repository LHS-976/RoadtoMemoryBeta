using Core;
using System.Collections.Generic;
using UnityEngine;

public class InGameUIManager : MonoBehaviour
{
    [Header("Channels")]
    [SerializeField] private GameStateSO _gameState;

    [Header("Panels")]
    [SerializeField] private PanelFader _hudPanel;

    [Header("Listening Channel")]
    [SerializeField] private PlayerUIEventChannelSO _playerUIChannel;

    [Header("HP Cell Settings")]
    [Tooltip("HP 한 칸 프리팹 (BarCellUnit 컴포넌트 포함)")]
    [SerializeField] private GameObject _hpCellPrefab;
    [Tooltip("HP 칸들이 배치될 부모 (HorizontalLayoutGroup 권장)")]
    [SerializeField] private RectTransform _hpCellContainer;

    [Header("Stamina Cell Settings")]
    [SerializeField] private GameObject _staminaCellPrefab;
    [SerializeField] private RectTransform _staminaCellContainer;

    [Header("Animation Settings")]
    [Tooltip("fillAmount가 변하는 속도")]
    [SerializeField] private float _fillLerpSpeed = 8f;

    [Header("Ghost Bar Settings")]
    [Tooltip("데미지 잔상 색상")]
    [SerializeField] private Color _ghostColor = new Color(1f, 0.3f, 0.3f, 0.8f);
    [Tooltip("잔상이 따라오기 전 대기 시간(초)")]
    [SerializeField] private float _ghostDelay = 0.4f;
    [Tooltip("잔상이 따라오는 속도")]
    [SerializeField] private float _ghostLerpSpeed = 4f;

    private const float HP_PER_CELL = 10f;
    private const float STAMINA_PER_CELL = 10f;

    private List<BarCellUnit> _hpCells = new List<BarCellUnit>();
    private List<BarCellUnit> _staminaCells = new List<BarCellUnit>();

    // 실제 목표값 (소수점)
    private float _targetHp;
    private float _targetStamina;

    // 현재 표시 중인 값 (Lerp로 부드럽게 이동)
    private float _displayHp;
    private float _displayStamina;

    // 고스트 바 값
    private float _ghostHp;
    private float _ghostStamina;
    private float _hpGhostTimer;
    private float _staminaGhostTimer;

    // 최대값 (칸 수 계산용)
    private float _maxHp;
    private float _maxStamina;

    private bool _initialized = false;

    private void Awake()
    {
        if (_gameState != null)
        {
            HandleStateChange(_gameState.CurrentState);
        }
        else
        {
            Debug.LogWarning("_gameState가 비어있습니다.");
        }
    }

    private void OnEnable()
    {
        if (_gameState != null)
            _gameState.OnStateChange += HandleStateChange;
        if (_playerUIChannel != null)
            _playerUIChannel.OnEventRaised += OnPlayerUIUpdated;
    }

    private void OnDisable()
    {
        if (_gameState != null)
            _gameState.OnStateChange -= HandleStateChange;
        if (_playerUIChannel != null)
            _playerUIChannel.OnEventRaised -= OnPlayerUIUpdated;
    }

    private void HandleStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.Gameplay:
                _hudPanel.FadeIn();
                break;
            case GameState.Dialogue:
            case GameState.StatShop:
            case GameState.Option:
            case GameState.PlayerInfo:
                _hudPanel.FadeOut();
                break;
        }
    }

    private void OnPlayerUIUpdated(PlayerUIPayload payload)
    {
        // ── HP ──
        if (payload.maxHp > 0)
        {
            _maxHp = payload.maxHp;
            float newHp = payload.currentHp;

            int totalCells = Mathf.CeilToInt(_maxHp / HP_PER_CELL);
            EnsureCellCount(_hpCells, _hpCellContainer, _hpCellPrefab, totalCells);

            // 첫 호출 시 즉시 동기화
            if (!_initialized)
            {
                _displayHp = newHp;
                _ghostHp = newHp;
            }

            // HP가 줄었을 때 고스트 딜레이 시작
            if (newHp < _targetHp)
            {
                _hpGhostTimer = _ghostDelay;
            }
            // HP가 회복됐을 때 고스트 즉시 따라감
            else if (newHp > _targetHp)
            {
                _ghostHp = newHp;
                _hpGhostTimer = 0f;
            }

            _targetHp = newHp;
        }

        // ── Stamina ──
        if (payload.maxStamina > 0)
        {
            _maxStamina = payload.maxStamina;
            float newStamina = payload.currentStamina;

            int totalCells = Mathf.CeilToInt(_maxStamina / STAMINA_PER_CELL);
            EnsureCellCount(_staminaCells, _staminaCellContainer, _staminaCellPrefab, totalCells);

            if (!_initialized)
            {
                _displayStamina = newStamina;
                _ghostStamina = newStamina;
            }

            if (newStamina < _targetStamina)
            {
                _staminaGhostTimer = _ghostDelay;
            }
            else if (newStamina > _targetStamina)
            {
                _ghostStamina = newStamina;
                _staminaGhostTimer = 0f;
            }

            _targetStamina = newStamina;
        }

        _initialized = true;
    }

    private void Update()
    {
        if (!_initialized) return;

        // 표시 HP를 목표값으로 부드럽게 이동
        _displayHp = Mathf.Lerp(_displayHp, _targetHp, Time.deltaTime * _fillLerpSpeed);
        _displayStamina = Mathf.Lerp(_displayStamina, _targetStamina, Time.deltaTime * _fillLerpSpeed);

        // 고스트 HP 처리
        UpdateGhostValue(ref _ghostHp, _targetHp, ref _hpGhostTimer);
        UpdateGhostValue(ref _ghostStamina, _targetStamina, ref _staminaGhostTimer);

        // 칸에 반영
        ApplyCells(_hpCells, _displayHp, _ghostHp, HP_PER_CELL);
        ApplyCells(_staminaCells, _displayStamina, _ghostStamina, STAMINA_PER_CELL);
    }

    #region Ghost Value

    private void UpdateGhostValue(ref float ghostValue, float targetValue, ref float ghostTimer)
    {
        // 회복 시 즉시 따라감
        if (targetValue >= ghostValue)
        {
            ghostValue = targetValue;
            ghostTimer = 0f;
            return;
        }

        // 딜레이 중
        if (ghostTimer > 0f)
        {
            ghostTimer -= Time.deltaTime;
            return;
        }

        // 딜레이 끝나면 부드럽게 따라옴
        ghostValue = Mathf.Lerp(ghostValue, targetValue, Time.deltaTime * _ghostLerpSpeed);

        // 거의 도달하면 스냅
        if (Mathf.Abs(ghostValue - targetValue) < 0.1f)
        {
            ghostValue = targetValue;
        }
    }

    #endregion

    #region Cell Management

    private void EnsureCellCount(List<BarCellUnit> cells, RectTransform container,
                                  GameObject prefab, int targetCount)
    {
        if (container == null || prefab == null) return;

        // 이미 맞으면 아무것도 안 함
        if (cells.Count == targetCount) return;

        while (cells.Count < targetCount)
        {
            GameObject cellObj = Instantiate(prefab, container);
            BarCellUnit unit = cellObj.GetComponent<BarCellUnit>();
            if (unit != null)
            {
                cells.Add(unit);
            }
            else
            {
                // BarCellUnit이 없으면 무한루프 방지 — 생성한 오브젝트 제거 후 중단
                Debug.LogError("[InGameUIManager] 프리팹에 BarCellUnit 컴포넌트가 없습니다!", prefab);
                Destroy(cellObj);
                break;
            }
        }

        while (cells.Count > targetCount)
        {
            int lastIndex = cells.Count - 1;
            if (cells[lastIndex] != null)
            {
                Destroy(cells[lastIndex].gameObject);
            }
            cells.RemoveAt(lastIndex);
        }
    }

    /// <summary>
    /// 각 칸의 fillAmount를 소수점 HP 기반으로 계산
    /// </summary>
    private void ApplyCells(List<BarCellUnit> cells, float displayValue, float ghostValue, float perCell)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i] == null) continue;

            float cellMin = i * perCell;        // 이 칸의 시작 HP
            float cellMax = (i + 1) * perCell;  // 이 칸의 끝 HP

            // 실제 표시값 기준 fillAmount
            float displayFill = Mathf.Clamp01((displayValue - cellMin) / perCell);

            // 고스트값 기준 fillAmount
            float ghostFill = Mathf.Clamp01((ghostValue - cellMin) / perCell);

            if (ghostFill > displayFill && ghostFill > 0f)
            {
                // 고스트가 더 크면: 고스트 잔상 표시
                cells[i].SetGhost(ghostFill, _ghostColor);
            }

            // 실제 값 위에 덮어쓰기 (고스트 위에 실제 바가 보이도록)
            // 고스트와 실제 바를 동시에 보여주려면 Cell_Fill이 두 개 필요하므로
            // 단일 Fill 구조에서는 고스트 → 실제 순서로 표시
            if (displayFill > 0f)
            {
                cells[i].SetFill(displayFill);
            }
            else if (ghostFill <= 0f)
            {
                cells[i].SetFill(0f);
            }
        }
    }

    #endregion
}