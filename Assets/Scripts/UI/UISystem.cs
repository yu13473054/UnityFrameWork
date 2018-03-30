public enum UIState
{
    NORMAL,
    DONTDESTROY,
    DESTROYONCLOSE,
}

public class UISystem : UIMod
{
    public UILayer layer = UILayer.POP;
    public UIState uiState = UIState.NORMAL;

}
