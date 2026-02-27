using UnityEngine;
using TMPro;
using Core;

/// <summary>
/// 플레이어 정보창 UI.
/// I키로 열기/닫기 토글
/// 현재 HP / 최대 HP
/// 현재 스태미나 / 최대 스태미나
/// 공격력 (Base + 업그레이드 보너스)
/// 콤보 공격력 배수 (추가됨)
/// 보유 DataChips
/// </summary>
public class PlayerInfoUIManager : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private PanelFader _infoPanelFader;

    [Header("UI Texts")]
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _staminaText;
    [SerializeField] private TextMeshProUGUI _attackText;
    [SerializeField] private TextMeshProUGUI _dataChipsText;

    [Tooltip("콤보 공격력 배수를 표시할 텍스트")]
    [SerializeField] private TextMeshProUGUI _comboMultipliersText;

    [Header("Settings")]
    [SerializeField] private KeyCode _toggleKey = KeyCode.I;

    [Header("GameState")]
    [SerializeField] private GameStateSO _gameState;

    [SerializeField] private AudioClip _openInfoSound;
    private bool _isOpen = false;

    private void Start()
    {
        _infoPanelFader?.SetImmediateClosed();
    }

    private void Update()
    {
        if (Input.GetKeyDown(_toggleKey))
        {
            //타이틀, 대화 중에는 무시
            if (_gameState != null &&
               (_gameState.CurrentState == GameState.Title ||
                _gameState.CurrentState == GameState.Dialogue ||
                _gameState.CurrentState == GameState.StatShop ||
                _gameState.CurrentState == GameState.Option))
            {
                return;
            }

            if (_isOpen)
                CloseInfo();
            else
                OpenInfo();
        }
    }

    private void OpenInfo()
    {
        if (_openInfoSound != null && SoundManager.Instance != null)
            SoundManager.Instance.PlayUI(_openInfoSound);
        _isOpen = true;
        RefreshInfo();
        _infoPanelFader?.FadeIn();

        //게임 일시정지
        if (_gameState != null)
            _gameState.SetState(GameState.PlayerInfo);

        if (GameCore.Instance?.TimeManager != null)
            GameCore.Instance.TimeManager.PauseTime();
    }

    private void CloseInfo()
    {
        _isOpen = false;
        _infoPanelFader?.FadeOut();

        //게임 재개
        if (GameCore.Instance != null)
            GameCore.Instance.ResumeGame();
    }

    private void RefreshInfo()
    {
        if (GameCore.Instance == null || GameCore.Instance.CurrentPlayer == null) return;

        PlayerManager pm = GameCore.Instance.CurrentPlayer.GetComponent<PlayerManager>();
        if (pm == null) return;

        DataManager dataManager = GameCore.Instance.DataManager;
        GameData data = dataManager?.CurrentData;

        //HP
        if (_hpText != null)
        {
            float maxHp = pm.MaxHp;
            float currentHp = pm.CurrentHp;
            _hpText.text = $"{currentHp:F0} / {maxHp:F0}";
        }

        //스태미나
        if (_staminaText != null)
        {
            float maxStamina = pm.MaxStamina;
            float currentStamina = pm.CurrentStamina;
            _staminaText.text = $"{currentStamina:F0} / {maxStamina:F0}";
        }

        PlayerCombatSystem combatSystem = GameCore.Instance.CurrentPlayer.GetComponent<PlayerCombatSystem>();

        //공격력
        if (_attackText != null)
        {
            float baseDamage = combatSystem != null && combatSystem.currentStrategy != null
                             ? combatSystem.currentStrategy.baseDamage
                             : 0f;

            float bonus = data != null ? data.GetAttackBonus() : 0f;

            if (bonus > 0)
                _attackText.text = $"{baseDamage:F0} + {bonus:F0}";
            else
                _attackText.text = $"{baseDamage:F0}";
        }

        //콤보 공격력 배수 표시
        if (_comboMultipliersText != null && combatSystem != null && combatSystem.currentStrategy != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (var action in combatSystem.currentStrategy.actions)
            {
                //해금 조건이 있는 공격이면 데이터에서 해금 여부를 확인
                bool isUnlocked = !action.requiresUnlock || (data != null && data.IsAttackUnlocked(action.attackName));

                string displayName = action.attackName.Replace("Attack_", "");

                if (isUnlocked)
                {
                    sb.AppendLine($"- {displayName} : x{action.damageMultiplier:F1}");
                }
                else
                {
                    sb.AppendLine($"- <color=#808080>{displayName} : (미해금)</color>");
                }
            }

            _comboMultipliersText.text = sb.ToString();
        }

        //DataChips
        if (_dataChipsText != null)
        {
            int chips = data != null ? data.DataChips : 0;
            _dataChipsText.text = $"{chips}";
        }
    }
}