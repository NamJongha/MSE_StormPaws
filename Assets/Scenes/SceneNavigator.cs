using UnityEngine;
using UnityEngine.SceneManagement; // Scene ������ ���� �ʿ�

public class SceneNavigator : MonoBehaviour
{
    // Mypage ������ �̵��ϴ� �Լ�
    public void GoToMyPageScene()
    {
        // "mypage"�� ���� �� ������ �̸��� ��Ȯ�� ��ġ�ؾ� �մϴ� (Ȯ���� .unity ����)
        SceneManager.LoadScene("MyPage");
        Debug.Log("���������� ������ �̵� �õ�..."); // Ȯ�ο� �α�
    }

    // Battle ������ �̵��ϴ� �Լ�
    public void GoToBattleScene()
    {
        // "battle"�� ���� �� ������ �̸��� ��Ȯ�� ��ġ�ؾ� �մϴ� (Ȯ���� .unity ����)
        SceneManager.LoadScene("Battle");
        Debug.Log("��Ʋ ������ �̵� �õ�..."); // Ȯ�ο� �α�
    }
}
