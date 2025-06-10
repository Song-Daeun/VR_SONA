using UnityEngine;
using UnityEngine.UI;

public class MissionReturnButton : MonoBehaviour
{
    // ================================ //
    // 돌아가기 버튼 처리
    // ================================ //
    
    private Button returnButton;

    void Start()
    {
        // 자기 자신이 Button 컴포넌트를 가지고 있다고 가정
        returnButton = GetComponent<Button>();
        
        if (returnButton != null)
        {
            returnButton.onClick.AddListener(OnReturnButtonClicked);
            Debug.Log("🔘 돌아가기 버튼 리스너 등록됨");
            Debug.Log($"🔘 버튼 상태 - Interactable: {returnButton.interactable}, Active: {returnButton.gameObject.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("❌ Button 컴포넌트를 찾을 수 없습니다!");
        }

        // EventSystem 확인
        UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogError("❌ EventSystem을 찾을 수 없습니다! UI 클릭이 작동하지 않을 수 있습니다.");
        }
        else
        {
            Debug.Log("✅ EventSystem 발견됨");
        }
    }

    private void OnReturnButtonClicked()
    {
        Debug.Log("🔙 돌아가기 버튼 클릭됨");
        
        // Time.timeScale 정상화 (Basketball 미션에서 0으로 설정되었을 수 있음)
        Time.timeScale = 1f;
        
        // MissionManager를 통해 결과 수집 및 메인씬 복귀
        if (MissionManager.Instance != null)
        {
            Debug.Log("✅ MissionManager.Instance 발견, ReturnFromMission 호출");
            MissionManager.Instance.ReturnFromMission();
        }
        else
        {
            Debug.LogError("❌ MissionManager.Instance를 찾을 수 없습니다!");
            
            // MissionManager 직접 찾기 시도
            MissionManager missionManager = FindObjectOfType<MissionManager>();
            if (missionManager != null)
            {
                Debug.Log("✅ MissionManager를 직접 찾았습니다. ReturnFromMission 호출");
                missionManager.ReturnFromMission();
            }
            else
            {
                Debug.LogError("❌ MissionManager를 전혀 찾을 수 없습니다!");
            }
        }
    }

    // ================================ //
    // 외부에서 호출 가능한 메서드
    // ================================ //
    public void ReturnToMainScene()
    {
        OnReturnButtonClicked();
    }
}