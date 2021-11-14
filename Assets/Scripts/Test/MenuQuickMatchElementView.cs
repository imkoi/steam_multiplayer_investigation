using UnityEngine;
using UnityEngine.EventSystems;

namespace Test
{
    public class MenuQuickMatchElementView : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private MenuQuickMatchType _gameMode;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("HI");
        }
    }
}