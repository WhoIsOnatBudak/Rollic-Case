using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    [Header("UI")]
    [SerializeField] private TMP_Text StartText;

    private void Start()
    {
        StartText.text = "Level " + PlayerPrefs.GetInt("CurrentLevel", 1);
        if(PlayerPrefs.GetInt("CurrentLevel", 1) > 10){
            StartText.text = "You Win!";
        }
    }

    public void OnStartClicked()
    {
        if(PlayerPrefs.GetInt("CurrentLevel", 1) > 10){return;}
        // Yükleme mantığını başlat, var olan sahneyi yeniden yükle
        // GameScene ismini değiştirebilirsiniz, şimdilik build index 1 varsayıyoruz
        // veya "GameScene" adında
        SceneManager.LoadScene(1);
    }

    

    public void OnResetClicked()
    {
        // Yükleme mantığını başlat, var olan sahneyi yeniden yükle
        // GameScene ismini değiştirebilirsiniz, şimdilik build index 1 varsayıyoruz
        // veya "GameScene" adında
        PlayerPrefs.SetInt("CurrentLevel", 1);
        PlayerPrefs.Save();
        StartText.text = "Level " + PlayerPrefs.GetInt("CurrentLevel", 1);
    }
}
