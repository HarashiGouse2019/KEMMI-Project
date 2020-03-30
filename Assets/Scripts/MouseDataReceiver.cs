using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MouseDataReceiver : MonoBehaviour
{
    //Something to put on a gameObject
    GameObject entityObj;
    Vector3 screenPoint;
    Vector3 offset;

    [Header("Draggable")]
    public bool dragable = false;

    void Awake()
    {
        entityObj = gameObject;
       
    }

    void OnMouseDown()
    {
        screenPoint = Camera.main.ViewportToScreenPoint(Input.mousePosition);

        offset = entityObj.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

    }

    void OnMouseDrag()
    {
        if (dragable) {
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);

            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;

            entityObj.transform.position = curPosition;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        entityObj = null;
    }
}
