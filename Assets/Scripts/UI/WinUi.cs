using System;
using UnityEngine;
using UnityEngine.UI;


namespace UI
{
    public class WinUi : MonoBehaviour
    {
        [SerializeField] private Image image;


        private void Awake()
        {
            gameObject.SetActive(false);
        }


        public void Show(Color winnerColor)
        {
            gameObject.SetActive(true);
            image.color = winnerColor;
        }
    }
}