using UnityEngine;

namespace Percent111.ProjectNS.UI
{
    // 마우스를 따라다니는 조준점 UI
    public class Crosshair : MonoBehaviour
    {
        [SerializeField] private RectTransform _crosshairImage;
        [SerializeField] private Canvas _canvas;

        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;

            // 마우스 커서 숨기기
            Cursor.visible = false;
        }

        private void Update()
        {
            UpdateCrosshairPosition();
        }

        // 조준점 위치 업데이트
        private void UpdateCrosshairPosition()
        {
            Vector2 mousePos = Input.mousePosition;

            // Canvas 렌더 모드에 따라 처리
            if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                _crosshairImage.position = mousePos;
            }
            else if (_canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvas.transform as RectTransform,
                    mousePos,
                    _canvas.worldCamera,
                    out Vector2 localPoint
                );
                _crosshairImage.localPosition = localPoint;
            }
        }

        private void OnDestroy()
        {
            // 마우스 커서 복원
            Cursor.visible = true;
        }

        private void OnDisable()
        {
            // 비활성화 시 마우스 커서 복원
            Cursor.visible = true;
        }

        private void OnEnable()
        {
            // 활성화 시 마우스 커서 숨기기
            Cursor.visible = false;
        }
    }
}
