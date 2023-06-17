using UnityEngine;

namespace Players.ChangeColor
{
    public interface IChangeColor
    {
        Color Change(Color previousColor);
    }
}