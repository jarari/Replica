using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class CharacterTalk : MonoBehaviour {
    public GameObject m_text2D;

    private Text textComp;
    private StringBuilder sb;

    private void Awake() {
        textComp = m_text2D.GetComponent<Text>();
        sb = new StringBuilder();
    }

    public void ClearText() {
        sb.Clear();
        textComp.text = "";
    }

    public void AddText(char message) {
        sb.Append(message);
        textComp.text = sb.ToString();
    }

    public void AddText(string message) {
        sb.Append(message);
        textComp.text = sb.ToString();
    }

    public void SetText(string message) {
        sb.Clear();
        sb.Append(message);
        textComp.text = sb.ToString();
    }
}
