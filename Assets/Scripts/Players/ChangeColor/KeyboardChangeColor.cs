using UnityEngine;

namespace Players.ChangeColor
{
    public class KeyboardChangeColor : IChangeColor
    {
        public Color Change(Color previousColor)
        {
            return Input.GetKeyDown(KeyCode.C) 
                ? Random.ColorHSV() 
                : previousColor;
        }
    }
}