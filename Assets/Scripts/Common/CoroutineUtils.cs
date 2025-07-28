using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Invoke ��� �ڷ�ƾ�� ����Ͽ� ������ ȣ���� ���� �ϱ� ���� ��ƿ��Ƽ Ŭ����
/// </summary>
public class CoroutineUtils
{
    /// <summary>
    /// ���� �ð� �� Action ����
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="delay"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static Coroutine DelayCall(MonoBehaviour owner, float delay, Action action)
    {
        return owner.StartCoroutine(DelayRoutine(delay, action));
    }

    private static IEnumerator DelayRoutine(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
}