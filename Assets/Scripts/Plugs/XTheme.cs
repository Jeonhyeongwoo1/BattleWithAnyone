using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class XTheme : MonoBehaviour, IPlugable
{
    public string plugName => nameof(XTheme);

    [SerializeField] Text m_Master;
    [SerializeField] Text m_Player;
    [SerializeField] Text m_RoundTime;
    [SerializeField] Text m_MasterWinCount;
    [SerializeField] Text m_PlayerWinCount;

    public void SetGameInfo(string roundTime, string numberOfRound, string master, string player)
    {
        m_Master.text = master;
        m_Player.text = player;
        m_RoundTime.text = roundTime;
    }

    public void Open(UnityAction done = null)
    {
        gameObject.SetActive(true);
        done?.Invoke();
    }

    public void Close(UnityAction done = null)
    {
        gameObject.SetActive(false);
        done?.Invoke();
    }

    // Start is called before the first frame update
    void Start()
    {
        Core.Ensure(() => Core.plugs.Loaded(this));
    }

    private void OnDestroy()
    {
        Core.Ensure(() => Core.plugs.Unloaded(this));
    }

}
