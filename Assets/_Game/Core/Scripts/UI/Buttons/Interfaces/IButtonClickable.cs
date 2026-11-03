using System;

namespace Game.UI
{
    public interface IButtonClickable
    {
        event Action OnClicked;
    }
}
