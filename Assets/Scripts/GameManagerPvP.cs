using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManagerPvP : MonoBehaviour
{
    public TextMeshProUGUI[] buttonList;
    public TextMeshProUGUI playerXScoreText;
    public TextMeshProUGUI playerOScoreText;
    public TextMeshProUGUI playerXText;
    public TextMeshProUGUI playerOText;
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

    private string playerSide;
    private string startingPlayerSide; // İlk başlayan oyuncuyu tutar
    private int moveCount;
    private Color playerColor;
    private int playerXScore;
    private int playerOScore;

    private ParticleSystem currentParticle;

    void Awake()
    {
        SetGameControllerReferenceOnButtons();
        startingPlayerSide = "X"; // Başlangıçta "X" başlar
        playerSide = startingPlayerSide;
        playerColor = Color.red;
        moveCount = 0;
        playerXScore = 0;
        playerOScore = 0;
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
        return playerSide;
    }

    public bool IsPlayerTurn()
    {
        return true;
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
            ChangeSides();
            UpdateTextColors();
        }
    }

    void DecreaseLifetimes()
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<ButtonHandler>().DecreaseLifetime();
        }
    }

    void ChangeSides()
    {
        if (playerSide == "X")
        {
            playerSide = "O";
            playerColor = Color.blue;
        }
        else
        {
            playerSide = "X";
            playerColor = Color.red;
        }
        Debug.Log("Player side changed to: " + playerSide);
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

        if (color1 == playerColor && color2 == playerColor && color3 == playerColor)
        {
            Color lineColor = (playerSide == "X") ? new Color(255f, 0, 0, 1f) : new Color(0, 0, 255f, 1f);
            Vector3 startPos = buttonList[index1].transform.position;
            Vector3 endPos = buttonList[index3].transform.position;

            // Çizginin başlangıç ve bitiş pozisyonlarını genişletme
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
        float duration = 1.8f; // Çizgi çizim animasyon süresi
        float shrinkDuration = 1.0f; // Çizgi küçülme animasyon süresi
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

        // Küçülerek yok olma animasyonu
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

        lineRenderer.enabled = false; // Çizgiyi pasif hale getir

        Destroy(currentParticle.gameObject, currentParticle.main.duration); // Partikül sistemini yok et
    }

    void GameOver()
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<Button>().interactable = false;
        }

        if (playerSide == "X")
        {
            playerXScore++;
            StartCoroutine(AnimateWinnerText(playerXText, Color.red));
        }
        else
        {
            playerOScore++;
            StartCoroutine(AnimateWinnerText(playerOText, Color.blue));
        }

        UpdateScoreTexts();
        Debug.Log("Game Over! " + playerSide + " wins!");

        if (playerXScore >= 3)
        {
            ShowWinPanel("X");
        }
        else if (playerOScore >= 3)
        {
            ShowWinPanel("O");
        }
        else
        {
            Invoke("ResetGame", 2f);
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
        // Ana menüye dön
        SceneManager.LoadScene(0);
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(2);
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
        Vector3 originalScaleX = playerXText.transform.localScale;
        Vector3 originalScaleO = playerOText.transform.localScale;
        Vector3 targetScale = originalScaleX * 2f;

        Vector3 fallDistance = new Vector3(0, -Screen.height / 2, 0);

        // İlk pozisyonları kaydet
        Vector3 originalPositionX = playerXText.transform.localPosition;
        Vector3 originalPositionO = playerOText.transform.localPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            playerXText.transform.localScale = Vector3.Lerp(originalScaleX, targetScale, t);
            playerOText.transform.localScale = Vector3.Lerp(originalScaleO, targetScale, t);

            playerXText.transform.localPosition = Vector3.Lerp(originalPositionX, originalPositionX + fallDistance, t);
            playerOText.transform.localPosition = Vector3.Lerp(originalPositionO, originalPositionO + fallDistance, t);

            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            playerXText.transform.localScale = Vector3.Lerp(targetScale, originalScaleX, t);
            playerOText.transform.localScale = Vector3.Lerp(targetScale, originalScaleO, t);

            playerXText.transform.localPosition = Vector3.Lerp(originalPositionX + fallDistance, originalPositionX, t);
            playerOText.transform.localPosition = Vector3.Lerp(originalPositionO + fallDistance, originalPositionO, t);

            yield return null;
        }

        playerXText.transform.localScale = originalScaleX;
        playerXText.transform.localPosition = originalPositionX;
        playerOText.transform.localScale = originalScaleO;
        playerOText.transform.localPosition = originalPositionO;
    }

    IEnumerator AnimateDrawCentralText()
    {
        float duration = 2f;
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

    void ResetGame()
    {
        ClearButtonTexts();
        lineRenderer.enabled = false;
        SwitchStartingPlayer();
        playerSide = startingPlayerSide;
        playerColor = (playerSide == "X") ? Color.red : Color.blue;
        UpdateTextColors();
        moveCount = 0;

        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<Button>().interactable = true;
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
        playerXScoreText.text = playerXScore.ToString();
        playerOScoreText.text = playerOScore.ToString();
    }

    void UpdateTextColors()
    {
        // Önce tüm yazıların renk ve boyutunu sıfırla
        playerXText.color = new Color(1, 0, 0, 0.5f);
        playerOText.color = new Color(0, 0, 1, 0.5f);
        playerXText.fontSize = 144;
        playerOText.fontSize = 144;

        if (playerSide == "X")
        {
            playerXText.color = new Color(1, 0, 0, 1);
            StartCoroutine(AnimateTextSize(playerXText, 144, 288));
            StartCoroutine(AnimateTextSize(playerOText, 288, 144));
        }
        else
        {
            playerOText.color = new Color(0, 0, 1, 1);
            StartCoroutine(AnimateTextSize(playerOText, 144, 288));
            StartCoroutine(AnimateTextSize(playerXText, 288, 144));
        }
    }

    IEnumerator AnimateTextSize(TextMeshProUGUI text, float fromSize, float toSize)
    {
        float duration = 0.5f;
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
}
