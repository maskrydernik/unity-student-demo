using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HatMenuRightClick : MonoBehaviour, IPointerClickHandler
{
    public int hatIndex;
    public Anthony_HatMenu hatMenu;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (hatMenu != null)
            {
                hatMenu.OnHatButtonClicked(hatIndex);
            }
        }
    }
}
