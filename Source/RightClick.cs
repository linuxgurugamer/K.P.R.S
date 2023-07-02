using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Smooth.Delegates;
using System.Runtime.CompilerServices;
using UnityEngineInternal;

namespace KPRS
{

    public class RightClick :  MonoBehaviour, IPointerClickHandler
    {

        public UnityEvent leftClick;
        public UnityEvent middleClick;
        public UnityEvent rightClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                leftClick.Invoke();
            else if (eventData.button == PointerEventData.InputButton.Middle)
                middleClick.Invoke();
            else if (eventData.button == PointerEventData.InputButton.Right)
                rightClick.Invoke();
        }
    }
}
