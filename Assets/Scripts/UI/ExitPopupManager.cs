using Doozy.Engine.UI;
using UnityEngine;

public class ExitPopupManager : MonoBehaviour
{
    public static ExitPopupManager instance;

    private const string popupName = "ExitCheckPopup";

    UIPopup popup;
    private void Awake()
    {
        if (ExitPopupManager.instance == null)
        {
            ExitPopupManager.instance = this;
        }

        else
        {
            Destroy(this);
            return;
        }

        GameObject.DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (popup == null)
            {
                ShowPopup();
            }

            else
            {
                return;
            }
        }
    }

    private void ShowPopup()
    {
        popup = UIPopup.GetPopup(popupName);

        if (popup == null) { return; }

        popup.Show();

        popup.Data.SetButtonsCallbacks(OnYesClicked, OnNoClicked);

    }

    private void OnYesClicked()
    {
        Application.Quit();
    }

    private void OnNoClicked()
    {
        popup.Hide();
    }

    public void ToggleTimeScale() // being called by the UIpopup component
    {
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
        }

        else
        {
            Time.timeScale = 0;
        }
    }
}
