using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManagerPvC : MonoBehaviour
{
    public TextMeshProUGUI[] buttonList;
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI computerScoreText;
    public TextMeshProUGUI playerText;
    public TextMeshProUGUI computerText;
    public TextMeshProUGUI drawText;
    public LineRenderer lineRenderer;
    public ParticleSystem particlePrefab;
    public GameObject redWinPanel;
    public GameObject blueWinPanel;
    public Button redWinBackToMainMenuButton;
    public Button redWinPlayAgainButton;
    public Button blueWinBackToMainMenuButton;
    public Button blueWinPlayAgainButton;
    public AudioSource drawLineAudioSource;
    private string startingPlayerSide;
    private int moveCount;
    private Color playerColor = Color.red;
    private string computerSide = "O";
    private Color computerColor = Color.blue;
    private bool isPlayerTurn;
    private int playerScore;
    private int computerScore;
    private ParticleSystem currentParticle;

    void Awake()
    {
        SetGameControllerReferenceOnButtons();
        startingPlayerSide = "X";
        isPlayerTurn = startingPlayerSide == "X";
        moveCount = 0;
        playerScore = 0;
        computerScore = 0;
        ClearButtonTexts();
        UpdateScoreTexts();
        UpdateTextColors();

        lineRenderer.startWidth = 0.5f; // Çizginin başlangıç kalınlığı
        lineRenderer.endWidth = 0.5f; // Çizginin bitiş kalınlığı
        lineRenderer.enabled = false;


        drawText.gameObject.SetActive(false);


        redWinPanel.SetActive(false);
        blueWinPanel.SetActive(false);

        redWinBackToMainMenuButton.onClick.AddListener(BackToMainMenu);
        redWinPlayAgainButton.onClick.AddListener(PlayAgain);
        blueWinBackToMainMenuButton.onClick.AddListener(BackToMainMenu);
        blueWinPlayAgainButton.onClick.AddListener(PlayAgain);

        // Eğer bilgisayar ilk başlayan ise, bilgisayar hamlesini yapar
        if (!isPlayerTurn)
        {
            Invoke("ComputerMove", 2f);
        }
    }

    void SetGameControllerReferenceOnButtons()
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<ButtonHandler>().SetGameControllerReference(this);
        }
    }

    void ClearButtonTexts()
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].text = "";
            buttonList[i].GetComponentInParent<ButtonHandler>().ResetButton();
        }
    }

    public string GetPlayerSide()
    {
        return "X"; // Oyuncu her zaman "X"
    }

    public bool IsPlayerTurn()
    {
        return isPlayerTurn;
    }

    public void EndTurn()
    {
        moveCount++;
        Debug.Log("Move count: " + moveCount);
        DecreaseLifetimes();
        if (CheckWin())
        {
            GameOver();
        }
        else if (moveCount >= 30)
        {
            GameDraw();
        }
        else
        {
            if (isPlayerTurn)
            {
                isPlayerTurn = false;
                UpdateTextColors();
                Invoke("ComputerMove", 1.5f);
            }
            else
            {
                isPlayerTurn = true;
                UpdateTextColors();
            }
        }
    }

    void DecreaseLifetimes()
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<ButtonHandler>().DecreaseLifetime();
        }
    }

    void GameDraw()
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<Button>().interactable = false;
        }

        StartCoroutine(AnimateDrawText());
        StartCoroutine(AnimateDrawCentralText());

        Debug.Log("Game Draw!");
        Invoke("ResetGame", 2f);
    }

    IEnumerator AnimateDrawText()
    {
        float duration = 2f;
        float elapsed = 0f;
        Vector3 originalScaleX = playerText.transform.localScale;
        Vector3 originalScaleO = computerText.transform.localScale;
        Vector3 targetScale = originalScaleX * 2f;

        Vector3 fallDistance = new Vector3(0, -Screen.height / 2, 0);

        Vector3 originalPositionX = playerText.transform.localPosition;
        Vector3 originalPositionO = computerText.transform.localPosition;

        // Metinlerin büyüme ve düşme animasyonu
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Ölçekleme
            playerText.transform.localScale = Vector3.Lerp(originalScaleX, targetScale, t);
            computerText.transform.localScale = Vector3.Lerp(originalScaleO, targetScale, t);

            // Düşme
            playerText.transform.localPosition = Vector3.Lerp(originalPositionX, originalPositionX + fallDistance, t);
            computerText.transform.localPosition = Vector3.Lerp(originalPositionO, originalPositionO + fallDistance, t);

            yield return null;
        }

        // Metinlerin yukarıya geri dönme animasyonu
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Ölçekleme
            playerText.transform.localScale = Vector3.Lerp(targetScale, originalScaleX, t);
            computerText.transform.localScale = Vector3.Lerp(targetScale, originalScaleO, t);

            // Geri dönme
            playerText.transform.localPosition = Vector3.Lerp(originalPositionX + fallDistance, originalPositionX, t);
            computerText.transform.localPosition = Vector3.Lerp(originalPositionO + fallDistance, originalPositionO, t);

            yield return null;
        }

        // Animasyon bitince metinleri orijinal boyut ve pozisyona getir
        playerText.transform.localScale = originalScaleX;
        playerText.transform.localPosition = originalPositionX;
        computerText.transform.localScale = originalScaleO;
        computerText.transform.localPosition = originalPositionO;
    }

    IEnumerator AnimateDrawCentralText()
    {
        float duration = 2f; // Animasyon süresi
        float elapsed = 0f;
        Vector3 originalScale = drawText.transform.localScale;
        Vector3 targetScale = originalScale * 4f;


        drawText.gameObject.SetActive(true);
        drawText.color = new Color(1, 1, 0, 1); // Sarı renk
        drawText.text = "DRAW!";


        ParticleSystem drawParticle = Instantiate(particlePrefab, drawText.transform.position, Quaternion.identity);
        var mainModule = drawParticle.main;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(Color.yellow);


        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);


            drawText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);

            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);


            drawText.color = new Color(drawText.color.r, drawText.color.g, drawText.color.b, 1 - t);
            drawText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);

            yield return null;
        }

        drawText.gameObject.SetActive(false);

        drawParticle.Stop();
        yield return new WaitForSeconds(drawParticle.main.duration);
        Destroy(drawParticle.gameObject);
    }

    void ChangeSides()
    {
        isPlayerTurn = !isPlayerTurn;
        UpdateTextColors();
    }

    bool CheckWin()
    {
        return CheckLine(0, 1, 2) || CheckLine(3, 4, 5) || CheckLine(6, 7, 8) ||
               CheckLine(0, 3, 6) || CheckLine(1, 4, 7) || CheckLine(2, 5, 8) ||
               CheckLine(0, 4, 8) || CheckLine(2, 4, 6);
    }

    bool CheckLine(int index1, int index2, int index3)
    {
        Color color1 = buttonList[index1].GetComponentInParent<ButtonHandler>().GetButtonColor();
        Color color2 = buttonList[index2].GetComponentInParent<ButtonHandler>().GetButtonColor();
        Color color3 = buttonList[index3].GetComponentInParent<ButtonHandler>().GetButtonColor();

        if ((color1 == playerColor && color2 == playerColor && color3 == playerColor) ||
            (color1 == computerColor && color2 == computerColor && color3 == computerColor))
        {
            Color lineColor = (color1 == playerColor) ? playerColor : computerColor;
            Vector3 startPos = buttonList[index1].transform.position;
            Vector3 endPos = buttonList[index3].transform.position;


            Vector3 direction = (endPos - startPos).normalized;
            startPos -= direction * 1.2f;
            endPos += direction * 1.2f;

            StartCoroutine(AnimateLine(startPos, endPos, lineColor));
            return true;
        }
        return false;
    }
    IEnumerator AnimateLine(Vector3 start, Vector3 end, Color lineColor)
    {
        float duration = 1.8f;
        float shrinkDuration = 1.0f;
        float elapsed = 0f;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, start);
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.enabled = true;


        drawLineAudioSource.Play();

        currentParticle = Instantiate(particlePrefab, start, Quaternion.identity);
        var mainModule = currentParticle.main;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(lineColor);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 currentPos = Vector3.Lerp(start, end, t);
            lineRenderer.SetPosition(1, currentPos);
            currentParticle.transform.position = currentPos;
            yield return null;
        }


        drawLineAudioSource.Stop();

        lineRenderer.SetPosition(1, end);
        currentParticle.Stop();


        elapsed = 0f;
        Vector3 originalStart = lineRenderer.GetPosition(0);
        Vector3 originalEnd = lineRenderer.GetPosition(1);

        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / shrinkDuration);
            Vector3 currentStart = Vector3.Lerp(originalStart, (originalStart + originalEnd) / 2, t);
            Vector3 currentEnd = Vector3.Lerp(originalEnd, (originalStart + originalEnd) / 2, t);
            lineRenderer.SetPosition(0, currentStart);
            lineRenderer.SetPosition(1, currentEnd);
            yield return null;
        }

        lineRenderer.enabled = false;

        Destroy(currentParticle.gameObject, currentParticle.main.duration);
    }
    void GameOver()
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<Button>().interactable = false;
        }

        if (isPlayerTurn)
        {
            playerScore++;
            StartCoroutine(AnimateWinnerText(playerText, playerColor));
        }
        else
        {
            computerScore++;
            StartCoroutine(AnimateWinnerText(computerText, computerColor));
        }

        UpdateScoreTexts();
        Debug.Log("Game Over! " + (isPlayerTurn ? "Player" : "Computer") + " wins!");

        if (playerScore >= 3)
        {
            ShowWinPanel("X");
        }
        else if (computerScore >= 3)
        {
            ShowWinPanel("O");
        }
        else
        {
            Invoke("ResetGame", 2f); // 2 saniye sonra oyunu resetler
        }
    }

    void ShowWinPanel(string winningPlayer)
    {
        if (winningPlayer == "X")
        {
            redWinPanel.SetActive(true);
        }
        else
        {
            blueWinPanel.SetActive(true);
        }
    }

    public void BackToMainMenu()
    {

        SceneManager.LoadScene(0);
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(1);
    }

    IEnumerator AnimateWinnerText(TextMeshProUGUI winnerText, Color particleColor)
    {
        float duration = 2f;
        float elapsed = 0f;
        Vector3 originalScale = winnerText.transform.localScale;
        Vector3 targetScale = originalScale * 4f;


        float rotationSpeed = 180f;


        ParticleSystem winnerParticle = Instantiate(particlePrefab, winnerText.transform.position, Quaternion.identity);
        var mainModule = winnerParticle.main;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(particleColor);


        float particleLifetime = winnerParticle.main.duration + winnerParticle.main.startLifetime.constantMax;


        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);


            winnerText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);


            winnerText.transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);


            winnerParticle.transform.position = winnerText.transform.position;

            yield return null;
        }


        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);


            winnerText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);


            winnerText.transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);


            winnerParticle.transform.position = winnerText.transform.position;

            yield return null;
        }


        winnerText.transform.localScale = originalScale;
        winnerText.transform.rotation = Quaternion.identity;


        winnerParticle.Stop();
        yield return new WaitForSeconds(particleLifetime);
        Destroy(winnerParticle.gameObject);
    }

    void ResetGame()
    {
        ClearButtonTexts();
        lineRenderer.enabled = false;
        SwitchStartingPlayer();
        isPlayerTurn = startingPlayerSide == "X";
        UpdateTextColors();
        moveCount = 0;

        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<Button>().interactable = true;
        }


        if (!isPlayerTurn)
        {
            Invoke("ComputerMove", 1f);
        }
    }

    void SwitchStartingPlayer()
    {
        if (startingPlayerSide == "X")
        {
            startingPlayerSide = "O";
        }
        else
        {
            startingPlayerSide = "X";
        }
    }

    void UpdateScoreTexts()
    {
        playerScoreText.text = playerScore.ToString();
        computerScoreText.text = computerScore.ToString();
    }

    void UpdateTextColors()
    {
        playerText.color = new Color(1, 0, 0, 0.5f); // Yarı opak kırmızı
        computerText.color = new Color(0, 0, 1, 0.5f); // Yarı opak mavi
        playerText.fontSize = 144; // Küçük boyut
        computerText.fontSize = 144; // Küçük boyut

        if (isPlayerTurn)
        {
            playerText.color = new Color(1, 0, 0, 1); // Tam opak kırmızı
            StartCoroutine(AnimateTextSize(playerText, 144, 288)); // Büyüt
            StartCoroutine(AnimateTextSize(computerText, 288, 144)); // Küçült
        }
        else
        {
            computerText.color = new Color(0, 0, 1, 1); // Tam opak mavi
            StartCoroutine(AnimateTextSize(computerText, 144, 288)); // Büyüt
            StartCoroutine(AnimateTextSize(playerText, 288, 144)); // Küçült
        }
    }

    IEnumerator AnimateTextSize(TextMeshProUGUI text, float fromSize, float toSize)
    {
        float duration = 0.5f; // Animasyon süresi
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            text.fontSize = Mathf.Lerp(fromSize, toSize, t);
            yield return null;
        }

        text.fontSize = toSize;
    }

    void ComputerMove()
    {
        // 1. Öncelik: Bilgisayar kazanıyor mu?
        if (TryToWinOrBlock(computerColor, 1)) return;

        // 2. Öncelik: Rakip kazanıyor mu?
        else if (TryToWinOrBlock(playerColor, 2)) return;

        // 3. Öncelik: Ömrü en uzun olan mavinin satır, sütün veya çaprazına koy
        else if (PlaceLongestLivedComputerButton()) return;

        // Eğer mümkün değilse, rastgele hamle yap
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < buttonList.Length; i++)
        {
            if (buttonList[i].text == "")
            {
                availableIndices.Add(i);
            }
        }

        if (availableIndices.Count > 0)
        {
            int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];
            buttonList[randomIndex].GetComponentInParent<ButtonHandler>().SetSpaceForComputer(computerSide, computerColor);
        }
        EndTurn();
    }

    bool TryToWinOrBlock(Color color, int priority)
    {
        for (int i = 0; i < buttonList.Length; i += 3)
        {
            if (CheckTwoInLine(color, i, i + 1, i + 2, priority)) return true; // Satır kontrolü
        }
        for (int i = 0; i < 3; i++)
        {
            if (CheckTwoInLine(color, i, i + 3, i + 6, priority)) return true; // Sütun kontrolü
        }
        if (CheckTwoInLine(color, 0, 4, 8, priority)) return true; // Çapraz kontrol
        if (CheckTwoInLine(color, 2, 4, 6, priority)) return true; // Çapraz kontrol

        return false;
    }

    bool CheckTwoInLine(Color color, int index1, int index2, int index3, int priority)
    {
        Color color1 = buttonList[index1].GetComponentInParent<ButtonHandler>().GetButtonColor();
        Color color2 = buttonList[index2].GetComponentInParent<ButtonHandler>().GetButtonColor();
        Color color3 = buttonList[index3].GetComponentInParent<ButtonHandler>().GetButtonColor();

        int lifetime1 = buttonList[index1].GetComponentInParent<ButtonHandler>().GetLifetime();
        int lifetime2 = buttonList[index2].GetComponentInParent<ButtonHandler>().GetLifetime();
        int lifetime3 = buttonList[index3].GetComponentInParent<ButtonHandler>().GetLifetime();

        if (color1 == color && color2 == color && buttonList[index3].text == "")
        {
            if (priority == 1 && (lifetime1 == 1 || lifetime2 == 1)) return false;
            if (priority == 2 && (lifetime1 == 2 || lifetime2 == 2)) return false;
            buttonList[index3].GetComponentInParent<ButtonHandler>().SetSpaceForComputer(computerSide, computerColor);
            EndTurn();
            return true;
        }
        if (color1 == color && color3 == color && buttonList[index2].text == "")
        {
            if (priority == 1 && (lifetime1 == 1 || lifetime3 == 1)) return false;
            if (priority == 2 && (lifetime1 == 2 || lifetime3 == 2)) return false;
            buttonList[index2].GetComponentInParent<ButtonHandler>().SetSpaceForComputer(computerSide, computerColor);
            EndTurn();
            return true;
        }
        if (color2 == color && color3 == color && buttonList[index1].text == "")
        {
            if (priority == 1 && (lifetime2 == 1 || lifetime3 == 1)) return false;
            if (priority == 2 && (lifetime2 == 2 || lifetime3 == 2)) return false;
            buttonList[index1].GetComponentInParent<ButtonHandler>().SetSpaceForComputer(computerSide, computerColor);
            EndTurn();
            return true;
        }
        return false;
    }

    bool PlaceLongestLivedComputerButton()
    {
        int maxLifetime = -1;
        int bestIndex = -1;

        for (int i = 0; i < buttonList.Length; i++)
        {
            ButtonHandler handler = buttonList[i].GetComponentInParent<ButtonHandler>();
            if (handler.GetButtonColor() == computerColor && handler.GetLifetime() > maxLifetime)
            {
                maxLifetime = handler.GetLifetime();
                bestIndex = i;
            }
        }

        if (bestIndex != -1)
        {
            int[] indices = GetAdjacentIndices(bestIndex);
            foreach (int index in indices)
            {
                if (buttonList[index].text == "")
                {
                    buttonList[index].GetComponentInParent<ButtonHandler>().SetSpaceForComputer(computerSide, computerColor);
                    EndTurn();
                    return true;
                }
            }
        }
        return false;
    }

    int[] GetAdjacentIndices(int index)
    {
        switch (index)
        {
            case 0: return new int[] { 1, 2, 3, 6, 4, 8 };
            case 1: return new int[] { 0, 2, 4, 7 };
            case 2: return new int[] { 0, 1, 5, 8, 4, 6 };
            case 3: return new int[] { 0, 6, 4, 5 };
            case 4: return new int[] { 0, 8, 2, 6, 1, 7, 3, 5 };
            case 5: return new int[] { 2, 8, 3, 4 };
            case 6: return new int[] { 0, 3, 7, 8, 2, 4 };
            case 7: return new int[] { 1, 4, 6, 8 };
            case 8: return new int[] { 0, 4, 2, 6, 5, 7 };
            default: return new int[0];
        }
    }
}