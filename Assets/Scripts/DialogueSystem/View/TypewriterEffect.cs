using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using FinalRPG.Utils;

/// <summary>
/// 打字机效果 — 用 DOTween 驱动 UI Toolkit Label 逐字显示。
/// 挂在 DialogueUIController 同 GameObject 上，由 DialogueUIController 注入目标 Label。
/// 支持富文本标签感知（跳过 &lt;tag&gt; 内字符）、点击跳过、速度配置。
/// </summary>
public class TypewriterEffect : MonoBehaviour
{
    [Header("Speed")]
    [Tooltip("每秒显示字符数")]
    [SerializeField] private float _charsPerSecond = 30f;

    private Label _label;
    private Tween _tween;
    private string _fullText;
    private int[] _visibleCharMap; // 可见字符索引 → 原字符串索引
    private Action _onComplete;
    private bool _isPlaying;

    public bool IsPlaying => _isPlaying;

    /// <summary>
    /// 注入目标 Label（由 DialogueUIController 在 Awake 中调用）。
    /// </summary>
    public void SetLabel(Label label)
    {
        _label = label;
    }

    /// <summary>
    /// 开始逐字播放文本。
    /// </summary>
    /// <param name="fullText">完整台词（可含富文本标签）</param>
    /// <param name="onComplete">打字机完成后回调</param>
    public void Play(string fullText, Action onComplete)
    {
        // 杀死旧 tween，防止多个动画竞争
        _tween?.Kill();

        if (_label == null)
        {
            RPGLog.Error("Dialogue", "TypewriterEffect.Play: _label 为 null，请先调用 SetLabel()");
            onComplete?.Invoke();
            return;
        }

        _fullText = fullText;
        _onComplete = onComplete;

        // 空文本直接完成
        if (string.IsNullOrEmpty(fullText))
        {
            _label.text = fullText ?? string.Empty;
            _isPlaying = false;
            _onComplete?.Invoke();
            return;
        }

        BuildVisibleCharMap(fullText);
        int totalVisibleChars = _visibleCharMap.Length;

        if (totalVisibleChars == 0)
        {
            // 纯标签无可见字符
            _label.text = fullText;
            _isPlaying = false;
            _onComplete?.Invoke();
            return;
        }

        _label.text = string.Empty;
        _isPlaying = true;

        float duration = totalVisibleChars / _charsPerSecond;

        _tween = DOTween
            .To(
                getter: () => 0f,
                setter: val => UpdateVisibleText(Mathf.RoundToInt(val)),
                endValue: (float)totalVisibleChars,
                duration: duration
            )
            .SetEase(Ease.Linear)
            .SetUpdate(true) // 使用 unscaled time，与项目 DOTween 惯例一致
            .OnComplete(OnTweenComplete);
    }

    /// <summary>
    /// 立即显示全文并触发完成回调（跳过打字机动画）。
    /// </summary>
    public void Skip()
    {
        if (!_isPlaying) return;

        _tween?.Kill();
        _tween = null;

        _label.text = _fullText;
        _isPlaying = false;

        var cb = _onComplete;
        _onComplete = null;
        cb?.Invoke();
    }

    /// <summary>
    /// 停止打字机并清理 tween（用于对话结束/场景切换）。
    /// </summary>
    public void Stop()
    {
        _tween?.Kill();
        _tween = null;
        _isPlaying = false;
        _onComplete = null;
    }

    private void OnDestroy()
    {
        Stop();
    }

    private void OnDisable()
    {
        Stop();
    }

    /// <summary>
    /// 构建可见字符索引映射表。跳过 &lt;tag&gt; 内的字符。
    /// </summary>
    private void BuildVisibleCharMap(string text)
    {
        var map = new List<int>();
        bool inTag = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (c == '<')
            {
                inTag = true;
                continue;
            }

            if (c == '>')
            {
                inTag = false;
                continue;
            }

            if (!inTag)
            {
                map.Add(i);
            }
        }

        _visibleCharMap = map.ToArray();
    }

    /// <summary>
    /// DOTween OnUpdate 回调：根据当前可见字符数量截取并显示文本。
    /// </summary>
    private void UpdateVisibleText(int visibleCharCount)
    {
        if (_label == null) return;

        visibleCharCount = Mathf.Clamp(visibleCharCount, 0, _visibleCharMap.Length);

        if (visibleCharCount <= 0)
        {
            _label.text = string.Empty;
            return;
        }

        // 第 visibleCharCount 个可见字符在原字符串中的索引 + 1 即截取长度
        int strIndex = _visibleCharMap[visibleCharCount - 1];
        _label.text = _fullText.Substring(0, strIndex + 1);
    }

    /// <summary>
    /// DOTween OnComplete 回调。
    /// </summary>
    private void OnTweenComplete()
    {
        // 确保最终文本完整显示
        _label.text = _fullText;
        _tween = null;
        _isPlaying = false;

        var cb = _onComplete;
        _onComplete = null;
        cb?.Invoke();
    }
}
