using AXitUnityTemplate.UI.Classic.Async;
using UnityEngine.UI;

public class SecondPopupModel : BasePopupModel
{
}

public class SecondPopupView : BasePopupView
{
    public Button buttonClose;
    public Button buttonOpenFirstPopup;
}

public class SecondPopupPresenter : BasePopupPresenter<SecondPopupView, SecondPopupModel>
{
    public override string PopupPath => "Popups/SecondPopupView";
    
    private readonly ScreenManager screenManager = ScreenManager.Instance;

    public override void Awake()
    {
        this.View.buttonOpenFirstPopup.onClick.AddListener(() =>
        {
            _ = this.screenManager.OpenPopupAsync<FirstPopupPresenter, FirstPopupModel>();
        });
        
        this.View.buttonClose.onClick.AddListener(() =>
        {
            this.OnCloseView();
        });
    }

    public override void OnEnable() { }

    public override void OnDisable() { }

    public override void OnDestroy() { }
}