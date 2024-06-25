using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ButtonHandler : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI buttonText;
    public Image buttonImage;
    public AudioSource audioSource;
    private GameManagerPvC gameControllerPvC;
    private GameManagerPvP gameControllerPvP;
    private int lifetime;

    public void SetGameControllerReference(GameManagerPvC controller)
    {
        gameControllerPvC = controller;
    }

    public void SetGameControllerReference(GameManagerPvP controller)
    {
        gameControllerPvP = controller;
    }

    public void SetSpace()
    {
        if (buttonText != null && buttonText.text == "" && ((gameControllerPvC != null && gameControllerPvC.IsPlayerTurn()) || (gameControllerPvP != null && gameControllerPvP.IsPlayerTurn())))
        {
            StartCoroutine(AnimateButtonPress());
            if (gameControllerPvC != null)
            {
                SetSpaceForPvC();
            }
            else if (gameControllerPvP != null)
            {
                SetSpaceForPvP();
            }
        }
    }

    private void SetSpaceForPvC()
    {
        buttonText.text = gameControllerPvC.GetPlayerSide();
        button.interactable = false;
        buttonImage.color = Color.red;
        lifetime = 7;
        gameControllerPvC.EndTurn();
    }

    private void SetSpaceForPvP()
    {
        string playerSide = gameControllerPvP.GetPlayerSide();
        buttonText.text = playerSide;
        button.interactable = false;
        buttonImage.color = (playerSide == "X") ? Color.red : Color.blue;
        lifetime = 7;
        gameControllerPvP.EndTurn();
    }

    public void SetSpaceForComputer(string side, Color color)
    {
        if (buttonText != null && buttonText.text == "")
        {
            StartCoroutine(AnimateButtonPress());
            buttonText.text = side;
            button.interactable = false;
            buttonImage.color = color;
            lifetime = 7;
        }
    }

    public void DecreaseLifetime()
    {
        if (lifetime > 0)
        {
            lifetime--;
            buttonText.text = lifetime.ToString();
            if (lifetime == 0)
            {
                ClearButton();
            }
        }
    }

    private void ClearButton()
    {
        buttonText.text = "";
        buttonImage.color = Color.white;
        button.interactable = true;
    }

    public Color GetButtonColor()
    {
        return buttonImage.color;
    }

    public int GetLifetime()
    {
        return lifetime;
    }

    public void ResetButton()
    {
        buttonText.text = "";
        buttonImage.color = Color.white;
        button.interactable = true;
        lifetime = 0;
    }

    private void PlayButtonSound()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    private IEnumerator AnimateButtonPress()
    {
        Vector3 originalScale = button.transform.localScale;
        Vector3 pressedScale = originalScale * 0.9f;
        float duration = 0.1f;

        // Butonun küçülme animasyonu
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            button.transform.localScale = Vector3.Lerp(originalScale, pressedScale, elapsedTime / duration);
            yield return null;
        }

        PlayButtonSound();


        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            button.transform.localScale = Vector3.Lerp(pressedScale, originalScale, elapsedTime / duration);
            yield return null;
        }
    }
}
