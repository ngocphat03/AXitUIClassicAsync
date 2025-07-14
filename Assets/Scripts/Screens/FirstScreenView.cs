using UnityEngine.UI;
using AXitUnityTemplate.UI.Classic.Async;

public class FirstScreenModel : BaseScreenModel
{
}

[ViewInitInScene(typeof(FirstScreenPresenter))]
public class FirstScreenView : BaseScreenView
{
    public Button buttonOpenSecondScreen;
    public Button buttonOpenFirstPopup;
    public Button buttonOpenSecondPopup;
}

public class FirstScreenPresenter : BaseScreenPresenter<FirstScreenView, FirstScreenModel>
{
    private ScreenManager screenManager = ScreenManager.Instance;

    public override string ScreenPath => "Screens/FirstScreenView";

    public override void Awake() { }

    public override void OnEnable()
    {
        this.View.buttonOpenSecondScreen.onClick.AddListener(this.OnButtonOpenSecondScreenClicked);
        this.View.buttonOpenFirstPopup.onClick.AddListener(this.OnButtonOpenFirstPopupClicked);
        this.View.buttonOpenSecondPopup.onClick.AddListener(this.OnButtonOpenSecondPopupClicked);
    }

    private async void OnButtonOpenSecondScreenClicked()
    {
        await this.screenManager.OpenScreen<SecondScreenPresenter, SecondScreenModel>(new SecondScreenModel()
            {
                Message = "Opened from First Screen"
            });
    }

    private async void OnButtonOpenFirstPopupClicked()
    {
        await this.screenManager.OpenPopup<FirstPopupPresenter, FirstPopupModel>();
    }

    private async void OnButtonOpenSecondPopupClicked()
    {
        await this.screenManager.OpenPopup<SecondPopupPresenter, SecondPopupModel>();
    }

    public override void OnDisable()
    {
        this.View.buttonOpenSecondScreen.onClick.RemoveListener(this.OnButtonOpenSecondScreenClicked);
        this.View.buttonOpenFirstPopup.onClick.RemoveListener(this.OnButtonOpenFirstPopupClicked);
        this.View.buttonOpenSecondPopup.onClick.RemoveListener(this.OnButtonOpenSecondPopupClicked);
    }

    public override void OnDestroy() { }
}
