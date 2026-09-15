using HeadbangHeroes.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace HeadbangHeroes.UI
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        [SerializeField] Text scoreText;
        [SerializeField] Text comboText;
        [SerializeField] Text judgmentText;

        public void ResetHud()
        {
            if (scoreText != null) scoreText.text = "0";
            if (comboText != null) comboText.text = "x0";
            if (judgmentText != null) judgmentText.text = "";
        }

        public void Show(JudgmentResult result, int combo, long score)
        {
            if (scoreText != null) scoreText.text = score.ToString("N0");
            if (comboText != null) comboText.text = $"x{combo}";
            if (judgmentText != null)
            {
                var ms = result.error * 1000.0;
                judgmentText.text = $"{result.judgment}\n{ms:+0;-0;0} ms";
            }
        }

        public void ShowMiss(int combo, long score)
        {
            if (scoreText != null) scoreText.text = score.ToString("N0");
            if (comboText != null) comboText.text = $"x{combo}";
            if (judgmentText != null) judgmentText.text = "MISS";
        }
    }
}
