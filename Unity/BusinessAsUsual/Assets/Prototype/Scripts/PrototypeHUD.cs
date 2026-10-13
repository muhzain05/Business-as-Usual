using UnityEngine;

namespace BusinessAsUsual.Prototype
{
    public sealed class PrototypeHUD : MonoBehaviour
    {
        public PlayerInteractor player;
        public PrototypeInput input;
        GUIStyle title, text, prompt;
        void OnGUI()
        {
            if (text == null)
            {
                title = new GUIStyle(GUI.skin.label) { fontSize = 23, fontStyle = FontStyle.Bold };
                text = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };
                prompt = new GUIStyle(text) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
            }
            GUI.Box(new Rect(16, 16, 520, 108), "");
            GUI.Label(new Rect(30, 24, 490, 32), "BUSINESS AS USUAL  /  FIRST PLAYABLE", title);
            GUI.Label(new Rect(30, 61, 490, 52), "WASD move  •  Mouse look  •  Shift sprint  •  Space jump\nE interact / drop box  •  Esc release mouse  •  Click to resume", text);
            if (!player) return;
            string action = player.HeldBox ? "[E] Drop delivery box" : player.Current ? "[E] " + player.Current.Prompt : "Explore the shop, garage and sanitation yard";
            if (input && !input.GameplayActive) action = "Click the Game view to capture the mouse";
            GUI.Box(new Rect(Screen.width / 2f - 270, Screen.height - 84, 540, 56), "");
            GUI.Label(new Rect(Screen.width / 2f - 260, Screen.height - 78, 520, 42), action, prompt);
            if (!string.IsNullOrEmpty(player.Status))
                GUI.Label(new Rect(30, 136, 600, 52), player.Status, text);
            GUI.Label(new Rect(Screen.width / 2f - 6, Screen.height / 2f - 10, 20, 24), "+", text);
        }
    }
}
