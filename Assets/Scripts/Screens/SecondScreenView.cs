using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using AXitUnityTemplate.UI.Classic.Async;

public class SecondScreenModel : BaseScreenModel
{
    public string Message { get; set; }
}

public class SecondScreenView : BaseScreenView
{
    public TMP_Text messageText;
    public Button buttonBack;
}

public class SecondScreenPresenter : BaseScreenPresenter<SecondScreenView, SecondScreenModel>
{
    public override string ScreenPath => "Screens/SecondScreenView";

    public override void Awake()
    {
        Debug.Log("[SecondScreenPresenter] Open with message: " + this.Model.Message);
    }

    public override void OnEnable()
    {
        this.View.messageText.text = this.Model.Message;
        this.View.buttonBack.onClick.AddListener(this.OnButtonBackClicked);
    }
    private void OnButtonBackClicked()
    {
        this.OnCloseView();
    }

    public override void OnDisable()
    {
        this.View.buttonBack.onClick.RemoveListener(this.OnButtonBackClicked);
    }

    public override void OnDestroy() { }
}
