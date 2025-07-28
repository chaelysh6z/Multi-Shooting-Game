using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Invoke 대신 코루틴을 사용하여 딜레이 호출을 쉽게 하기 위한 유틸리티 클래스
/// </summary>
public class CoroutineUtils
{
    /// <summary>
    /// 일정 시간 후 Action 실행
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