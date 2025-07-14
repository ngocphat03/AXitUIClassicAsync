using UnityEngine.UI;
using AXitUnityTemplate.UI.Classic.Async;

public class FirstPopupModel : BasePopupModel
{
}

public class FirstPopupView : BasePopupView
{
    public Button buttonClose;
    public Button buttonOpenSecondPopup;
}

public class FirstPopupPresenter : BasePopupPresenter<FirstPopupView, FirstPopupModel>
{
    public override string PopupPath => "Popups/FirstPopupView";
    
    private readonly ScreenManager screenManager = ScreenManager.Instance;

    public override void Awake()
    {
        this.View.buttonOpenSecondPopup.onClick.AddListener(() =>
        {
            _ = this.screenManager.OpenPopup<SecondPopupPresenter, SecondPopupModel>();
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